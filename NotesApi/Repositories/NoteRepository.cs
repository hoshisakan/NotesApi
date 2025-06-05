using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;
using NotesApi.Repositories.IRepositories;
using NotesApi.Repositories.Repositories;

public class NoteRepository : Repository<Note>, INoteRepository
{
    private readonly NotesContext _context;

    public NoteRepository(NotesContext context) : base (context)
    {
        _context = context;
    }

    public async Task<Note?> GetByIdAsync(int id) =>
        await _context.Notes.FindAsync(id);

    public void Update(Note obj)
    {
        _context.Notes.Update(obj);
    }

    public bool IsExists(string includeProperties, string value)
    {
        if (includeProperties == "Title")
        {
            return _context.Notes.Any(u => u.Title == value);
        }
        else if (includeProperties == "Content")
        {
            return _context.Notes.Any(u => u.Content == value);
        }
        else
        {
            return false;
        }
    }
}