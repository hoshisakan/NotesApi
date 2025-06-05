using Microsoft.EntityFrameworkCore;
using NotesApi.Models;
using NotesApi.Repositories;
using NotesApi.Repositories.IRepositories;
using NotesApi.Services.IService;

namespace NotesApi.Services
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NoteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            return await _unitOfWork.Notes.GetAllAsync();
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            return await _unitOfWork.Notes.GetByIdAsync(id);
        }

        public async Task CreateNoteAsync(Note note)
        {
            await _unitOfWork.Notes.AddAsync(note);
            await _unitOfWork.SaveAsync();
        }

        public async Task<bool> UpdateNoteAsync(int userId, Note updatedNote)
        {
            var note = await _unitOfWork.Notes.GetFirstOrDefaultAsync(
                n => n.Id == updatedNote.Id && n.UserId == userId);

            if (note == null)
                return false;

            note.Title = updatedNote.Title;
            note.Content = updatedNote.Content;

            try
            {
                _unitOfWork.Notes.Update(note);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _unitOfWork.Notes.GetFirstOrDefaultAsync(
                    n => n.Id == updatedNote.Id && n.UserId == userId) == null)
                    return false;
                else
                    throw;
            }
        }

        public async Task<bool> DeleteNoteAsync(int id, int userId)
        {
            var note = await _unitOfWork.Notes.GetFirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (note == null) return false;

            _unitOfWork.Notes.Remove(note);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}