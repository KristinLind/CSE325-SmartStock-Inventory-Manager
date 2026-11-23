using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartStock.Data;
using SmartStock.Services;
using SmartStock.Web.Components;

var usCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = usCulture;
CultureInfo.DefaultThreadCurrentUICulture = usCulture;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<SmartStockDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(rawConnectionString) &&
        (rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
         rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
         rawConnectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)))
    {
        options.UseNpgsql(BuildNpgsqlConnectionString(rawConnectionString));
    }
    else
    {
        options.UseSqlite("Data Source=smartstock.db");
    }
});

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CartService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Create the database tables (and the starting categories, products, + demo user) on first run
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SmartStockDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.EnsureCreated();
    await AuthService.EnsureSeedDataAsync(factory);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static void LoadDotEnv()
{
    string[] candidatePaths =
    [
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
        Path.Combine(AppContext.BaseDirectory, ".env"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env")
    ];

    foreach (var path in candidatePaths)
    {
        if (!File.Exists(path))
            continue;

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"', '\'');

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
        break;
    }
}

static string BuildNpgsqlConnectionString(string input)
{
    if (!input.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !input.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return input;
    }

    var uri = new Uri(input);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var database = uri.AbsolutePath.Trim('/');

    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = database,
        Username = username,
        Password = password,
        SslMode = SslMode.Require,
        ChannelBinding = input.Contains("channel_binding=require", StringComparison.OrdinalIgnoreCase)
            ? ChannelBinding.Require
            : ChannelBinding.Prefer
    };

    return csb.ConnectionString;
}
