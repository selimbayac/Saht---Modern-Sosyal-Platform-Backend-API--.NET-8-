using Saht.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Users
{
    public sealed record UserDto(Guid Id, string UserName, string DisplayName, string? Bio, string? AvatarUrl, DateTime CreatedAt);
    public sealed record UpdateProfileCommand(string DisplayName, string? Bio, string? AvatarUrl);
    public sealed record ChangePasswordCommand(string OldPassword, string NewPassword);

    public interface IUserService
    {
        Task<UserDto?> GetPublicAsync(Guid id, CancellationToken ct = default);
        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileCommand cmd, CancellationToken ct = default);
        Task ChangePasswordAsync(Guid userId, ChangePasswordCommand cmd, CancellationToken ct = default);
        Task<UserDto?> GetPublicByUserNameAsync(string userName, CancellationToken ct = default);

    }
}
