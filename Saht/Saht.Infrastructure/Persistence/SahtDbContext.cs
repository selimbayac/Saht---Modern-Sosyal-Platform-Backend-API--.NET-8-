using Microsoft.EntityFrameworkCore;
using Saht.Domain.Blogs;
using Saht.Domain.Comments;
using Saht.Domain.Follows;
using Saht.Domain.Notifications;
using Saht.Domain.Posts;
using Saht.Domain.Reactions;
using Saht.Domain.Reports;
using Saht.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Saht.Infrastructure.Persistence
{
    public sealed class SahtDbContext : DbContext
    {
        public SahtDbContext(DbContextOptions<SahtDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Follow> Follows => Set<Follow>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Reaction> Reactions => Set<Reaction>();
        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Saht.Domain.Social.Block> Blocks => Set<Saht.Domain.Social.Block>();
        public DbSet<Saht.Domain.Social.Mute> Mutes => Set<Saht.Domain.Social.Mute>();


        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // USERS
            b.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);
                e.Property(x => x.UserName).IsRequired().HasMaxLength(32);
                e.Property(x => x.Email).IsRequired().HasMaxLength(255);
                e.Property(x => x.DisplayName).IsRequired().HasMaxLength(64);
                e.Property(x => x.PasswordHash).IsRequired();
                e.HasIndex(x => x.UserName).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.CreatedAt);
            });

            // FOLLOWS (composite key)
            b.Entity<Follow>(e =>
            {
                e.ToTable("Follows");
                e.HasKey(x => new { x.FollowerId, x.FolloweeId }); // composite PK
                e.HasIndex(x => x.FolloweeId);
                e.HasIndex(x => x.FollowerId);
                //e.HasOne<User>().WithMany().HasForeignKey(x => x.FollowerId).OnDelete(DeleteBehavior.Cascade);
                //e.HasOne<User>().WithMany().HasForeignKey(x => x.FolloweeId).OnDelete(DeleteBehavior.Cascade);

                // composite PK zaten unique; alttaki indeks opsiyonel
                // e.HasIndex(nameof(Follow.FollowerId), nameof(Follow.FolloweeId)).IsUnique();
            });

            // POSTS
            b.Entity<Post>(e =>
            {
                e.ToTable("Posts");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd(); // long identity
                e.Property(x => x.Body).HasMaxLength(4000);
                e.HasIndex(x => x.AuthorId);
                e.HasIndex(x => x.CreatedAt);
                e.HasIndex(x => x.ParentPostId);
            });

            // COMMENTS (generic hedef)
            b.Entity<Comment>(e =>
            {
                e.ToTable("Comments");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.Body).IsRequired().HasMaxLength(4000);
                e.HasIndex(x => new { x.TargetType, x.TargetId });
                e.HasIndex(x => x.ParentCommentId);
                e.HasIndex(x => x.CreatedAt);
            });

            // REACTIONS (like/dislike) – tekil oy: composite PK
            b.Entity<Reaction>(e =>
            {
                e.ToTable("Reactions");
                e.HasKey(x => new { x.TargetType, x.TargetId, x.UserId });
                e.Property(x => x.Value).IsRequired();
                e.HasIndex(x => new { x.TargetType, x.TargetId }); // toplam sayım için
            });

            // BLOGS
            b.Entity<Saht.Domain.Blogs.BlogPost>(e =>
            {
                e.ToTable("BlogPosts");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.AuthorId).IsRequired();
                e.Property(x => x.Title).IsRequired().HasMaxLength(160);
                e.Property(x => x.Body).IsRequired();
                e.HasIndex(x => x.AuthorId);
                e.HasIndex(x => x.CreatedAt);
            });

            // BLOG POSTS
            b.Entity<BlogPost>(e =>
            {
                e.ToTable("BlogPosts");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.Title).IsRequired().HasMaxLength(160);
                // HATA DÜZELTİLDİ: Content yerine Body kullanılmalı
                e.Property(x => x.Body).IsRequired();
                // BlogId sınıfınızda yoksa bu satır kaldırılmalı/düzeltilmeli.
                // Eğer sadece yazar ve oluşturulma zamanına göre indekslenecekse:
                e.HasIndex(x => new { x.AuthorId, x.CreatedAt });
            });

            // REPORTS
            b.Entity<Report>(e =>
            {
                e.ToTable("Reports");
                e.HasKey(x => x.Id);
                e.Property(x => x.Reason).IsRequired().HasMaxLength(512);
                e.HasIndex(x => new { x.TargetType, x.TargetId });
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.CreatedAt);
            });

            // NOTIFICATIONS
            b.Entity<Notification>(e =>
            {
                e.ToTable("Notifications");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.Payload).IsRequired();
                e.HasIndex(x => x.UserId);
                e.HasIndex(x => new { x.UserId, x.IsRead });
                e.HasIndex(x => x.CreatedAt);
            });

            b.Entity<Saht.Domain.Social.Block>(e =>
            {
                e.ToTable("Blocks");
                e.HasKey(x => new { x.BlockerId, x.BlockedId });
                e.HasIndex(x => x.BlockerId);
                e.HasIndex(x => x.BlockedId);
            });

            // Mutes
            b.Entity<Saht.Domain.Social.Mute>(e =>
            {
                e.ToTable("Mutes");
                e.HasKey(x => new { x.MuterId, x.MutedId });
                e.HasIndex(x => x.MuterId);
                e.HasIndex(x => x.MutedId);
            });
            // OnModelCreating
           


        }
    }
}
