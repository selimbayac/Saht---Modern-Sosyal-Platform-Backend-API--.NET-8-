using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Comments
{
    public interface ICommentService
    {
        Task<CommentDto> CreateOnPostAsync(Guid userId, CreateOnPostCommand cmd, CancellationToken ct = default);
        Task<CommentDto> ReplyAsync(Guid userId, ReplyCommentCommand cmd, CancellationToken ct = default);
        Task<CommentDto> EditAsync(Guid userId, EditCommentCommand cmd, CancellationToken ct = default);
        Task DeleteAsync(Guid userId, long commentId, bool isModerator = false, CancellationToken ct = default);

        // Viewer'ı isteyerek alıyoruz ki "my" alanı doldurulsun
        Task<CommentDto?> GetByIdAsync(long id, Guid viewerOrEmpty, CancellationToken ct = default);

        // Generic listeleme (şimdilik Post için kullanacaksın)
        Task<Paged<CommentDto>> ListForTargetAsync(
            ContentType targetType,
            long targetId,
            int page,
            int size,
            Guid viewerOrEmpty,
            CancellationToken ct = default
        );
        Task<CommentDto> CreateOnTargetAsync(Guid userId, ContentType targetType, long targetId, string body, CancellationToken ct = default);
     

    }
}
