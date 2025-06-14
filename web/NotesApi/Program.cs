using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotesApi.Data;
using NotesApi.Repositories;
using NotesApi.Repositories.IRepositories;
using NotesApi.Services;
using NotesApi.Services.IService;
using NotesApi.Services.Jobs;
using Persistence.DbInitializer;
using Quartz;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

// 設定 Serilog，每小時一個 log 檔案
// Log.Logger = new LoggerConfiguration()
//     .Enrich.FromLogContext()
//     .WriteTo.Console()
//     .WriteTo.File("Logs/log-.txt", 
//         rollingInterval: RollingInterval.Hour,              // ✅ 每小時一個檔案
//         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
//     .CreateLogger();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// 加入 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:3000") // React 開發伺服器
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000); // 監聽 0.0.0.0:5000
    serverOptions.ListenAnyIP(5005, listenOptions =>
    {
        listenOptions.UseHttps(); // 使用 HTTPS
    });
});

builder.Services.AddDbContext<NotesContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey("PostgresBackupJob");

    q.AddJob<PostgresBackupJob>(opts => opts.WithIdentity(jobKey));

    // q.AddTrigger(t => t
    //     .ForJob(jobKey)
    //     .WithIdentity("PostgresBackupJob-trigger")
    //     .WithCronSchedule("0 * * ? * *")); // ⏱️ 每分鐘執行一次

    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithIdentity("PostgresBackupJob-trigger")
        .WithCronSchedule("0 0/30 * ? * *")); // 每半小時執行一次
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;  // 可視需求調整
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 加入 JWT 驗證
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
// Repository + UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Service 層
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Notes API", Version = "v1" });

    // 加入 JWT Bearer 認證設定
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "請輸入 JWT Token（格式：Bearer {your token})",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    };

    options.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

app.UseCors("AllowReactApp");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    // 自動建立資料庫和套用 Migrations
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NotesContext>();
        dbContext.Database.Migrate(); // ← 核心語句：會自動套用 Migrations 中的所有變更
    }
}
catch (Exception ex)
{
    Log.Error($"An error occurred while migrating or seeding the database or initializing the static file path: {ex.Message}");
}

app.Run();

public partial class Program { }