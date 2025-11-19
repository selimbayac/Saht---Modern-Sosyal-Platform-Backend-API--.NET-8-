using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Blogs
{
    public sealed class Blog : AggregateRoot<Guid>
    {
        private Blog() { }

        public Guid OwnerId { get; private set; }
        public string Title { get; private set; } = null!;
        public string Slug { get; private set; } = null!;
        public bool IsPublic { get; private set; } = true;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public static Blog Create(Guid ownerId, string title, string slug)
        {
            if (ownerId == Guid.Empty) throw new ArgumentException(nameof(ownerId));
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException(nameof(title));
            if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException(nameof(slug));

            return new Blog
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                Title = title.Trim(),
                Slug = slug.Trim().ToLowerInvariant()
            };
        }

        public void Rename(string newTitle) => Title = string.IsNullOrWhiteSpace(newTitle) ? Title : newTitle.Trim();
        public void SetPublic(bool value) => IsPublic = value;
    }
}
