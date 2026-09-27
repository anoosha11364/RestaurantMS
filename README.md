# 🍽️ RestaurantMS — Restaurant Management System

A full-stack Restaurant Management System (DBMS project):
- **Backend:** ASP.NET Core 8 Web API + Entity Framework Core + MySQL
- **Frontend:** Plain HTML / CSS / JavaScript (no framework, no build step)

```
RestaurantMS/
├── backend/
│   └── RestaurantAPI/
│       ├── Controllers/        → All API endpoints
│       ├── Services/           → Business logic (reservations, orders, payments, reports...)
│       ├── Models/             → Entity classes (mapped to MySQL tables via EF Core)
│       ├── DTOs/                → Request/response shapes used by the API
│       ├── Data/                → RestaurantDbContext (EF Core)
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Program.cs
│       ├── appsettings.json    → ⚠️ Set your MySQL password here
│       ├── seed.sql            → Sample data (roles, categories, tables, menu, demo logins)
│       └── RestaurantAPI.csproj
└── frontend/
    ├── index.html               → Login page
    ├── css/style.css
    ├── js/utils.js               → API base URL + shared helpers
    └── pages/                    → All customer + admin pages
```

---

## 1. Prerequisites

Install these before starting:
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) (running locally)
- VS Code with the **C# Dev Kit** extension (or any editor)
- A simple way to serve static HTML — the built-in **Live Server** VS Code extension is easiest

---

## 2. Database Setup

1. Open MySQL and create the database:
   ```sql
   CREATE DATABASE RestaurantDB;
   ```

2. Open `backend/RestaurantAPI/appsettings.json` and set **your** MySQL username/password:
   ```json
   "DefaultConnection": "Server=localhost;Port=3306;Database=RestaurantDB;User=root;Password=YOUR_PASSWORD_HERE;"
   ```

3. From a terminal, install the EF Core CLI tool (one-time, only if you don't have it):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. Move into the backend project and create the database tables from the C# models:
   ```bash
   cd backend/RestaurantAPI
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
   This reads your `Models/` + `RestaurantDbContext.cs` and creates all tables (Staff, Customers, Orders, MenuItems, etc.) in `RestaurantDB`.

5. Load sample/demo data (roles, menu, tables, and test logins):
   ```bash
   mysql -u root -p RestaurantDB < seed.sql
   ```

---

## 3. Run the Backend

```bash
cd backend/RestaurantAPI
dotnet restore
dotnet run
```

The API will start at **http://localhost:5000** (fixed via `launchSettings.json` so it always matches the frontend). You can browse the interactive API docs at:

```
http://localhost:5000/swagger
```

---

## 4. Run the Frontend

The frontend is plain HTML/CSS/JS — no build step needed. Easiest way in VS Code:

1. Install the **Live Server** extension.
2. Right-click `frontend/index.html` → **Open with Live Server**.
3. It'll open something like `http://127.0.0.1:5500/frontend/index.html`.

Make sure the backend (step 3) is running at the same time — the frontend calls it at `http://localhost:5000/api` (set in `frontend/js/utils.js`, change this if your API runs on a different port).

> CORS is already configured in `Program.cs` to accept requests from any local origin during development, so Live Server, VS Code's built-in preview, or any other local port will work without extra setup.

---

## 5. Demo Logins (from `seed.sql`)

| Role     | Email                  | Password      |
|----------|-------------------------|----------------|
| Admin/Staff | admin@restaurant.com | Admin@123      |
| Customer | customer@example.com   | Customer@123   |

You can also register a new customer account from the login page ("Create account").

---

## 6. Pages Overview

**Customer:** Dashboard → Reservations → Menu → Place Order → Feedback
**Admin:** Dashboard → Tables → Menu → Orders → Staff → Inventory → Reports

---

## 7. Notes / Known Limitations (worth knowing for a DBMS project)

- Login returns a placeholder token (no real JWT/session yet) — fine for a course project, but not production auth.
- `Vendor` and `Role` records must be created directly in MySQL (via `seed.sql` or Workbench) — there's no admin UI for those yet, since the backend doesn't expose list endpoints for them.
- Passwords are hashed with BCrypt (`BCrypt.Net-Next`) before storage — never stored in plain text.

## 8. What was fixed in this version

- **Real customer identity on login.** `AuthResponseDto` now returns `CustomerID` (for customers) and `StaffID` (for staff) from the login endpoints. The frontend (`order.html`, `reservation.html`, `feedback.html`) now uses the actual logged-in customer's ID instead of a hardcoded `1` — every customer's orders, reservations, and feedback are now correctly attributed to them.
- **MySQL connection string** in `appsettings.json` is set to your database password.
- **CORS** widened to accept any local origin so Live Server / VS Code preview work without extra config.
- **`launchSettings.json`** added so the API always starts on a fixed port (`http://localhost:5000`), matching what the frontend expects.
- **`seed.sql`** added with roles, categories, sample tables/menu/inventory, and two ready-to-use demo logins.

