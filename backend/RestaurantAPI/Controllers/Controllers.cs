using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.DTOs;
using RestaurantAPI.Models;
using RestaurantAPI.Services;

namespace RestaurantAPI.Controllers
{
    // ─────────────────────────────────────────────────────────────────────
    // AUTH CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly RestaurantDbContext _db;

        public AuthController(AuthService authService, RestaurantDbContext db)
        {
            _authService = authService;
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CustomerRegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _db.Customers.AnyAsync(c => c.Email == dto.Email))
                return BadRequest(new { message = "Email already registered." });

            var customer = new Customer
            {
                FullName     = dto.FullName,
                Email        = dto.Email,
                Phone        = dto.Phone,
                PasswordHash = _authService.HashPassword(dto.Password)
            };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Registration successful.", customerId = customer.CustomerID });
        }

        [HttpPost("login/customer")]
        public async Task<IActionResult> CustomerLogin([FromBody] LoginDto dto)
        {
            var customer = await _authService.AuthenticateCustomerAsync(dto.Email, dto.Password);
            if (customer == null) return Unauthorized(new { message = "Invalid credentials." });

            return Ok(new AuthResponseDto
            {
                Token      = "jwt-token-placeholder",
                FullName   = customer.FullName,
                Role       = "Customer",
                CustomerID = customer.CustomerID
            });
        }

        [HttpPost("login/staff")]
        public async Task<IActionResult> StaffLogin([FromBody] LoginDto dto)
        {
            var staff = await _authService.AuthenticateStaffAsync(dto.Email, dto.Password);
            if (staff == null) return Unauthorized(new { message = "Invalid credentials." });

            return Ok(new AuthResponseDto
            {
                Token    = "jwt-token-placeholder",
                FullName = staff.FullName,
                Role     = staff.Role.RoleName,
                StaffID  = staff.StaffID
            });
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // CUSTOMERS CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        public CustomersController(RestaurantDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Customers.Select(c => new CustomerDto
            {
                CustomerID   = c.CustomerID,
                FullName     = c.FullName,
                Email        = c.Email,
                Phone        = c.Phone,
                RegisteredOn = c.RegisteredOn
            }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _db.Customers.FindAsync(id);
            if (c == null) return NotFound();
            return Ok(new CustomerDto
            {
                CustomerID   = c.CustomerID,
                FullName     = c.FullName,
                Email        = c.Email,
                Phone        = c.Phone,
                RegisteredOn = c.RegisteredOn
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Customers.FindAsync(id);
            if (c == null) return NotFound();
            _db.Customers.Remove(c);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // TABLES CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        public TablesController(RestaurantDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.RestaurantTables.Select(t => new RestaurantTableDto
            {
                TableID     = t.TableID,
                TableNumber = t.TableNumber,
                Capacity    = t.Capacity,
                Status      = t.Status
            }).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var table = new RestaurantTable
            {
                TableNumber = dto.TableNumber,
                Capacity    = dto.Capacity,
                Status      = "Available"
            };
            _db.RestaurantTables.Add(table);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = table.TableID }, table);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTableDto dto)
        {
            var table = await _db.RestaurantTables.FindAsync(id);
            if (table == null) return NotFound();
            table.TableNumber = dto.TableNumber;
            table.Capacity    = dto.Capacity;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _db.RestaurantTables.FindAsync(id);
            if (table == null) return NotFound();
            _db.RestaurantTables.Remove(table);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Available tables by date/time/partysize
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable(
            [FromQuery] string date,
            [FromQuery] string time,
            [FromQuery] int partySize,
            [FromServices] ReservationManager rm)
        {
            var result = await rm.GetAvailableTablesAsync(DateTime.Parse(date), time, partySize);
            return Ok(result);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // RESERVATIONS CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly ReservationManager _rm;

        public ReservationsController(RestaurantDbContext db, ReservationManager rm)
        {
            _db = db;
            _rm = rm;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Table)
                .Select(r => new ReservationDto
                {
                    ReservationID   = r.ReservationID,
                    CustomerName    = r.Customer.FullName,
                    TableNumber     = r.Table.TableNumber,
                    ReservationDate = r.ReservationDate,
                    ReservationTime = r.ReservationTime.ToString(@"hh\:mm"),
                    PartySize       = r.PartySize,
                    Status          = r.Status,
                    CreatedOn       = r.CreatedOn
                }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _db.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.ReservationID == id);
            if (r == null) return NotFound();
            return Ok(new ReservationDto
            {
                ReservationID   = r.ReservationID,
                CustomerName    = r.Customer.FullName,
                TableNumber     = r.Table.TableNumber,
                ReservationDate = r.ReservationDate,
                ReservationTime = r.ReservationTime.ToString(@"hh\:mm"),
                PartySize       = r.PartySize,
                Status          = r.Status,
                CreatedOn       = r.CreatedOn
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!ValidatorService.IsValidReservationTime(dto.ReservationTime))
                return BadRequest(new { message = "Invalid time format. Use HH:mm" });

            var reservation = await _rm.CreateReservationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = reservation.ReservationID }, reservation);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var success = await _rm.UpdateStatusAsync(id, dto.Status);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _db.Reservations.FindAsync(id);
            if (res == null) return NotFound();
            _db.Reservations.Remove(res);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // MENU CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        public MenuController(RestaurantDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.MenuItems.Include(m => m.Category)
                .Select(m => new MenuItemDto
                {
                    MenuItemID   = m.MenuItemID,
                    ItemName     = m.ItemName,
                    CategoryName = m.Category.CategoryName,
                    Price        = m.Price,
                    Description  = m.Description,
                    IsAvailable  = m.IsAvailable
                }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var m = await _db.MenuItems.Include(m => m.Category).FirstOrDefaultAsync(m => m.MenuItemID == id);
            if (m == null) return NotFound();
            return Ok(new MenuItemDto
            {
                MenuItemID   = m.MenuItemID,
                ItemName     = m.ItemName,
                CategoryName = m.Category.CategoryName,
                Price        = m.Price,
                Description  = m.Description,
                IsAvailable  = m.IsAvailable
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMenuItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var item = new MenuItem
            {
                ItemName    = dto.ItemName,
                CategoryID  = dto.CategoryID,
                Price       = dto.Price,
                Description = dto.Description
            };
            _db.MenuItems.Add(item);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = item.MenuItemID }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateMenuItemDto dto)
        {
            var item = await _db.MenuItems.FindAsync(id);
            if (item == null) return NotFound();
            item.ItemName   = dto.ItemName;
            item.CategoryID = dto.CategoryID;
            item.Price      = dto.Price;
            item.Description = dto.Description;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.MenuItems.FindAsync(id);
            if (item == null) return NotFound();
            _db.MenuItems.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories() =>
            Ok(await _db.MenuCategories.ToListAsync());
    }

    // ─────────────────────────────────────────────────────────────────────
    // ORDERS CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly OrderProcessor _processor;

        public OrdersController(RestaurantDbContext db, OrderProcessor processor)
        {
            _db        = db;
            _processor = processor;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.Staff)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Select(o => new OrderDto
                {
                    OrderID      = o.OrderID,
                    CustomerName = o.Customer.FullName,
                    TableNumber  = o.Table.TableNumber,
                    StaffName    = o.Staff != null ? o.Staff.FullName : null,
                    OrderDate    = o.OrderDate,
                    Status       = o.Status,
                    TotalAmount  = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ItemName  = oi.MenuItem.ItemName,
                        Quantity  = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal  = oi.Subtotal
                    }).ToList()
                }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var o = await _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .Include(o => o.Staff)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (o == null) return NotFound();
            return Ok(new OrderDto
            {
                OrderID      = o.OrderID,
                CustomerName = o.Customer.FullName,
                TableNumber  = o.Table.TableNumber,
                StaffName    = o.Staff?.FullName,
                OrderDate    = o.OrderDate,
                Status       = o.Status,
                TotalAmount  = o.TotalAmount,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ItemName  = oi.MenuItem.ItemName,
                    Quantity  = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal  = oi.Subtotal
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var order = await _processor.PlaceOrderAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = order.OrderID }, order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            if (!ValidatorService.IsValidOrderStatus(dto.Status))
                return BadRequest(new { message = "Invalid order status." });

            var success = await _processor.UpdateOrderStatusAsync(id, dto.Status);
            if (!success) return NotFound();
            return NoContent();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // PAYMENTS CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly PaymentService _paymentService;

        public PaymentsController(RestaurantDbContext db, PaymentService paymentService)
        {
            _db             = db;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Payments.Select(p => new PaymentDto
            {
                PaymentID     = p.PaymentID,
                OrderID       = p.OrderID,
                Amount        = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentDate   = p.PaymentDate,
                Status        = p.Status
            }).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!ValidatorService.IsValidPaymentMethod(dto.PaymentMethod))
                return BadRequest(new { message = "Invalid payment method. Use Cash, Card, or Online." });

            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(dto);
                return Ok(new PaymentDto
                {
                    PaymentID     = payment.PaymentID,
                    OrderID       = payment.OrderID,
                    Amount        = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentDate   = payment.PaymentDate,
                    Status        = payment.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // STAFF CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly AuthService _authService;

        public StaffController(RestaurantDbContext db, AuthService authService)
        {
            _db          = db;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Staff.Include(s => s.Role).Select(s => new StaffDto
            {
                StaffID  = s.StaffID,
                FullName = s.FullName,
                Email    = s.Email,
                Phone    = s.Phone,
                RoleName = s.Role.RoleName,
                IsActive = s.IsActive
            }).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStaffDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var staff = new Staff
            {
                FullName     = dto.FullName,
                Email        = dto.Email,
                Phone        = dto.Phone,
                RoleID       = dto.RoleID,
                PasswordHash = _authService.HashPassword(dto.Password)
            };
            _db.Staff.Add(staff);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Staff created.", staffId = staff.StaffID });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.Staff.FindAsync(id);
            if (s == null) return NotFound();
            s.IsActive = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // INVENTORY CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly InventoryService _inventoryService;

        public InventoryController(RestaurantDbContext db, InventoryService inventoryService)
        {
            _db               = db;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.InventoryItems.Include(i => i.Vendor).Select(i => new InventoryItemDto
            {
                InventoryItemID = i.InventoryItemID,
                ItemName        = i.ItemName,
                QuantityInStock = i.QuantityInStock,
                Unit            = i.Unit,
                ReorderLevel    = i.ReorderLevel,
                VendorName      = i.Vendor != null ? i.Vendor.VendorName : null
            }).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInventoryItemDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var item = await _inventoryService.AddItemAsync(dto);
            return Ok(item);
        }

        [HttpPatch("{id}/restock")]
        public async Task<IActionResult> Restock(int id, [FromBody] decimal quantity)
        {
            try
            {
                await _inventoryService.RestockAsync(id, quantity);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // FEEDBACK CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        public FeedbackController(RestaurantDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Feedbacks.Include(f => f.Customer).Select(f => new FeedbackDto
            {
                FeedbackID   = f.FeedbackID,
                CustomerName = f.Customer.FullName,
                OrderID      = f.OrderID,
                Rating       = f.Rating,
                Comments     = f.Comments,
                SubmittedOn  = f.SubmittedOn
            }).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var fb = new Feedback
            {
                CustomerID = dto.CustomerID,
                OrderID    = dto.OrderID,
                Rating     = dto.Rating,
                Comments   = dto.Comments
            };
            _db.Feedbacks.Add(fb);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Feedback submitted.", feedbackId = fb.FeedbackID });
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // REPORTS CONTROLLER
    // ─────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportGenerator _reportGenerator;
        public ReportsController(ReportGenerator rg) => _reportGenerator = rg;

        [HttpGet("revenue")]
        public async Task<IActionResult> Revenue(
            [FromQuery] string startDate,
            [FromQuery] string endDate)
        {
            var result = await _reportGenerator.GetRevenueReportAsync(
                DateTime.Parse(startDate), DateTime.Parse(endDate));
            return Ok(result);
        }

        [HttpGet("top-items")]
        public async Task<IActionResult> TopItems([FromQuery] int top = 10) =>
            Ok(await _reportGenerator.GetTopSellingItemsAsync(top));

        [HttpGet("low-stock")]
        public async Task<IActionResult> LowStock() =>
            Ok(await _reportGenerator.GetLowStockItemsAsync());
    }
}
