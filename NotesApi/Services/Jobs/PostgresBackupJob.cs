using Quartz;
using System.Diagnostics;

namespace NotesApi.Services.Jobs
{
    public class PostgresBackupJob : IJob
    {
        private readonly ILogger<PostgresBackupJob> _logger;

        public PostgresBackupJob(ILogger<PostgresBackupJob> logger)
        {
            _logger = logger;
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            var backupDir = Environment.GetEnvironmentVariable("POSTGRES_DATA_BACKUP_PATH") ?? "/app/backups";
            var host = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_HOST_IP") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_PORT") ?? "5432";
            var user = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_USER") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
            var dbName = Environment.GetEnvironmentVariable("DOTNET_POSTGRES_DB") ?? "notesdb_dev";

            var filePath = Path.Combine(backupDir, $"backup_{timestamp}.dump");

            var startInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
                Arguments = $"-h {host} -p {port} -U {user} -F c -b -v -f \"{filePath}\" {dbName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.Environment["PGPASSWORD"] = password;

            try
            {
                Directory.CreateDirectory(backupDir);
                using var process = new Process { StartInfo = startInfo };
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                _logger.LogInformation($"[Quartz] Backup created: {filePath}");
                if (!string.IsNullOrWhiteSpace(error))
                    _logger.LogError($"[Quartz ERROR] {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Quartz EXCEPTION] {ex.Message}");
            }
        }
    }
}
