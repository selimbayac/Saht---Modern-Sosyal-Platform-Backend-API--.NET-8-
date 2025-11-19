using Saht.Application.Abstractions;
using Saht.Domain.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.SocialConnections
{
    public sealed class PrivacyService : IPrivacyService
    {
        private readonly IBlockRepository _blocks;
        private readonly IMuteRepository _mutes;
        private readonly IFollowRepository _follows;

        public PrivacyService(IBlockRepository blocks, IMuteRepository mutes, IFollowRepository follows)
        { _blocks = blocks; _mutes = mutes; _follows = follows; }

        public async Task BlockAsync(Guid me, Guid target, CancellationToken ct = default)
        {
            if (me == target) throw new InvalidOperationException("Kendini engelleyemezsin");
            if (await _blocks.ExistsAsync(me, target, ct)) return;

            // varsa follow’u kopar
            await _follows.RemoveAsync(me, target, ct);
            await _follows.RemoveAsync(target, me, ct);

            await _blocks.AddAsync(Block.Create(me, target), ct);
            await _blocks.SaveChangesAsync(ct);
        }

        public async Task UnblockAsync(Guid me, Guid target, CancellationToken ct = default)
        {
            await _blocks.RemoveAsync(me, target, ct);
            await _blocks.SaveChangesAsync(ct);
        }

        public Task<bool> IsBlockedEitherWayAsync(Guid a, Guid b, CancellationToken ct = default)
            => _blocks.IsBlockedEitherWayAsync(a, b, ct);

        public async Task MuteAsync(Guid me, Guid target, CancellationToken ct = default)
        {
            if (me == target) throw new InvalidOperationException("Kendini susturamazsın");
            if (await _mutes.ExistsAsync(me, target, ct)) return;
            await _mutes.AddAsync(Mute.Create(me, target), ct);
            await _mutes.SaveChangesAsync(ct);
        }

        public async Task UnmuteAsync(Guid me, Guid target, CancellationToken ct = default)
        {
            await _mutes.RemoveAsync(me, target, ct);
            await _mutes.SaveChangesAsync(ct);
        }

        public Task<IReadOnlyList<Guid>> GetMutedAsync(Guid me, CancellationToken ct = default)
            => _mutes.GetMutedIdsAsync(me, ct);

        public Task<IReadOnlyList<Guid>> GetBlockedAsync(Guid me, CancellationToken ct = default)
            => _blocks.GetBlockedIdsAsync(me, ct);
        public Task<IReadOnlyList<Guid>> GetBlockedIdsAsync(Guid viewerId, CancellationToken ct = default)
        {
            // Bu metot, sizin engellediğiniz kişileri ve sizi engelleyen kişileri birleştirir.
            // Bu, repository'de tanımlanan özel bir metot olmalıdır.
            return _blocks.GetBlockedEitherWayIdsAsync(viewerId, ct);
        }
    }
}
