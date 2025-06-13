dotnet restore
dotnet ef migrations add UpdateTestDatabaseAndSchemaSetting
dotnet ef database update
dotnet run