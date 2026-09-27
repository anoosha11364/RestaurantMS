-- =====================================================================
--  RestaurantMS — Seed Data
--  Run this AFTER `dotnet ef database update` has created the tables.
--  Usage: mysql -u root -p RestaurantDB < seed.sql
-- =====================================================================

USE RestaurantDB;

-- ── Roles (IDs match the dropdown in admin-staff.html) ───────────────
INSERT INTO `Role` (`RoleID`, `RoleName`) VALUES
  (1, 'Manager'),
  (2, 'Waiter'),
  (3, 'Chef'),
  (4, 'Cashier');

-- ── Menu Categories ────────────────────────────────────────────────
INSERT INTO `MenuCategories` (`CategoryID`, `CategoryName`) VALUES
  (1, 'Starters'),
  (2, 'Main Course'),
  (3, 'Beverages'),
  (4, 'Desserts');

-- ── Demo Admin / Staff Login ──────────────────────────────────────
-- Email: admin@restaurant.com   Password: Admin@123
INSERT INTO `Staff` (`FullName`, `Email`, `PasswordHash`, `Phone`, `RoleID`, `HireDate`, `IsActive`) VALUES
  ('Admin User', 'admin@restaurant.com', '$2b$11$7nDskPsTnOY1Uk7x.Eqmf.PPu1uj3m7gGRAUNIyYUQFo8kZZAN1Ou', '03001234567', 1, NOW(), 1);

-- ── Demo Customer Login ────────────────────────────────────────────
-- Email: customer@example.com   Password: Customer@123
INSERT INTO `Customers` (`FullName`, `Email`, `PasswordHash`, `Phone`, `RegisteredOn`) VALUES
  ('Demo Customer', 'customer@example.com', '$2b$11$elvh8hM4WhZJpA0XLf39mOFu7geGigJ6l1qAuvWggyt1Vn4Kho/F2', '03007654321', NOW());

-- ── Sample Tables ──────────────────────────────────────────────────
INSERT INTO `RestaurantTables` (`TableNumber`, `Capacity`, `Status`) VALUES
  ('T1', 2, 'Available'),
  ('T2', 4, 'Available'),
  ('T3', 4, 'Available'),
  ('T4', 6, 'Available'),
  ('T5', 2, 'Available'),
  ('T6', 8, 'Available');

-- ── Sample Menu Items ──────────────────────────────────────────────
INSERT INTO `MenuItems` (`ItemName`, `CategoryID`, `Price`, `Description`, `IsAvailable`) VALUES
  ('Chicken Spring Rolls', 1, 450.00, 'Crispy rolls filled with spiced chicken.', 1),
  ('Seekh Kebab', 1, 550.00, 'Grilled minced beef skewers.', 1),
  ('Chicken Biryani', 2, 650.00, 'Fragrant basmati rice with spiced chicken.', 1),
  ('Beef Karahi', 2, 950.00, 'Traditional wok-cooked beef curry.', 1),
  ('Grilled Fish', 2, 850.00, 'Fresh fish grilled with herbs.', 1),
  ('Soft Drink', 3, 120.00, 'Chilled 350ml soft drink.', 1),
  ('Fresh Lime Soda', 3, 150.00, 'Refreshing lime soda.', 1),
  ('Gulab Jamun', 4, 250.00, 'Sweet milk-solid dumplings in syrup.', 1),
  ('Kheer', 4, 220.00, 'Traditional rice pudding.', 1);

-- ── Sample Inventory Items (optional, for admin-inventory.html) ─────
INSERT INTO `InventoryItems` (`ItemName`, `QuantityInStock`, `Unit`, `ReorderLevel`) VALUES
  ('Chicken', 40.00, 'kg', 10.00),
  ('Beef', 25.00, 'kg', 10.00),
  ('Basmati Rice', 60.00, 'kg', 15.00),
  ('Cooking Oil', 30.00, 'liters', 10.00),
  ('Soft Drink Bottles', 100.00, 'pcs', 20.00);
