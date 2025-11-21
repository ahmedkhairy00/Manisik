using Manisik.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets (initialized with null-forgiving to satisfy nullable references)
    public DbSet<Hotel> Hotels { get; set; } = null!;
    public DbSet<HotelRoom> HotelRooms { get; set; } = null!;
    public DbSet<InternationalTransport> InternationalTransports { get; set; } = null!;
    public DbSet<GroundTransport> GroundTransports { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingHotel> BookingHotels { get; set; } = null!;
    public DbSet<BookingInternationalTransport> BookingInternationalTransports { get; set; } = null!;
    public DbSet<BookingGroundTransport> BookingGroundTransports { get; set; } = null!;
    public DbSet<Traveler> Travelers { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<PaymentEvent> PaymentEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser (int) -> Booking (1:N)
        builder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        //// ApplicationUser (int) -> AIConversation (1:N)
        //builder.Entity<AIConversation>()
        //    .HasOne(c => c.User)
        //    .WithMany(u => u.AIConversations)
        //    .HasForeignKey(c => c.UserId)
        //    .OnDelete(DeleteBehavior.Cascade);

        // Booking -> BookingHotel (1:N)
        builder.Entity<BookingHotel>()
            .HasOne(bh => bh.Booking)
            .WithMany(b => b.Hotels)
            .HasForeignKey(bh => bh.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // BookingHotel -> Hotel (N:1)
        builder.Entity<BookingHotel>()
            .HasOne(bh => bh.Hotel)
            .WithMany(h => h.BookingHotels)
            .HasForeignKey(bh => bh.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        // BookingHotel -> HotelRoom (N:1)
        builder.Entity<BookingHotel>()
            .HasOne(bh => bh.Room)
            .WithMany()
            .HasForeignKey(bh => bh.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking -> BookingInternationalTransport (1:N)
        builder.Entity<BookingInternationalTransport>()
            .HasOne(bit => bit.Booking)
            .WithMany(b => b.BookingInternationalTransport)
            .HasForeignKey(bit => bit.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // BookingInternationalTransport -> InternationalTransport (N:1)
        builder.Entity<BookingInternationalTransport>()
            .HasOne(bit => bit.InternationalTransport)
            .WithMany(t => t.BookingInternationalTransport)
            .HasForeignKey(bit => bit.InternationalTransportId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking -> BookingGroundTransport (1:N)
        builder.Entity<BookingGroundTransport>()
            .HasOne(bgt => bgt.Booking)
            .WithMany(b => b.BookingGroundTransport)
            .HasForeignKey(bgt => bgt.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // BookingGroundTransport -> GroundTransport (N:1)
        builder.Entity<BookingGroundTransport>()
            .HasOne(bgt => bgt.GroundTransport)
            .WithMany(gt => gt.BookingGroundTransport)
            .HasForeignKey(bgt => bgt.GroundTransportId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking -> Traveler (1:N)
        builder.Entity<Traveler>()
            .HasOne(t => t.Booking)
            .WithMany(b => b.Travelers)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Booking -> Payment (1:1) (FK on Payment)
        builder.Entity<Payment>()
            .HasOne(p => p.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Payment -> PaymentEvent (1:N)
        builder.Entity<PaymentEvent>()
            .HasOne(pe => pe.Payment)
            .WithMany(p => p.PaymentEvents)
            .HasForeignKey(pe => pe.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
