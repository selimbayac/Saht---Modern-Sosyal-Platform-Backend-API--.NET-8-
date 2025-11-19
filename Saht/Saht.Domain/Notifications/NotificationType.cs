using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Notifications
{
    public enum NotificationType
    {
        // Sosyal
        NewFollower = 1,  // followerId
        PostLiked = 2,  // postId, fromUserId, value(+1/-1)
        PostCommented = 3,  // postId, commentId, fromUserId
        CommentLiked = 4,  // commentId, fromUserId, value
        MentionInPost = 5,  // postId, fromUserId
        MentionInComment = 6,  // commentId, fromUserId

        // Moderasyon
        ReportReceived = 20, // reportId
        ReportDecision = 21, // reportId, decision

        // Sistem / Duyuru
        SystemAnnouncement = 40, // title, link(optional)

        // Takip ettiğin kullanıcı aktiviteleri (opsiyonel)
        FolloweeNewPost = 60, // postId, followeeId
        FolloweeNewBlogPost = 61, // blogPostId, followeeId

        BlogPostCommented = 62,
        BlogPostLiked = 63
    }
}
