dotnet restore
dotnet ef migrations add UpdateUserTable
dotnet ef database update
dotnet run