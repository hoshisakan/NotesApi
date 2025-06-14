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
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _logger = logger;
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
            if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
            {
                _logger.LogWarning("Login attempt with empty username or password.");
                return null;
            }
            else
            {
                _logger.LogInformation($"Login attempt for user: {dto.Username}");
            }
            var user = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u => u.Username == dto.Username, includeProperties: "RefreshTokens");

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            _logger.LogInformation($"User {dto.Username} logged in successfully.");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            _logger.LogInformation($"Generated access token and refresh token for user {dto.Username}.");

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            _logger.LogInformation($"Added refresh token for user {dto.Username}.");

            await _unitOfWork.SaveAsync();

            _logger.LogInformation($"Saved user {dto.Username} with new refresh token.");

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponseDto?> RefreshAsync(TokenResponseDto tokenDto)
        {
            _logger.LogInformation($"Attempting to refresh token: {tokenDto.RefreshToken}", tokenDto.RefreshToken);
            _logger.LogInformation($"Attempting to access token: {tokenDto.AccessToken}", tokenDto.AccessToken);
            if (string.IsNullOrEmpty(tokenDto.RefreshToken))
            {
                _logger.LogWarning("Refresh token is null or empty.");
                return null;
            }
            _logger.LogInformation("Fetching user with refresh token.");
            // Find user by refresh token
            var user = await _unitOfWork.Users
                .GetFirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(rt => rt.Token == tokenDto.RefreshToken),
                    includeProperties: "RefreshTokens");

            _logger.LogInformation($"User fetched: {user?.Username}", user?.Username);
            // If user not found or no refresh tokens, return null
            if (user == null) return null;

            var refreshToken = user.RefreshTokens.First(rt => rt.Token == tokenDto.RefreshToken);

            _logger.LogInformation($"Refresh token found for user {user.Username}: {refreshToken.Token}");

            bool IsRevoked = refreshToken.IsRevoked;
            bool IsExpired = refreshToken.ExpiresAt < DateTime.UtcNow;
            _logger.LogInformation($"Refresh token status for user {user.Username} - IsRevoked: {IsRevoked}, IsExpired: {IsExpired}");


            if (IsRevoked || IsExpired)
            {
                _logger.LogWarning($"Refresh token expired or revoked for user {user.Username}: {refreshToken.Token}");
                return null;
            }

            // Revoke old token
                refreshToken.IsRevoked = true;

            _logger.LogInformation($"Revoked old refresh token for user {user.Username}: {refreshToken.Token}");

            // Generate new token
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            _logger.LogInformation($"Generated new refresh token for user {user.Username}: {newRefreshToken}");

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

        public bool CheckTokenValidity(string token)
        {
            return _jwtService.CheckTokenValidity(token);
        }
    }
}