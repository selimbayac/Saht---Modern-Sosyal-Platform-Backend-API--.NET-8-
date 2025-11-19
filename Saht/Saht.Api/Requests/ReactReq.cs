using Saht.Domain.Common;

namespace Saht.Api.Requests
{
    public sealed record ReactReq(int Value);
    public sealed record EditReq(string Body);
    public sealed record ReactBody(int Value);
    public sealed record CreateReq( string Body);
    public sealed record CreateBlog(string Title ,string Body);
    public sealed record BodyReq(string Body);
   // public sealed record CreateReq(ContentType TargetType, long TargetId, string Reason);
    public sealed record NoteReq(string? Note);
    public sealed record ContentCreateReq(string Title, string Body);
    public sealed record ReportCreateReq(ContentType TargetType, long TargetId, string Reason);
}
