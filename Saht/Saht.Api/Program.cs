using Saht.Api.Security;
using Saht.Application.Abstractions;
using Saht.Infrastructure;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Saht.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

            builder.Services
     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(opt =>
     {
         opt.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             ValidIssuer = builder.Configuration["Jwt:Issuer"],
             ValidAudience = builder.Configuration["Jwt:Audience"],
             IssuerSigningKey = new SymmetricSecurityKey(
                 Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
         };

         //  Burasý eklenecek kýsým
         opt.Events = new JwtBearerEvents
         {
             OnMessageReceived = ctx =>
             {
                 // Ýstekten "AuthToken" adlý cookie'yi al
                 if (ctx.Request.Cookies.TryGetValue("AuthToken", out var token))
                 {
                     // Eðer cookie bulunduysa, JWT middleware'ine token olarak ayarla
                     ctx.Token = token;
                 }
                 return Task.CompletedTask;
             }
         };

        
     });



            builder.Services.AddSwaggerGen(c =>
            {
                var jwt = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Bearer {token}"
                };
                c.AddSecurityDefinition("Bearer", jwt);
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwt, Array.Empty<string>() }
    });
            });


            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapGet("/api/ping", () => Results.Ok(new { ok = true, ts = DateTime.UtcNow }));


            app.Run();
        }
    }
}
