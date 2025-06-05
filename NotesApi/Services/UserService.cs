using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotesApi.Models;
using NotesApi.Repositories.IRepositories;
using NotesApi.Services.IService;

namespace NotesApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public UserService(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _unitOfWork.Users.GetUserByUsernameAsync(username);
        }

        public async Task CreateUserAsync(User user)
        {
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user != null)
            {
                _unitOfWork.Users.Remove(user);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            var exists = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => u.Username == registerDto.Username, tracked: false);

            if (exists != null)
                return false;

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => u.Username == dto.Username, includeProperties: "RefreshTokens");

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            await _unitOfWork.SaveAsync();

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponseDto?> RefreshAsync(TokenResponseDto tokenDto)
        {
            var user = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => 
                    u.RefreshTokens.Any(rt => rt.Token == tokenDto.RefreshToken),
                    includeProperties: "RefreshTokens");

            if (user == null) return null;

            var refreshToken = user.RefreshTokens.First(rt => rt.Token == tokenDto.RefreshToken);

            if (refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
                return null;

            // Revoke old token
            refreshToken.IsRevoked = true;

            // Generate new token
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            await _unitOfWork.SaveAsync();

            var newAccessToken = _jwtService.GenerateAccessToken(user);

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}