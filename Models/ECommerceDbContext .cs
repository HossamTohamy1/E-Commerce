using E_Commers.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ECommerceDbContext : IdentityDbContext<ApplicationUser>
{

    public DbSet<Address> Addresses { get; set; }
    public DbSet<SellerUpgradeRequest> SellerUpgradeRequests { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ShippingDetail> ShippingDetails { get; set; }
    public DbSet<ProductSeller> ProductSellers { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<ProductPromotion> ProductPromotions { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<PromotionRequest> PromotionRequests { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SiteSettings> SiteSettings { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductCategory>()
            .HasKey(pc => new { pc.ProductId, pc.CategoryId });

        modelBuilder.Entity<ProductSeller>()
            .HasKey(ps => new { ps.ProductId, ps.UserId }); // تغيير SellerId إلى UserId

        modelBuilder.Entity<ProductPromotion>()
            .HasKey(pp => new { pp.ProductId, pp.PromotionId });

        modelBuilder.Entity<Order>()
            .HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.User)
            .WithMany(u => u.Wishlists)
            .HasForeignKey(w => w.UserId);

        modelBuilder.Entity<Recommendation>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Product)
            .WithMany(p => p.Inventories)
            .HasForeignKey(i => i.ProductId);

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Warehouse)
            .WithMany(w => w.Inventories)
            .HasForeignKey(i => i.WarehouseId);

        modelBuilder.Entity<ProductSeller>()
            .HasOne(ps => ps.Product)
            .WithMany(p => p.ProductSellers)
            .HasForeignKey(ps => ps.ProductId);

        modelBuilder.Entity<ProductSeller>()
            .HasOne(ps => ps.User) // تغيير Seller إلى User
            .WithMany(u => u.ProductSellers)
            .HasForeignKey(ps => ps.UserId); // تغيير SellerId إلى UserId

        modelBuilder.Entity<SellerUpgradeRequest>()
            .HasOne(s => s.User)
            .WithMany(u => u.SellerUpgradeRequests)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId);

        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId);

        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId);

        modelBuilder.Entity<ProductAttribute>()
            .HasOne(pa => pa.Product)
            .WithMany(p => p.Attributes)
            .HasForeignKey(pa => pa.ProductId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);

        modelBuilder.Entity<WishlistItem>()
            .HasOne(wi => wi.Wishlist)
            .WithMany(w => w.Items)
            .HasForeignKey(wi => wi.WishlistId);

        modelBuilder.Entity<WishlistItem>()
            .HasOne(wi => wi.Product)
            .WithMany(p => p.WishlistItems)
            .HasForeignKey(wi => wi.ProductId);

        modelBuilder.Entity<ProductPromotion>()
            .HasOne(pp => pp.Product)
            .WithMany(p => p.ProductPromotions)
            .HasForeignKey(pp => pp.ProductId);

        modelBuilder.Entity<ProductPromotion>()
            .HasOne(pp => pp.Promotion)
            .WithMany(p => p.ProductPromotions)
            .HasForeignKey(pp => pp.PromotionId);

        modelBuilder.Entity<ShippingDetail>()
            .HasOne(sd => sd.Order)
            .WithOne(o => o.ShippingDetail)
            .HasForeignKey<ShippingDetail>(sd => sd.OrderId);

        modelBuilder.Entity<Store>()
                   .HasOne(s => s.User)
                   .WithMany(u => u.Stores)
                   .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<PromotionRequest>()
            .HasOne(pr => pr.User)
            .WithMany(u => u.PromotionRequests)
            .HasForeignKey(pr => pr.UserId);

        modelBuilder.Entity<PromotionRequest>()
            .HasOne(pr => pr.Promotion)
            .WithMany()
            .HasForeignKey(pr => pr.PromotionId);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId);

        modelBuilder.Entity<Complaint>()
            .HasOne(c => c.User)
            .WithMany(u => u.Complaints)
            .HasForeignKey(c => c.UserId);
    }
}