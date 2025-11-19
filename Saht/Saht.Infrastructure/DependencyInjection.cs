using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Saht.Application.Abstractions;
using Saht.Application.Auth;
using Saht.Application.Blogs;
using Saht.Application.Comments;
using Saht.Application.Notifications;
using Saht.Application.Posts;
using Saht.Application.Reactions;
using Saht.Application.Reports;
using Saht.Application.SocialConnections;
using Saht.Application.Users;
using Saht.Infrastructure.Persistence;
using Saht.Infrastructure.Repositories;
using Saht.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saht.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<SahtDbContext>(opt =>
                opt.UseNpgsql(config.GetConnectionString("Default")));


            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPostRepository, EFPostRepository>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IReactionRepository, EFReactionRepository>();
            services.AddScoped<IReactionService, ReactionService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ICommentRepository, EFCommentRepository>();
            services.AddScoped<INotificationRepository, EFNotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IFollowRepository, EFFollowRepository>();
            services.AddScoped<IFollowService, FollowService>();
            services.AddScoped<IBlogPostRepository, EFBlogPostRepository>();
            services.AddScoped<IBlogPostService, BlogPostService>();
            services.AddScoped<IBlockRepository, EFBlockRepository>();
            services.AddScoped<IMuteRepository, EFMuteRepository>();
            services.AddScoped<IPrivacyService, PrivacyService>();

            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportRepository, EFReportRepository>();





            // İleride buraya eklenecekler:
            // services.AddScoped<IUnitOfWork, SahtDbContext>();
            // services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            // services.AddStackExchangeRedisCache(...);
            // services.AddSingleton<ISlugGenerator, SlugGenerator>();

            return services;
        }
    }
}
