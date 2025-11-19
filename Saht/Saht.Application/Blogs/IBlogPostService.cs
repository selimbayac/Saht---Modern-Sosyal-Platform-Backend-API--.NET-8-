using Saht.Application.Comments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Blogs
{
    public interface IBlogPostService
    {
        Task<BlogPostDto> CreateAsync(Guid authorId, CreateBlogPostCommand cmd, CancellationToken ct = default);
        Task<BlogPostDto?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<IReadOnlyList<BlogPostDto>> ListByUserAsync(Guid userId, int page, int size, CancellationToken ct = default);
        Task<IReadOnlyList<BlogPostDto>> ListRecentAsync(int page, int size, CancellationToken ct = default);
        Task<BlogPostDto> EditAsync(Guid authorId, EditBlogPostCommand cmd, CancellationToken ct = default);
        Task DeleteAsync(Guid authorId, long blogId, bool isModerator, CancellationToken ct = default);

        // Yorumlar — mevcut CommentService’i kullanacağız
        Task<Paged<Saht.Application.Comments.CommentDto>> ListCommentsAsync(long blogId, int page, int size, Guid viewerOrEmpty, CancellationToken ct = default);
        Task<Saht.Application.Comments.CommentDto> CreateCommentAsync(Guid authorId, long blogId, string body, CancellationToken ct = default);
    }
}
