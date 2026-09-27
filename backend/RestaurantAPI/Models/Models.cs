using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAPI.Models
{
    // ─────────────────────────────────────────────
    // 1. Role
    // ─────────────────────────────────────────────
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required, MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        public ICollection<Staff> Staff { get; set; } = new List<Staff>();
    }

    // ─────────────────────────────────────────────
    // 2. Staff
    // ─────────────────────────────────────────────
    public class Staff
    {
        [Key]
        public int StaffID { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public int RoleID { get; set; }
        public Role Role { get; set; } = null!;

        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }

    // ─────────────────────────────────────────────
    // 3. Customer
    // ─────────────────────────────────────────────
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }

    // ─────────────────────────────────────────────
    // 4. RestaurantTable
    // ─────────────────────────────────────────────
    public class RestaurantTable
    {
        [Key]
        public int TableID { get; set; }

        [Required, MaxLength(10)]
        public string TableNumber { get; set; } = string.Empty;

        public int Capacity { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Available"; // Available, Reserved, Occupied

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    // ─────────────────────────────────────────────
    // 5. Reservation
    // ─────────────────────────────────────────────
    public class Reservation
    {
        [Key]
        public int ReservationID { get; set; }

        public int CustomerID { get; set; }
        public Customer Customer { get; set; } = null!;

        public int TableID { get; set; }
        public RestaurantTable Table { get; set; } = null!;

        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int PartySize { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    // ─────────────────────────────────────────────
    // 6. MenuCategory
    // ─────────────────────────────────────────────
    public class MenuCategory
    {
        [Key]
        public int CategoryID { get; set; }

        [Required, MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }

    // ─────────────────────────────────────────────
    // 7. MenuItem
    // ─────────────────────────────────────────────
    public class MenuItem
    {
        [Key]
        public int MenuItemID { get; set; }

        [Required, MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        public int CategoryID { get; set; }
        public MenuCategory Category { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<MenuItemIngredient> Ingredients { get; set; } = new List<MenuItemIngredient>();
    }

    // ─────────────────────────────────────────────
    // 8. Order
    // ─────────────────────────────────────────────
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int CustomerID { get; set; }
        public Customer Customer { get; set; } = null!;

        public int TableID { get; set; }
        public RestaurantTable Table { get; set; } = null!;

        public int? StaffID { get; set; }
        public Staff? Staff { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Placed"; // Placed, Preparing, Served, Paid, Cancelled

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; } = 0;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();
    }

    // ─────────────────────────────────────────────
    // 9. OrderItem
    // ─────────────────────────────────────────────
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        public int OrderID { get; set; }
        public Order Order { get; set; } = null!;

        public int MenuItemID { get; set; }
        public MenuItem MenuItem { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
    }

    // ─────────────────────────────────────────────
    // 10. Payment
    // ─────────────────────────────────────────────
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        public int OrderID { get; set; }
        public Order Order { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(20)]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Online

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Completed";
    }

    // ─────────────────────────────────────────────
    // 11. Vendor
    // ─────────────────────────────────────────────
    public class Vendor
    {
        [Key]
        public int VendorID { get; set; }

        [Required, MaxLength(100)]
        public string VendorName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }

    // ─────────────────────────────────────────────
    // 12. InventoryItem
    // ─────────────────────────────────────────────
    public class InventoryItem
    {
        [Key]
        public int InventoryItemID { get; set; }

        [Required, MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal QuantityInStock { get; set; } = 0;

        [Required, MaxLength(20)]
        public string Unit { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ReorderLevel { get; set; } = 10;

        public int? VendorID { get; set; }
        public Vendor? Vendor { get; set; }

        public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; } = new List<MenuItemIngredient>();
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }

    // ─────────────────────────────────────────────
    // 13. MenuItemIngredient (junction)
    // ─────────────────────────────────────────────
    public class MenuItemIngredient
    {
        public int MenuItemID { get; set; }
        public MenuItem MenuItem { get; set; } = null!;

        public int InventoryItemID { get; set; }
        public InventoryItem InventoryItem { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal QuantityRequired { get; set; }
    }

    // ─────────────────────────────────────────────
    // 14. PurchaseOrder
    // ─────────────────────────────────────────────
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderID { get; set; }

        public int VendorID { get; set; }
        public Vendor Vendor { get; set; } = null!;

        public int InventoryItemID { get; set; }
        public InventoryItem InventoryItem { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal QuantityOrdered { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Received
    }

    // ─────────────────────────────────────────────
    // 15. Feedback
    // ─────────────────────────────────────────────
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        public int CustomerID { get; set; }
        public Customer Customer { get; set; } = null!;

        public int? OrderID { get; set; }
        public Order? Order { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }

        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    }

    // ─────────────────────────────────────────────
    // 16. Discount
    // ─────────────────────────────────────────────
    public class Discount
    {
        [Key]
        public int DiscountID { get; set; }

        [Required, MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentOff { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<OrderDiscount> OrderDiscounts { get; set; } = new List<OrderDiscount>();
    }

    // ─────────────────────────────────────────────
    // 17. OrderDiscount (junction)
    // ─────────────────────────────────────────────
    public class OrderDiscount
    {
        public int OrderID { get; set; }
        public Order Order { get; set; } = null!;

        public int DiscountID { get; set; }
        public Discount Discount { get; set; } = null!;
    }

    // ─────────────────────────────────────────────
    // 18. Shift
    // ─────────────────────────────────────────────
    public class Shift
    {
        [Key]
        public int ShiftID { get; set; }

        public int StaffID { get; set; }
        public Staff Staff { get; set; } = null!;

        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool Attended { get; set; } = false;
    }

    // ─────────────────────────────────────────────
    // 19. AuditLog
    // ─────────────────────────────────────────────
    public class AuditLog
    {
        [Key]
        public int LogID { get; set; }

        [Required, MaxLength(20)]
        public string LogLevel { get; set; } = string.Empty; // Info, Warning, Error

        [Required, MaxLength(100)]
        public string Source { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
