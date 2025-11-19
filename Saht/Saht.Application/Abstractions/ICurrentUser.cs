using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Abstractions
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
        string? UserName { get; }
        string? Role { get; }
        ClaimsPrincipal? Principal { get; }
    }
}
