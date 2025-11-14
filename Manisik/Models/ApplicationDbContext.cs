using Manisik.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<Auth>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<HotelRoom> HotelRooms { get; set; }
    public DbSet<GlobalTransport> InternationalTransports { get; set; }
    public DbSet<InternalTransport> GroundTransports { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingHotel> BookingHotels { get; set; }
    public DbSet<BookingGlobalTransport> BookingGlobalTransport { get; set; }
    public DbSet<BookingInternalTransport> BookingInternalTransport { get; set; }
    public DbSet<Traveler> Travelers { get; set; }
    public DbSet<PaymentEvent> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ========== Package Configuration ==========


        // ========== Hotel Configuration ==========
        builder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.HotelId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DistanceFromHaram).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.Rate);

            entity.HasMany(e => e.Rooms)
                .WithOne(e => e.Hotel)
                .HasForeignKey(e => e.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== HotelRoom Configuration ==========
        builder.Entity<HotelRoom>(entity =>
        {
            entity.HasKey(e => e.HotelRoomID);
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)");
        });

        // ========== Booking Configuration ==========
        builder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId);
            entity.Property(e => e.BookingNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.BookingNumber).IsUnique();
            entity.HasIndex(e => e.BookingStatus);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(e => e.Booking)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);



            //entity.HasOne(e => e.PaymentEvents)
            //    .WithOne(e => e.Booking)
            //    .HasForeignKey<Payment>(e => e.BookingId)
            //    .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== BookingHotel Configuration ==========
        builder.Entity<BookingHotel>(entity =>
        {
            entity.HasKey(e => e.BookingHotelId);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Booking)
                .WithMany(e => e.BookingHotel)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== Traveler Configuration ==========
        builder.Entity<Traveler>(entity =>
        {
            entity.HasKey(e => e.TravelerId);
            entity.Property(e => e.PassportNumber).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Booking)
                .WithMany(e => e.Travelers)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== Payment Configuration ==========
        builder.Entity<PaymentEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

        });
    }
}
