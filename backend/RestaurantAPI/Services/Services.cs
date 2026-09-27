using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.DTOs;
using RestaurantAPI.Models;

namespace RestaurantAPI.Services
{
    // ─────────────────────────────────────────────────────────────────────
    // 1. ReservationManager
    // ─────────────────────────────────────────────────────────────────────
    public class ReservationManager
    {
        private readonly RestaurantDbContext _db;
        public ReservationManager(RestaurantDbContext db) => _db = db;

        public async Task<List<RestaurantTableDto>> GetAvailableTablesAsync(
            DateTime date, string time, int partySize)
        {
            var reservedTableIds = await _db.Reservations
                .Where(r => r.ReservationDate.Date == date.Date
                         && r.ReservationTime == TimeSpan.Parse(time)
                         && (r.Status == "Pending" || r.Status == "Confirmed"))
                .Select(r => r.TableID)
                .ToListAsync();

            return await _db.RestaurantTables
                .Where(t => t.Capacity >= partySize && !reservedTableIds.Contains(t.TableID))
                .Select(t => new RestaurantTableDto
                {
                    TableID     = t.TableID,
                    TableNumber = t.TableNumber,
                    Capacity    = t.Capacity,
                    Status      = t.Status
                }).ToListAsync();
        }

        public async Task<Reservation> CreateReservationAsync(CreateReservationDto dto)
        {
            var reservation = new Reservation
            {
                CustomerID      = dto.CustomerID,
                TableID         = dto.TableID,
                ReservationDate = dto.ReservationDate,
                ReservationTime = TimeSpan.Parse(dto.ReservationTime),
                PartySize       = dto.PartySize,
                Status          = "Pending"
            };
            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
            return reservation;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var res = await _db.Reservations.FindAsync(id);
            if (res == null) return false;

            res.Status = status;

            // Update table status accordingly
            var table = await _db.RestaurantTables.FindAsync(res.TableID);
            if (table != null)
            {
                table.Status = status == "Confirmed"  ? "Reserved"  :
                               status == "Cancelled"  ? "Available" :
                               status == "Completed"  ? "Available" : table.Status;
            }

            await _db.SaveChangesAsync();
            return true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2. OrderProcessor
    // ─────────────────────────────────────────────────────────────────────
    public class OrderProcessor
    {
        private readonly RestaurantDbContext _db;
        private readonly LoggerService       _logger;

        public OrderProcessor(RestaurantDbContext db, LoggerService logger)
        {
            _db     = db;
            _logger = logger;
        }

        public async Task<Order> PlaceOrderAsync(CreateOrderDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Create order header
                var order = new Order
                {
                    CustomerID  = dto.CustomerID,
                    TableID     = dto.TableID,
                    StaffID     = dto.StaffID,
                    Status      = "Placed",
                    TotalAmount = 0
                };
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                // 2. Add order items
                decimal total = 0;
                foreach (var item in dto.Items)
                {
                    var menuItem = await _db.MenuItems.FindAsync(item.MenuItemID)
                        ?? throw new Exception($"MenuItem {item.MenuItemID} not found.");

                    var orderItem = new OrderItem
                    {
                        OrderID    = order.OrderID,
                        MenuItemID = item.MenuItemID,
                        Quantity   = item.Quantity,
                        UnitPrice  = menuItem.Price
                    };
                    _db.OrderItems.Add(orderItem);
                    total += menuItem.Price * item.Quantity;
                }

                // 3. Update order total & table status
                order.TotalAmount = total;
                var table = await _db.RestaurantTables.FindAsync(dto.TableID);
                if (table != null) table.Status = "Occupied";

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                await _logger.LogAsync("Info", "OrderProcessor",
                    $"Order #{order.OrderID} placed successfully.");

                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logger.LogAsync("Error", "OrderProcessor", ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;

            // Free table when order is paid or cancelled
            if (status == "Paid" || status == "Cancelled")
            {
                var table = await _db.RestaurantTables.FindAsync(order.TableID);
                if (table != null) table.Status = "Available";
            }

            await _db.SaveChangesAsync();
            return true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 3. PaymentService
    // ─────────────────────────────────────────────────────────────────────
    public class PaymentService
    {
        private readonly RestaurantDbContext _db;
        private readonly LoggerService       _logger;

        public PaymentService(RestaurantDbContext db, LoggerService logger)
        {
            _db     = db;
            _logger = logger;
        }

        public async Task<Payment> ProcessPaymentAsync(CreatePaymentDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var order = await _db.Orders.FindAsync(dto.OrderID)
                    ?? throw new Exception("Order not found.");

                if (order.Status == "Paid")
                    throw new Exception("Order already paid.");

                var payment = new Payment
                {
                    OrderID       = dto.OrderID,
                    Amount        = dto.Amount,
                    PaymentMethod = dto.PaymentMethod,
                    Status        = "Completed"
                };
                _db.Payments.Add(payment);

                order.Status = "Paid";
                var table = await _db.RestaurantTables.FindAsync(order.TableID);
                if (table != null) table.Status = "Available";

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                await _logger.LogAsync("Info", "PaymentService",
                    $"Payment processed for Order #{dto.OrderID}.");

                return payment;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logger.LogAsync("Error", "PaymentService", ex.Message);
                throw;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 4. ReportGenerator
    // ─────────────────────────────────────────────────────────────────────
    public class ReportGenerator
    {
        private readonly RestaurantDbContext _db;
        public ReportGenerator(RestaurantDbContext db) => _db = db;

        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime start, DateTime end)
        {
            var result = await _db.Orders
                .Where(o => o.Status == "Paid"
                         && o.OrderDate.Date >= start.Date
                         && o.OrderDate.Date <= end.Date)
                .GroupBy(o => 1)
                .Select(g => new RevenueReportDto
                {
                    TotalRevenue = g.Sum(o => o.TotalAmount),
                    TotalOrders  = g.Count(),
                    StartDate    = start,
                    EndDate      = end
                }).FirstOrDefaultAsync();

            return result ?? new RevenueReportDto { StartDate = start, EndDate = end };
        }

        public async Task<List<MenuItemDto>> GetTopSellingItemsAsync(int top = 10)
        {
            return await _db.OrderItems
                .Include(oi => oi.MenuItem).ThenInclude(m => m.Category)
                .GroupBy(oi => new { oi.MenuItemID, oi.MenuItem.ItemName, oi.MenuItem.Category.CategoryName, oi.MenuItem.Price })
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Take(top)
                .Select(g => new MenuItemDto
                {
                    MenuItemID   = g.Key.MenuItemID,
                    ItemName     = g.Key.ItemName,
                    CategoryName = g.Key.CategoryName,
                    Price        = g.Key.Price
                }).ToListAsync();
        }

        public async Task<List<InventoryItemDto>> GetLowStockItemsAsync()
        {
            return await _db.InventoryItems
                .Where(i => i.QuantityInStock <= i.ReorderLevel)
                .Select(i => new InventoryItemDto
                {
                    InventoryItemID = i.InventoryItemID,
                    ItemName        = i.ItemName,
                    QuantityInStock = i.QuantityInStock,
                    ReorderLevel    = i.ReorderLevel,
                    Unit            = i.Unit
                }).ToListAsync();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 5. InventoryService
    // ─────────────────────────────────────────────────────────────────────
    public class InventoryService
    {
        private readonly RestaurantDbContext _db;
        public InventoryService(RestaurantDbContext db) => _db = db;

        public async Task<InventoryItem> AddItemAsync(CreateInventoryItemDto dto)
        {
            var item = new InventoryItem
            {
                ItemName        = dto.ItemName,
                QuantityInStock = dto.QuantityInStock,
                Unit            = dto.Unit,
                ReorderLevel    = dto.ReorderLevel,
                VendorID        = dto.VendorID
            };
            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task RestockAsync(int id, decimal quantity)
        {
            var item = await _db.InventoryItems.FindAsync(id)
                ?? throw new Exception("Inventory item not found.");
            item.QuantityInStock += quantity;
            await _db.SaveChangesAsync();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 6. LoggerService (writes to AuditLog table)
    // ─────────────────────────────────────────────────────────────────────
    public class LoggerService
    {
        private readonly RestaurantDbContext _db;
        public LoggerService(RestaurantDbContext db) => _db = db;

        public async Task LogAsync(string level, string source, string message)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                LogLevel  = level,
                Source    = source,
                Message   = message,
                CreatedOn = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 7. AuthService (login + password hashing)
    // ─────────────────────────────────────────────────────────────────────
    public class AuthService
    {
        private readonly RestaurantDbContext _db;
        public AuthService(RestaurantDbContext db) => _db = db;

        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string password, string hash) =>
            BCrypt.Net.BCrypt.Verify(password, hash);

        public async Task<Customer?> AuthenticateCustomerAsync(string email, string password)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null) return null;
            return VerifyPassword(password, customer.PasswordHash) ? customer : null;
        }

        public async Task<Staff?> AuthenticateStaffAsync(string email, string password)
        {
            var staff = await _db.Staff.Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Email == email);
            if (staff == null) return null;
            return VerifyPassword(password, staff.PasswordHash) ? staff : null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 8. ValidatorService (input validation helpers)
    // ─────────────────────────────────────────────────────────────────────
    public static class ValidatorService
    {
        private static readonly string[] ValidOrderStatuses =
            { "Placed", "Preparing", "Served", "Paid", "Cancelled" };

        private static readonly string[] ValidPaymentMethods =
            { "Cash", "Card", "Online" };

        public static bool IsValidOrderStatus(string status) =>
            ValidOrderStatuses.Contains(status);

        public static bool IsValidPaymentMethod(string method) =>
            ValidPaymentMethods.Contains(method);

        public static bool IsValidPhone(string phone) =>
            phone.Length >= 10 && phone.All(c => char.IsDigit(c) || c == '+' || c == '-');

        public static bool IsValidReservationTime(string time) =>
            TimeSpan.TryParse(time, out _);
    }
}
