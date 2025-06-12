using Microsoft.EntityFrameworkCore;
using NotesApi.Models;


namespace NotesApi.Data;

public class NotesContext : DbContext
{
    public NotesContext(DbContextOptions<NotesContext> options) : base(options) { }

    public DbSet<Note> Notes => Set<Note>();
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dev");

        base.OnModelCreating(modelBuilder);
    }
}
