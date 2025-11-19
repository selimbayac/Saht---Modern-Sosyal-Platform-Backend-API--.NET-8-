using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Blogs
{
    public sealed record BlogPostDto(
       long Id,
       Guid AuthorId,
       string AuthorUserName,
       string AuthorDisplayName,
       string Title,
       string Body,
       DateTime CreatedAt,
       DateTime? EditedAt
   );

    public sealed record CreateBlogPostCommand(string Title, string Body);
    public sealed record EditBlogPostCommand(long Id, string Title, string Body);
}
