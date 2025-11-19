using Saht.Application.Abstractions;
using System.Security.Claims;

namespace Saht.Api.Security
{
    public sealed class HttpCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;

        public HttpCurrentUser(IHttpContextAccessor http) => _http = http;

        public ClaimsPrincipal? Principal => _http.HttpContext?.User;
        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        public Guid? UserId
        {
            get
            {
                var sub = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? Principal?.FindFirst(ClaimTypes.Name)?.Value
                          ?? Principal?.FindFirst("sub")?.Value;
                return Guid.TryParse(sub, out var id) ? id : null;
            }
        }

        public string? UserName =>
            Principal?.FindFirst(ClaimTypes.Name)?.Value
            ?? Principal?.FindFirst("unique_name")?.Value;

        public string? Role =>
            Principal?.FindFirst(ClaimTypes.Role)?.Value;
    }
}
