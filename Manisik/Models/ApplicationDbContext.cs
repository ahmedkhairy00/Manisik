using Manisik.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;

namespace Manasik.Infrastructure.Data
{
    /// <summary>
    /// السياق الرئيسي لقاعدة بيانات تطبيق Manasik
    /// يحتوي على الجداول الخاصة بـ (Auth, UmrahBooking, Hotel, Transport, UmrahBookingHotel)
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<Auth, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 🧩 الجداول (Tables)
        public DbSet<Auth> Auths { get; set; }
        public DbSet<UmrahBooking> UmrahBookings { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Transport> Transports { get; set; }
        public DbSet<UmrahBookingHotel> UmrahBookingHotels { get; set; }

        // Rooms table
        public DbSet<Room> Rooms { get; set; }

        // Payment events to ensure idempotency
        public DbSet<PaymentEvent> PaymentEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // العلاقة بين Auth ↔ UmrahBooking
            // ========================================
            modelBuilder.Entity<UmrahBooking>()
                .HasOne(b => b.Auth)
                .WithMany(a => a.UmrahBookings)
                .HasForeignKey(b => b.AuthId)
                .OnDelete(DeleteBehavior.Cascade);
            // مستخدم واحد ممكن يعمل أكتر من حجز
            // لو اتحدف المستخدم → تتحدف كل حجوزاته تلقائيًا

            // ========================================
            // العلاقة بين Transport ↔ UmrahBooking
            // ========================================
            modelBuilder.Entity<UmrahBooking>()
                .HasOne(b => b.Transport)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TransportId)
                .OnDelete(DeleteBehavior.Restrict);
            // وسيلة النقل ممكن تكون مرتبطة بأكتر من حجز
            // لو اتحدفت وسيلة النقل → الحجوزات تظل موجودة

            // ========================================
            // العلاقة بين Hotel ↔ UmrahBookingHotel
            // ========================================
            modelBuilder.Entity<UmrahBookingHotel>()
                .HasOne(ubh => ubh.Hotel)
                .WithMany(h => h.Bookings)
                .HasForeignKey(ubh => ubh.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
            // كل BookingHotel مربوط بفندق
            // الفندق ممكن يكون له أكتر من حجز
            // لو اتحدف الفندق → الحجوزات مش هتمسح

            // ========================================
            // العلاقة بين UmrahBooking ↔ UmrahBookingHotel
            // ========================================
            modelBuilder.Entity<UmrahBookingHotel>()
                .HasOne(ubh => ubh.UmrahBooking)
                .WithMany(b => b.BookingHotels)
                .HasForeignKey(ubh => ubh.UmrahBookingId)
                .OnDelete(DeleteBehavior.Cascade);
            // كل حجز ممكن يكون له أكتر من حجز فندقي (مكة + المدينة)
            // لو اتحدف الحجز → تحذف كل BookingHotels المرتبطة بيه

            // ========================================
            // Relationship between Hotel and Room
            // ========================================
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========================================
            // ImgsUrl JSON conversion for Room with ValueComparer
            // ========================================
            var imgsComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v != null ? v.GetHashCode() : 0)),
                c => c.ToList());

            modelBuilder.Entity<Room>()
                .Property(r => r.ImgsUrl)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)")
                .Metadata.SetValueComparer(imgsComparer);

            // ========================================
            // Price precision for Room
            // ========================================
            modelBuilder.Entity<Room>()
                .Property(r => r.PricePerNight)
                .HasColumnType("decimal(10,2)");

            // ========================================
            // PaymentEvent mapping
            // ========================================
            modelBuilder.Entity<PaymentEvent>().ToTable("PaymentEvents");

            // ========================================
            // تحديد أسماء الجداول
            // ========================================
            modelBuilder.Entity<Auth>().ToTable("Auths");
            modelBuilder.Entity<Hotel>().ToTable("Hotels");
            modelBuilder.Entity<Transport>().ToTable("Transports");
            modelBuilder.Entity<UmrahBooking>().ToTable("UmrahBookings");
            modelBuilder.Entity<UmrahBookingHotel>().ToTable("UmrahBookingHotels");
            modelBuilder.Entity<Room>().ToTable("Rooms");
        }
    }
}
