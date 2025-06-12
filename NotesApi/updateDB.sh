dotnet restore
dotnet ef migrations add UpdateSchemaSetting
dotnet ef database update
dotnet run