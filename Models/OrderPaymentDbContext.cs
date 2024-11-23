using Microsoft.EntityFrameworkCore;
using OrderPayment.Models;

public class OrderPaymentDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } // Ürünler
    public DbSet<User> Users { get; set; } // Kullanıcılar
    public DbSet<Order> Orders { get; set; } // Siparişler
    public DbSet<OrderItem> OrderItems { get; set; } // Sipariş Kalemleri
    public DbSet<Admin> Admins { get; set; } // Yöneticiler
    public DbSet<VerificationCode> VerificationCodes { get; set; } // Doğrulama Kodları
    public DbSet<Cart> Carts { get; set; } // Sepetler
    public DbSet<CartItem> CartItems { get; set; } // Sepet Kalemleri
    public DbSet<UserCard> UserCards { get; set; } // Kullanıcı Kartları
    public DbSet<Address> Address { get; set; } // Adresler

    public OrderPaymentDbContext(DbContextOptions<OrderPaymentDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - Order ilişkisi
        // Bir kullanıcı birden fazla siparişe sahip olabilir
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde siparişler de silinir

        // Order - OrderItem ilişkisi
        // Bir sipariş birden fazla sipariş kalemine sahip olabilir
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Sipariş silindiğinde kalemler de silinir

        // User - VerificationCode ilişkisi
        // Bir kullanıcı birden fazla doğrulama koduna sahip olabilir
        modelBuilder.Entity<VerificationCode>()
            .HasOne(vc => vc.User)
            .WithMany(u => u.VerificationCodes)
            .HasForeignKey(vc => vc.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde doğrulama kodları da silinir

        // User - Cart ilişkisi
        // Bir kullanıcı bir sepete sahip olabilir
        modelBuilder.Entity<User>()
            .HasOne(u => u.Cart)
            .WithOne(c => c.User)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde sepet de silinir

        // Cart - CartItem ilişkisi
        // Bir sepet birden fazla sepet öğesine sahip olabilir
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade); // Sepet silindiğinde öğeler de silinir

        // User - UserCard ilişkisi
        // Bir kullanıcı birden fazla karta sahip olabilir
        modelBuilder.Entity<UserCard>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserCards)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde kartlar da silinir

        // Address ilişkisi (Bir kullanıcı birden fazla adrese sahip olabilir)
        modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde adresler de silinir
    }
}
