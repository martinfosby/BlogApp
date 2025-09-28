using BlogApp.Models;
using BlogApp.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlogApp.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string username, string password);
        Task<User> LoginAsync(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository users, IConfiguration config)
        {
            _users = users;
            _config = config;
        }

        public async Task<User> RegisterAsync(string username, string password)
        {
            var existing = await _users.GetByUsernameAsync(username);
            if (existing != null)
                throw new Exception("User already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = HashPassword(password)
            };

            await _users.AddAsync(user);
            return user;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _users.GetByUsernameAsync(username);
            if (user == null) return null;
            if (!VerifyHash(password, user.PasswordHash)) return null;

            return user;
        }

        // simple salted PBKDF2
        private static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000))
            {
                var hash = pbkdf2.GetBytes(20);
                var hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static bool VerifyHash(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000))
            {
                var hash = pbkdf2.GetBytes(20);
                for (int i = 0; i < 20; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                }
            }

            return true;
        }
    }
}
