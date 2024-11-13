using Microsoft.EntityFrameworkCore;
using OrderPayment.Models;

public class OrderPaymentDbContext : DbContext
{
    public DbSet<Product> products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; } // Yeni model ekledik
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }


    public OrderPaymentDbContext(DbContextOptions<OrderPaymentDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - Order ilişkisi (Bir kullanıcı birden fazla siparişe sahip olabilir)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde ona ait siparişler de silinsin

        // Order - OrderItem ilişkisi (Bir sipariş birden fazla sipariş kalemine sahip olabilir)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Sipariş silindiğinde ona ait kalemler de silinsin

        // User - VerificationCode ilişkisi (Bir kullanıcı birden fazla doğrulama koduna sahip olabilir)
        modelBuilder.Entity<VerificationCode>()
            .HasOne(vc => vc.User)
            .WithMany(u => u.VerificationCodes)
            .HasForeignKey(vc => vc.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde doğrulama kodları da silinsin

        // User - Cart ilişkisi (Bir kullanıcı bir sepete sahip olabilir)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Cart) // Her kullanıcıya bir sepet
            .WithOne(c => c.User) // Her sepet bir kullanıcıya ait
            .HasForeignKey<Cart>(c => c.UserId) // Cart tablosunda UserId dış anahtar olarak kullanılır
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde ona ait sepet de silinsin

        // Cart - CartItem ilişkisi (Bir sepet birden fazla sepet öğesine sahip olabilir)
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade); // Sepet silindiğinde ona ait öğeler de silinsin
    }
}
