using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Posts
{
    public class PostCommands
    {
        public sealed record CreatePostCommand(string Body);
        public sealed record ReplyCommand(long ParentPostId, string Body);
        public sealed record QuoteCommand(long ParentPostId, string Body);
        public sealed record RepostCommand(long ParentPostId);
        public sealed record EditPostCommand(long PostId, string Body);
    }
}
