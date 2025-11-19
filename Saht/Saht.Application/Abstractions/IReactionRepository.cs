using Saht.Domain.Common;
using Saht.Domain.Reactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface IReactionRepository
    {
        Task<Reaction?> GetAsync(ContentType type, long targetId, Guid userId, CancellationToken ct = default);
        Task UpsertAsync(Reaction reaction, CancellationToken ct = default); // insert or update
        Task RemoveAsync(ContentType type, long targetId, Guid userId, CancellationToken ct = default);
        Task<(int likes, int dislikes)> CountAsync(ContentType type, long targetId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        Task<(IReadOnlyList<(Guid userId, string userName, string displayName, int value, DateTime at)>, int total)>
        GetReactorsAsync(ContentType type, long targetId, int? value, int skip, int take, CancellationToken ct);


    }
}
