using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Users
{
    public sealed class User : AggregateRoot<Guid>
    {
        private User() { } // EF için boş ctor

        public string UserName { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string DisplayName { get; private set; } = null!;
        public string? Bio { get; private set; }
        public string? AvatarUrl { get; private set; }
        public UserRole Role { get; private set; } = UserRole.User;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public bool IsBanned { get; private set; } = false;

        public static User Create(string userName, string email, string displayName, string passwordHash)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                Email = email,
                DisplayName = displayName,
                PasswordHash = passwordHash
            };
        }

        public void SetBio(string bio) => Bio = bio;
        public void SetAvatar(string url) => AvatarUrl = url;
        public void SetRole(UserRole role) => Role = role;
        public void Ban() => IsBanned = true;
        public void Unban() => IsBanned = false;

        public void SetDisplayName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length is < 2 or > 64)
                throw new ArgumentException("DisplayName 2-64 karakter olmalı.");
            DisplayName = name;
        }
        public void SetProfile(string? bio, string? avatarUrl)
        {
            Bio = bio;
            AvatarUrl = avatarUrl;
        }
        public void SetPasswordHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Geçersiz şifre özeti.");
            PasswordHash = hash;
        }
    }
}
