// Saht.Infrastructure/Persistence/DesignTimeDbContextFactory.cs
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Saht.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SahtDbContext>
    {
        public SahtDbContext CreateDbContext(string[] args)
        {
            // 0) ENV VAR (istersen hiç uğraşmadan buradan ver)
            var envCs = Environment.GetEnvironmentVariable("SAHT_CONN");
            if (!string.IsNullOrWhiteSpace(envCs))
                return Build(envCs);

            // 1) appsettings.Development.json’ı arıyoruz (çoğu zaman Saht.Api’de)
            var baseDir = Directory.GetCurrentDirectory();
            var candidates = new[]
            {
                Path.Combine(baseDir, "appsettings.Development.json"),
                Path.Combine(baseDir, "appsettings.json"),

                Path.Combine(baseDir, "Saht.Api", "appsettings.Development.json"),
                Path.Combine(baseDir, "Saht.Api", "appsettings.json"),

                Path.Combine(baseDir, "..", "Saht.Api", "appsettings.Development.json"),
                Path.Combine(baseDir, "..", "Saht.Api", "appsettings.json"),

                Path.Combine(baseDir, "..", "..", "Saht.Api", "appsettings.Development.json"),
                Path.Combine(baseDir, "..", "..", "Saht.Api", "appsettings.json"),

                // bin/Debug/… senaryosu
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Saht.Api", "appsettings.Development.json"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Saht.Api", "appsettings.json")
            };

            string? foundPath = null;
            foreach (var p in candidates)
            {
                var full = Path.GetFullPath(p);
                if (File.Exists(full)) { foundPath = full; break; }
            }

            if (foundPath is null)
                throw new InvalidOperationException(
                    "ConnectionStrings:Default bulunamadı. Design-time sırasında appsettings'e erişilemedi. " +
                    "Ya SAHT_CONN ortam değişkeni ver ya da Saht.Api içindeki appsettings.Development.json yolunu kontrol et.");

            var config = new ConfigurationBuilder()
                .AddJsonFile(foundPath, optional: false)
                .Build();

            var cs = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("appsettings içerisinde ConnectionStrings:Default yok.");

            return Build(cs);
        }

        private static SahtDbContext Build(string cs)
        {
            var opts = new DbContextOptionsBuilder<SahtDbContext>()
                .UseNpgsql(cs)
                .Options;
            return new SahtDbContext(opts);
        }
    }
}
