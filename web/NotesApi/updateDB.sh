dotnet restore
dotnet ef migrations add UpdateAddRoleToUser
dotnet ef database update
dotnet run