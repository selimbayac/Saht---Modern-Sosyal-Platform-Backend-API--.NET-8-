using Saht.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Domain.Blogs
{
    public sealed class BlogPost : AggregateRoot<long>
    {
        private BlogPost() { }

        public long Id { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Title { get; private set; } = null!;
        public string Body { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; private set; }
        public bool IsDeleted { get; private set; }

        public static BlogPost Create(Guid authorId, string title, string body)
        {
            if (authorId == Guid.Empty) throw new ArgumentException(nameof(authorId));
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Başlık boş olamaz");
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Metin boş olamaz");
            if (title.Length > 160) throw new ArgumentException("Başlık 160 karakteri aşmamalı");
            if (body.Length > 20000) throw new ArgumentException("Metin 20k karakteri aşmamalı");

            return new BlogPost { AuthorId = authorId, Title = title.Trim(), Body = body };
        }

        public void Edit(string title, string body)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Başlık boş olamaz");
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Metin boş olamaz");
            Title = title.Trim();
            Body = body;
            EditedAt = DateTime.UtcNow;
        }

        public void SoftDelete() => IsDeleted = true;
    }
}
