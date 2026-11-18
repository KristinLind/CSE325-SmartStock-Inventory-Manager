
# SmartStock Inventory Manager

## CSE325 – Group Project

## Team Members — CSE 325 Team 03

- **Kristin Lind** — Week 02 Group Leader
- **Isaac Miti** — Team Member
- **Raymond Mukonda** — Team Member
- **Rex Jonathan Kapoloma** — Team Member

## Team Collaboration

Our team will collaborate to plan, design, develop, and test a .NET Blazor web application for CSE 325.

We will use GitHub for version control, feature branches, and pull requests. Trello will help us organize assignments, track progress, and coordinate development.

Our goal is to create a functional application while ensuring that every team member has opportunities to contribute to the project.

### Project Overview
SmartStock is an inventory management application designed for small online retail businesses specializing in home and kitchen products.

The application will help businesses organize products, track inventory quantities, record sales, and identify items that need restocking.

### Product Categories
- Small kitchen appliances
- Cookware and utensils
- Food storage and organization
- Home essentials and accessories

### Planned Features
- Add, edit, and delete products
- Organize products by category
- Track quantities and product prices
- Search and filter inventory
- Record sales and update stock levels
- Display low-stock alerts
- View inventory summaries and reports

### Project Goals
Develop a user-friendly inventory management system that helps small businesses maintain accurate inventory records and improve daily operations.

### Development
This is a collaborative CSE325 student project. Technologies, responsibilities, and additional features will be finalized by the team.

---

## Tech Stack & Dependencies

- **Framework:** [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (ASP.NET Core Blazor Web App — Interactive Server Render Mode)
- **Database:** SQLite (`smartstock.db`, automatically created and seeded on first run)
- **NuGet Packages:**
  - `Microsoft.EntityFrameworkCore.Sqlite` (`10.0.0`) — ORM and SQLite database provider
  - `SQLitePCLRaw.bundle_e_sqlite3` (`2.1.12`) — Native SQLite library bundle
- **Styling:** Custom CSS + Bootstrap 5 (included in `wwwroot/lib/bootstrap`)

## Project Structure

```text
CSE325-SmartStock-Inventory-Manager/
├── Data/
│   └── SmartStockDbContext.cs       # EF Core DbContext & initial category seed data
├── Models/
│   ├── Category.cs                  # Product category model
│   ├── Product.cs                   # Inventory product model & stock status logic
│   ├── Sale.cs                      # Recorded sale transaction model
│   └── User.cs                      # User account model for authentication
├── Services/
│   ├── AuthService.cs               # User sign-in, registration, and session management
│   ├── CategoryService.cs           # Category CRUD operations
│   ├── ProductService.cs            # Product CRUD, filtering, and summary stats
│   └── SaleService.cs               # Sales recording and revenue calculation
└── SmartStock.Web/
    ├── Components/
    │   ├── Layout/                  # MainLayout & NavMenu components
    │   ├── Pages/                   # Blazor pages (Dashboard, Inventory, Products, Categories, Sales, Login)
    │   └── StockBadge.razor         # Reusable stock status badge
    ├── Properties/
    │   └── launchSettings.json      # Local development URLs and ports
    ├── wwwroot/                     # Static assets, product images, and stylesheets
    ├── Program.cs                   # App configuration, DI registration, and DB initialization
    └── SmartStock.Web.csproj        # Project file and NuGet dependencies
```

## Getting Started (Run Locally)

### 1. Prerequisites
Make sure you have the **.NET 10.0 SDK** installed on your machine. Verify your installation by running:
```bash
dotnet --version
```

### 2. Clone the Repository
```bash
git clone <repository-url>
cd CSE325-SmartStock-Inventory-Manager
```

### 3. Restore Dependencies & Build
Restore the required NuGet packages and compile the project:
```bash
dotnet restore SmartStock.Web/SmartStock.Web.csproj
dotnet build SmartStock.Web/SmartStock.Web.csproj
```

### 4. Run the Application
Start the Blazor web server from the root directory:
```bash
dotnet run --project SmartStock.Web/SmartStock.Web.csproj
```
*(Alternatively, you can `cd SmartStock.Web` and run `dotnet watch` for hot-reload during development.)*

### 5. Open in Browser
Once the server starts, open your browser and navigate to:
- **HTTP:** `http://localhost:5252`
- **HTTPS:** `https://localhost:7126` (if running with the `https` launch profile: `dotnet run --project SmartStock.Web --launch-profile https`)

> **Note on Database Setup:** No manual migration step is required to start the app. On startup, `Program.cs` calls `db.Database.EnsureCreated()` and `AuthService.EnsureSeedDataAsync()`, which automatically creates `SmartStock.Web/smartstock.db`, seeds the starting product categories, and creates a default demo account.

### 6. Authentication & Demo Credentials
SmartStock allows **public read-only browsing** of the dashboard, inventory, products, categories, and sales history. Signing in is required to **add, edit, or delete** products/categories or **record sales**.

You can register a new account at `/login` (or `/register`), or sign in immediately using the seeded demo account:
- **Email:** `admin@smartstock.com`
- **Password:** `SmartStock123!`


