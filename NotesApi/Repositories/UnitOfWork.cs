using NotesApi.Data;
using NotesApi.Repositories;
using NotesApi.Repositories.IRepositories;

namespace NotesApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NotesContext _context;

        public UnitOfWork(NotesContext context)
        {
            _context = context;
            Notes = new NoteRepository(context);
            Users = new UserRepository(context);
        }

        public INoteRepository Notes { get; private set; }
        public IUserRepository Users { get; private set; }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}