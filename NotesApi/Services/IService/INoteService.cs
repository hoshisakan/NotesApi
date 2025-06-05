using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotesApi.Models;

namespace NotesApi.Services.IService
{
    public interface INoteService
    {
        Task<List<Note>> GetAllNotesAsync();
        Task<Note?> GetNoteByIdAsync(int id);
        Task CreateNoteAsync(Note note);
        Task<bool> UpdateNoteAsync(int userId, Note note);
        Task<bool> DeleteNoteAsync(int id, int userId);
    }
}