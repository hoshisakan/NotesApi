using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotesApi.Models;
using NotesApi.Repositories.IRepository;

namespace NotesApi.Repositories.IRepositories
{
    public interface INoteRepository : IRepository<Note>
    {
        Task<Note?> GetByIdAsync(int id);
        void Update(Note obj);
        bool IsExists(string includeProperties, string value);
    }
}