Saht - Modern Sosyal Platform Backend API (.NET 8)
Saht, ASP.NET Core Web API (.NET 8) ile sıfırdan geliştirilmiş, kurumsal standartlarda, modern ve ölçeklenebilir bir sosyal medya platformu backend uygulamasıdır.

Bu proje, gerçek bir sosyal medya uygulamasının tüm temel altyapısını (kullanıcı işlemleri, içerik yönetimi, etkileşimler, bildirimler ve moderasyon) içermektedir.

✨ Özellikler
Kullanıcı Yönetimi: Kayıt, giriş, profil işlemleri
Gönderi (Post) Yönetimi: Oluşturma, düzenleme, silme, reply, quote, repost
Yorum (Comment) Sistemi: Thread yapısı, düzenleme, soft delete
Reaksiyonlar: İdempotent like/dislike motoru, reaksiyon özeti ve liste
Blog Modülü: Blog yazıları, yorumlar, reaksiyonlar ve bildirimler
Takip Sistemi: Takip et/çık, follower feed
Bildirim Sistemi: JSON payload tabanlı, okunmamış bildirim sayısı, toplu okuma
Moderasyon & Raporlama: Şikayet → İnceleme → Aksiyon akışı (Pending > Reviewed > ActionTaken)
Güvenlik: JWT Authentication, Refresh Token, Role-based Authorization (User, Moderator, Admin), Anti-Abuse kuralları, soft delete, audit trail
🏗️ Mimari ve Tasarım
N-Layer Architecture (Domain, Application, Infrastructure, API)
Domain-Driven Design (DDD) prensipleri
Aggregate Root yapıları ve domain içi iş kuralları
CQRS Pattern (MediatR ile)
Repository Pattern & Unit of Work
Value Objects ve zengin domain modelleme
Tamamen asenkron mimari (Async/Await)
📁 Proje Yapısı
Saht/
├── Saht.Domain/           # Entities, Aggregates, Value Objects, Business Rules
├── Saht.Application/      # Commands, Queries, DTOs, Services, Validators
├── Saht.Infrastructure/   # DbContext, Repositories, Migrations, Configurations
└── Saht.Api/              # Controllers, Middleware, Program.cs, Swagger
🔧 Kullanılan Teknolojiler

Backend: .NET 8, ASP.NET Core Web API Veritabanı: PostgreSQL, Entity Framework Core Mimari: MediatR (CQRS), AutoMapper, FluentValidation Güvenlik: JWT Authentication, Role-based Authorization Diğer: Swagger UI, Pagination, Global Exception Handling, Serilog

🔒 Güvenlik Özellikleri

JWT + Refresh Token Rol tabanlı yetkilendirme (User / Moderator / Admin) Global Exception Middleware Validation Pipeline Soft Delete & Audit Trail Anti-Abuse ve davranış kısıtlamaları

🚀 Nasıl Çalıştırılır?

1.Projeyi klonlayın:Bash git clone https://github.com/selimbayac/Saht---Modern-Sosyal-Platform-Backend-API--.NET-8-.git cd Saht---Modern-Sosyal-Platform-Backend-API--.NET-8-/Saht

Saht.Api/appsettings.json dosyasında PostgreSQL bağlantı ayarlarını yapın.
Migration'ları çalıştırın: dotnet ef database update --project Saht.Infrastructure --startup-project Saht.Api
Uygulamayı başlatın:dotnet run --project Saht.Api Visual Studio kullanıyorsanız Saht.sln dosyasını açıp F5 ile çalıştırabilirsiniz.
📌 Bu Proje ile Kazanılan Yetkinlikler

Kurumsal seviyede backend mimarisi tasarımı Domain-Driven Design ve zengin domain modelleme İlişkisel veritabanı tasarımı ve performans optimizasyonu Güvenli ve ölçeklenebilir API geliştirme Gerçek dünya sosyal medya mantığının implementasyonu
