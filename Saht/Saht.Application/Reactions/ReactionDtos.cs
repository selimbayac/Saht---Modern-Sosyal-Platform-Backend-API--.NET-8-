using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Reactions
{  
        public sealed record ReactCommand(long TargetId, int Value); // +1 / -1
        public sealed record ReactionSummaryDto(int Likes, int Dislikes, int Score, int? My);
        public sealed record ReactionUserDto(Guid UserId, string UserName, string DisplayName, int Value, DateTime At);

        public sealed record ReactorItemDto(Guid UserId, string UserName,string DisplayName,int Value, DateTime At);


}
