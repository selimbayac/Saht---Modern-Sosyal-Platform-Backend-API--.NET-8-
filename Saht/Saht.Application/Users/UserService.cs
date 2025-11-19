using Saht.Application.Abstractions;
using Saht.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Users
{
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public UserService(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<UserDto?> GetPublicAsync(Guid id, CancellationToken ct = default)
        {
            var u = await _users.GetByIdAsync(id, ct);
            if (u is null) return null;

            return new UserDto(u.Id, u.UserName, u.DisplayName, u.Bio, u.AvatarUrl, u.CreatedAt);
        }

        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileCommand cmd, CancellationToken ct = default)
        {
            var u = await _users.GetByIdAsync(userId, ct) ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");
            u.SetDisplayName(cmd.DisplayName);
            u.SetProfile(cmd.Bio, cmd.AvatarUrl);
            await _users.SaveChangesAsync(ct);
            return new UserDto(u.Id, u.UserName, u.DisplayName, u.Bio, u.AvatarUrl, u.CreatedAt);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordCommand cmd, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(cmd.NewPassword) || cmd.NewPassword.Length < 6)
                throw new ArgumentException("Yeni şifre en az 6 karakter olmalı.");

            var u = await _users.GetByIdAsync(userId, ct) ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");
            if (!_hasher.Verify(cmd.OldPassword, u.PasswordHash))
                throw new UnauthorizedAccessException("Eski şifre hatalı.");

            u.SetPasswordHash(_hasher.Hash(cmd.NewPassword));
            await _users.SaveChangesAsync(ct);
        }
        public async Task<UserDto?> GetPublicByUserNameAsync(string userName, CancellationToken ct = default)
        {
            var u = await _users.GetByUserNameAsync(userName, ct);
            return u is null ? null : new UserDto(u.Id, u.UserName, u.DisplayName, u.Bio, u.AvatarUrl, u.CreatedAt);
        }

    

    }
}
