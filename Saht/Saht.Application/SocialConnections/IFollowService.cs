using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.SocialConnections
{
    public sealed record UserBriefDto(Guid Id, string UserName, string DisplayName);
    public interface IFollowService
    {
        Task FollowAsync(Guid me, Guid target, CancellationToken ct = default);
        Task UnfollowAsync(Guid me, Guid target, CancellationToken ct = default);
        Task<bool> IsFollowingAsync(Guid me, Guid target, CancellationToken ct = default);
        Task<(int followers, int following)> CountsAsync(Guid userId, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid me, int page, int size, CancellationToken ct = default);

        // UI için DTO’lu listeler + total
        Task<(IReadOnlyList<UserBriefDto> Items, int Total)> GetFollowersAsync(Guid userId, int page, int size, CancellationToken ct = default);
        Task<(IReadOnlyList<UserBriefDto> Items, int Total)> GetFollowingAsync(Guid userId, int page, int size, CancellationToken ct = default);


    }
}
