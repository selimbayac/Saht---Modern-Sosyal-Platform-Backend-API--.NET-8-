using Saht.Application.Abstractions;
using Saht.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Auth
{
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtProvider _jwt;

        public AuthService(IUserRepository users, IPasswordHasher hasher, IJwtProvider jwt)
        {
            _users = users; _hasher = hasher; _jwt = jwt;
        }

        public async Task<AuthResult> RegisterAsync(RegisterCommand cmd, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(cmd.Password) || cmd.Password.Length < 6)
                throw new ArgumentException("Şifre en az 6 karakter olmalı.");

            if (await _users.ExistsByUserNameOrEmailAsync(cmd.UserName, cmd.Email, ct))
                throw new InvalidOperationException("Kullanıcı adı veya e-posta kullanımda.");

            var hash = _hasher.Hash(cmd.Password);
            var user = User.Create(cmd.UserName, cmd.Email, cmd.DisplayName, hash);

            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct);

            return new AuthResult(_jwt.Create(user));
        }

        public async Task<AuthResult> LoginAsync(LoginCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByUserNameOrEmailAsync(cmd.UserNameOrEmail, ct);
            if (user is null || !_hasher.Verify(cmd.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Kullanıcı adı veya şifre hatalı.");

            return new AuthResult(_jwt.Create(user));
        }
    }
}