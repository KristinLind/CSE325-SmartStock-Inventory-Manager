
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
- **Database:** Neon Cloud PostgreSQL (via `DATABASE_URL` in `.env`, with automatic local SQLite fallback `smartstock.db`)
- **NuGet Packages:**
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (`10.0.0`) — PostgreSQL / Neon cloud database provider
  - `Microsoft.EntityFrameworkCore.Sqlite` (`10.0.0`) — Local SQLite fallback provider
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

### 3. Configure Database Connection (Optional for Local SQLite / Required for Neon PostgreSQL)
To connect to **Neon Cloud PostgreSQL**, create a `.env` file in the root of the repository (ignored by Git):
```env
DATABASE_URL=postgresql://<username>:<password>@<endpoint>.neon.tech/<dbname>?sslmode=require&channel_binding=require
```
*(If no `.env` or `DATABASE_URL` is set, the application automatically falls back to a local SQLite database `smartstock.db`.)*

### 4. Restore Dependencies & Build
Restore the required NuGet packages and compile the project:
```bash
dotnet restore SmartStock.Web/SmartStock.Web.csproj
dotnet build SmartStock.Web/SmartStock.Web.csproj
```

### 5. Run the Application
Start the Blazor web server from the root directory:
```bash
dotnet run --project SmartStock.Web/SmartStock.Web.csproj
```
*(Alternatively, you can `cd SmartStock.Web` and run `dotnet watch` for hot-reload during development.)*

### 6. Open in Browser
Once the server starts, open your browser and navigate to:
- **HTTP:** `http://localhost:5252`
- **HTTPS:** `https://localhost:7126` (if running with the `https` launch profile: `dotnet run --project SmartStock.Web --launch-profile https`)

> **Note on Database Setup:** No manual migration step is required to start the app. On startup, `Program.cs` calls `db.Database.EnsureCreated()` and `AuthService.EnsureSeedDataAsync()`, which automatically creates all tables in Neon PostgreSQL (or SQLite), seeds the product categories and initial kitchen inventory items, and creates a default demo account.

### 7. Authentication & Demo Credentials
SmartStock allows **public read-only browsing** of the dashboard, inventory, products, categories, and sales history. Signing in is required to **add, edit, or delete** products/categories or **record sales**.

You can register a new account at `/login` (or `/register`), or sign in immediately using the seeded demo account:
**Administrator Access:** Login credentials are provided privately to authorized team members and are not published in this repository.

## Deploying on Render

The repository includes a root-level Dockerfile for deploying the ASP.NET Core app as a Render web service.

1. Create a **Web Service** from the original `KristinLind/CSE325-SmartStock-Inventory-Manager` repository.
2. Select the `main` branch and **Docker** as the runtime. Leave the root directory blank so Docker can access the shared `Data`, `Models`, and `Services` folders.
3. Add `DATABASE_URL` in the Render service's environment settings using the Neon PostgreSQL connection string. Do not commit the `.env` file or expose the connection string in client-side code.
4. Keep the service listening port at `10000`.
5. Enable automatic deploys from `main` so merged changes are deployed to production.


