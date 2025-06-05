using NotesApi.Models;

namespace NotesApi.Repositories.IRepositories
{
    public interface IUnitOfWork
    {
        INoteRepository Notes { get; }
        IUserRepository Users { get; }
        Task SaveAsync();
    }
}