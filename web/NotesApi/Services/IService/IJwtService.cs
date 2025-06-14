using NotesApi.Models;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool CheckTokenValidity(string token);
}