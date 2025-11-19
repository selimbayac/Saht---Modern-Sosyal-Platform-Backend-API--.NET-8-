using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Application.Posts
{
    public sealed record PagedList<T>(
     IReadOnlyList<T> Items,
     int TotalCount,
     int PageNumber,
     int PageSize)
    {
        // Hesaplanan faydalı alanlar
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => PageNumber * PageSize < TotalCount;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
