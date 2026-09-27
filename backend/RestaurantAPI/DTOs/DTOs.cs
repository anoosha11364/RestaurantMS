using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.DTOs
{
    // ─────────────────────────────────────────────
    // AUTH
    // ─────────────────────────────────────────────
    public class LoginDto
    {
        [Required, EmailAddress] public string Email    { get; set; } = string.Empty;
        [Required]               public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token      { get; set; } = string.Empty;
        public string FullName   { get; set; } = string.Empty;
        public string Role       { get; set; } = string.Empty;
        public int?   CustomerID { get; set; }
        public int?   StaffID    { get; set; }
    }

    // ─────────────────────────────────────────────
    // CUSTOMER
    // ─────────────────────────────────────────────
    public class CustomerRegisterDto
    {
        [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress]   public string Email    { get; set; } = string.Empty;
        [Required, MinLength(6)]   public string Password { get; set; } = string.Empty;
        [Required, MinLength(10)]  public string Phone    { get; set; } = string.Empty;
    }

    public class CustomerDto
    {
        public int    CustomerID   { get; set; }
        public string FullName     { get; set; } = string.Empty;
        public string Email        { get; set; } = string.Empty;
        public string Phone        { get; set; } = string.Empty;
        public DateTime RegisteredOn { get; set; }
    }

    // ─────────────────────────────────────────────
    // RESERVATION
    // ─────────────────────────────────────────────
    public class CreateReservationDto
    {
        [Required] public int      CustomerID       { get; set; }
        [Required] public int      TableID          { get; set; }
        [Required] public DateTime ReservationDate  { get; set; }
        [Required] public string   ReservationTime  { get; set; } = string.Empty; // "HH:mm"
        [Required, Range(1, 20)] public int PartySize { get; set; }
    }

    public class ReservationDto
    {
        public int      ReservationID    { get; set; }
        public string   CustomerName     { get; set; } = string.Empty;
        public string   TableNumber      { get; set; } = string.Empty;
        public DateTime ReservationDate  { get; set; }
        public string   ReservationTime  { get; set; } = string.Empty;
        public int      PartySize        { get; set; }
        public string   Status           { get; set; } = string.Empty;
        public DateTime CreatedOn        { get; set; }
    }

    // ─────────────────────────────────────────────
    // MENU
    // ─────────────────────────────────────────────
    public class MenuItemDto
    {
        public int     MenuItemID   { get; set; }
        public string  ItemName     { get; set; } = string.Empty;
        public string  CategoryName { get; set; } = string.Empty;
        public decimal Price        { get; set; }
        public string? Description  { get; set; }
        public bool    IsAvailable  { get; set; }
    }

    public class CreateMenuItemDto
    {
        [Required, MaxLength(100)] public string  ItemName    { get; set; } = string.Empty;
        [Required]                 public int     CategoryID  { get; set; }
        [Required, Range(1, 999999)] public decimal Price     { get; set; }
        [MaxLength(300)]           public string? Description { get; set; }
    }

    // ─────────────────────────────────────────────
    // ORDER
    // ─────────────────────────────────────────────
    public class CreateOrderDto
    {
        [Required] public int CustomerID { get; set; }
        [Required] public int TableID    { get; set; }
        public int? StaffID { get; set; }
        [Required] public List<OrderItemInputDto> Items { get; set; } = new();
    }

    public class OrderItemInputDto
    {
        [Required] public int MenuItemID { get; set; }
        [Required, Range(1, 100)] public int Quantity { get; set; }
    }

    public class OrderDto
    {
        public int      OrderID      { get; set; }
        public string   CustomerName { get; set; } = string.Empty;
        public string   TableNumber  { get; set; } = string.Empty;
        public string?  StaffName    { get; set; }
        public DateTime OrderDate    { get; set; }
        public string   Status       { get; set; } = string.Empty;
        public decimal  TotalAmount  { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string  ItemName   { get; set; } = string.Empty;
        public int     Quantity   { get; set; }
        public decimal UnitPrice  { get; set; }
        public decimal Subtotal   { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required] public string Status { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────
    // PAYMENT
    // ─────────────────────────────────────────────
    public class CreatePaymentDto
    {
        [Required] public int     OrderID       { get; set; }
        [Required] public decimal Amount        { get; set; }
        [Required] public string  PaymentMethod { get; set; } = string.Empty;
    }

    public class PaymentDto
    {
        public int      PaymentID     { get; set; }
        public int      OrderID       { get; set; }
        public decimal  Amount        { get; set; }
        public string   PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate   { get; set; }
        public string   Status        { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────
    // FEEDBACK
    // ─────────────────────────────────────────────
    public class CreateFeedbackDto
    {
        [Required] public int CustomerID { get; set; }
        public int? OrderID { get; set; }
        [Required, Range(1, 5)] public int Rating { get; set; }
        [MaxLength(500)] public string? Comments { get; set; }
    }

    public class FeedbackDto
    {
        public int      FeedbackID   { get; set; }
        public string   CustomerName { get; set; } = string.Empty;
        public int?     OrderID      { get; set; }
        public int      Rating       { get; set; }
        public string?  Comments     { get; set; }
        public DateTime SubmittedOn  { get; set; }
    }

    // ─────────────────────────────────────────────
    // INVENTORY
    // ─────────────────────────────────────────────
    public class InventoryItemDto
    {
        public int     InventoryItemID { get; set; }
        public string  ItemName        { get; set; } = string.Empty;
        public decimal QuantityInStock { get; set; }
        public string  Unit            { get; set; } = string.Empty;
        public decimal ReorderLevel    { get; set; }
        public string? VendorName      { get; set; }
    }

    public class CreateInventoryItemDto
    {
        [Required, MaxLength(100)] public string  ItemName        { get; set; } = string.Empty;
        [Required]                 public decimal QuantityInStock { get; set; }
        [Required, MaxLength(20)]  public string  Unit            { get; set; } = string.Empty;
        public decimal ReorderLevel { get; set; } = 10;
        public int?    VendorID     { get; set; }
    }

    // ─────────────────────────────────────────────
    // TABLE
    // ─────────────────────────────────────────────
    public class RestaurantTableDto
    {
        public int    TableID     { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int    Capacity    { get; set; }
        public string Status      { get; set; } = string.Empty;
    }

    public class CreateTableDto
    {
        [Required, MaxLength(10)] public string TableNumber { get; set; } = string.Empty;
        [Required, Range(1, 50)]  public int    Capacity    { get; set; }
    }

    // ─────────────────────────────────────────────
    // STAFF
    // ─────────────────────────────────────────────
    public class CreateStaffDto
    {
        [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress]   public string Email    { get; set; } = string.Empty;
        [Required, MinLength(6)]   public string Password { get; set; } = string.Empty;
        [Required, MinLength(10)]  public string Phone    { get; set; } = string.Empty;
        [Required]                 public int    RoleID   { get; set; }
    }

    public class StaffDto
    {
        public int    StaffID  { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public string Phone    { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool   IsActive { get; set; }
    }

    // ─────────────────────────────────────────────
    // REPORTS
    // ─────────────────────────────────────────────
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int     TotalOrders  { get; set; }
        public DateTime StartDate   { get; set; }
        public DateTime EndDate     { get; set; }
    }
}
