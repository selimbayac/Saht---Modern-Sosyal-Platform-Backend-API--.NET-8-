using Microsoft.EntityFrameworkCore;
using Saht.Application.Abstractions;
using Saht.Domain.Users;
using Saht.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure.Repositories
{
    public sealed class EfUserRepository : IUserRepository
    {
        private readonly SahtDbContext _db;
        public EfUserRepository(SahtDbContext db) => _db = db;

        public Task<bool> ExistsByUserNameOrEmailAsync(string userName, string email, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.UserName == userName || u.Email == email, ct);

        public Task<User?> GetByUserNameOrEmailAsync(string k, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.UserName == k || u.Email == k, ct);

        public Task AddAsync(User user, CancellationToken ct = default)
        { _db.Users.Add(user); return Task.CompletedTask; }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default) =>
      _db.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct);

        public async Task<IReadOnlyList<Guid>> GetUserIdsByUsernamesAsync(IReadOnlyList<string> usernames, CancellationToken ct = default)
        {
            // Veri tabanında, verilen kullanıcı adlarına karşılık gelen User.Id alanlarını seç.
            var userIds = await _db.Users
                .AsNoTracking() // Sadece ID'leri aldığımız için takip etmeye gerek yok
                .Where(u => usernames.Contains(u.UserName)) // Kullanıcı adları listesi içinde olanları filtrele
                .Select(u => u.Id) // Sadece ID'leri seç
                .ToListAsync(ct);

            return userIds;
        }
        public async Task<IReadOnlyDictionary<Guid, User>> GetUsersByIdsAsync( IReadOnlyList<Guid> ids, CancellationToken ct = default)
        {
            var users = await _db.Users .Where(u => ids.Contains(u.Id)).ToListAsync(ct);
            return users.ToDictionary(u => u.Id, u => u);
        }
    }
}
