using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options) { }

        // ── DbSets ──────────────────────────────────────────────────────
        public DbSet<Role>                Role               { get; set; }
        public DbSet<Staff>               Staff              { get; set; }
        public DbSet<Customer>            Customers          { get; set; }
        public DbSet<RestaurantTable>     RestaurantTables   { get; set; }
        public DbSet<Reservation>         Reservations       { get; set; }
        public DbSet<MenuCategory>        MenuCategories     { get; set; }
        public DbSet<MenuItem>            MenuItems          { get; set; }
        public DbSet<Order>               Orders             { get; set; }
        public DbSet<OrderItem>           OrderItems         { get; set; }
        public DbSet<Payment>             Payments           { get; set; }
        public DbSet<Vendor>              Vendors            { get; set; }
        public DbSet<InventoryItem>       InventoryItems     { get; set; }
        public DbSet<MenuItemIngredient>  MenuItemIngredients{ get; set; }
        public DbSet<PurchaseOrder>       PurchaseOrders     { get; set; }
        public DbSet<Feedback>            Feedbacks          { get; set; }
        public DbSet<Discount>            Discounts          { get; set; }
        public DbSet<OrderDiscount>       OrderDiscounts     { get; set; }
        public DbSet<Shift>               Shifts             { get; set; }
        public DbSet<AuditLog>            AuditLogs          { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Composite keys for junction tables ──────────────────────
            modelBuilder.Entity<MenuItemIngredient>()
                .HasKey(m => new { m.MenuItemID, m.InventoryItemID });

            modelBuilder.Entity<OrderDiscount>()
                .HasKey(od => new { od.OrderID, od.DiscountID });

            // ── Unique constraints ───────────────────────────────────────
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Email).IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email).IsUnique();

            modelBuilder.Entity<RestaurantTable>()
                .HasIndex(t => t.TableNumber).IsUnique();

            modelBuilder.Entity<Discount>()
                .HasIndex(d => d.Code).IsUnique();

            // ── Payment → Order (one-to-one) ────────────────────────────
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderID);

            // ── OrderItem computed column (Subtotal) ─────────────────────
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Subtotal)
                .HasComputedColumnSql("`Quantity` * `UnitPrice`", stored: true);

            // ── Cascade rules ────────────────────────────────────────────
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            // ── CHECK constraints ────────────────────────────────────────
            modelBuilder.Entity<RestaurantTable>()
                .ToTable(t => t.HasCheckConstraint("CHK_Table_Capacity", "`Capacity` > 0"));

            modelBuilder.Entity<RestaurantTable>()
                .ToTable(t => t.HasCheckConstraint("CHK_Table_Status",
                    "`Status` IN ('Available','Reserved','Occupied')"));

            modelBuilder.Entity<MenuItem>()
                .ToTable(t => t.HasCheckConstraint("CHK_MenuItem_Price", "`Price` > 0"));

            modelBuilder.Entity<Feedback>()
                .ToTable(t => t.HasCheckConstraint("CHK_Feedback_Rating",
                    "`Rating` BETWEEN 1 AND 5"));

            modelBuilder.Entity<Discount>()
                .ToTable(t => t.HasCheckConstraint("CHK_Discount_Percent",
                    "`PercentOff` > 0 AND `PercentOff` <= 100"));
        }
    }
}
