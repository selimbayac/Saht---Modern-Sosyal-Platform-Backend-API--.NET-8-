using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Auth
{
    public sealed record RegisterCommand(string UserName, string Email, string DisplayName, string Password);
    public sealed record LoginCommand(string UserNameOrEmail, string Password);
    public sealed record AuthResult(string Token);

    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterCommand cmd, CancellationToken ct = default);
        Task<AuthResult> LoginAsync(LoginCommand cmd, CancellationToken ct = default);
    }
}
