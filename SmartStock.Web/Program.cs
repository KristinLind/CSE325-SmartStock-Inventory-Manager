using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Services;
using SmartStock.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<SmartStockDbContext>(options =>
    options.UseSqlite("Data Source=smartstock.db"));
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<AuthService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Create the database file (and the starting categories + demo user) on first run
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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
