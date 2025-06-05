using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotesApi.Models;
using NotesApi.Repositories.IRepository;

namespace NotesApi.Repositories.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        void Update(User obj);
        bool IsExists(string includeProperties, string value);
    }
}