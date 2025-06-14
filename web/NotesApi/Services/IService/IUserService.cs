using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotesApi.Models;

namespace NotesApi.Services.IService
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<TokenResponseDto?> LoginAsync(LoginDto dto);
        Task<TokenResponseDto?> RefreshAsync(TokenResponseDto tokenDto);
        bool CheckTokenValidity(string token);
    }
}