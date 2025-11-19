using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Notifications
{
    public sealed record NotificationDto(
    long Id,
    string Type,
    string Payload,    // ham JSON (UI kendisi çözer)
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt
);
}
