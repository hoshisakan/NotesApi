namespace NotesApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}