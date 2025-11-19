using Saht.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyDictionary<Guid, User>> GetUsersByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default );
        Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);// isme göre kullanıcı getir
        Task<IReadOnlyList<Guid>> GetUserIdsByUsernamesAsync(IReadOnlyList<string> usernames, CancellationToken ct = default);
        Task<bool> ExistsByUserNameOrEmailAsync(string userName, string email, CancellationToken ct = default);
        Task<User?> GetByUserNameOrEmailAsync(string userNameOrEmail, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
