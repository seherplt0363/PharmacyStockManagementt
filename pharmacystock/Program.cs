using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using pharmacystock.Models;

using PharmacyStock.DataAccess.Repositories.Concrete;
using PharmacyStock.DataAccess.Repositories.Interfaces;

using ApplicationDbContext = PharmacyStock.DataAccess.Context.ApplicationDbContext;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();


// =====================================================
// CONNECTION STRING
// =====================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");


// =====================================================
// DATABASE CONTEXTS
// =====================================================

// Yeni katmanlı mimarinin DbContext'i
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});


// =====================================================
// DATA ACCESS
// =====================================================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// =====================================================
// YENİ BUSINESS SERVİSLERİ
// =====================================================

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IProductService,
    PharmacyStock.Business.Services.ProductService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IBrandService,
    PharmacyStock.Business.Services.BrandService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.ICategoryService,
    PharmacyStock.Business.Services.CategoryService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IStockTransactionService,
    PharmacyStock.Business.Services.StockTransactionService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.ISupplierService,
    PharmacyStock.Business.Services.SupplierService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IPurchaseOrderService,
    PharmacyStock.Business.Services.PurchaseOrderService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IDashboardService,
    PharmacyStock.Business.Services.DashboardService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IEmailService,
    PharmacyStock.Business.Services.EmailService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IOrderDraftService,
    PharmacyStock.Business.Services.OrderDraftService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IStockTurnoverService,
    PharmacyStock.Business.Services.StockTurnoverService>();

builder.Services.AddScoped<
    PharmacyStock.Business.Interfaces.IABCAnalysisService,
    PharmacyStock.Business.Services.ABCAnalysisService>();





// =====================================================
// SMTP CONFIGURATION
// =====================================================

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));


// =====================================================
// IDENTITY
// =====================================================

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// =====================================================
// COOKIE SETTINGS
// =====================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});


// =====================================================
// APPLICATION
// =====================================================

var app = builder.Build();


// =====================================================
// ERROR HANDLING
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// =====================================================
// HTTP PIPELINE
// =====================================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


// =====================================================
// ROUTING
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


// =====================================================
// IDENTITY SEED
// =====================================================
// =====================================================
// IDENTITY SEED
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        await PharmacyStock.Data.DbInitializer
            .SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Identity seed işlemi sırasında hata oluştu: {ex.Message}");
    }
}


// =====================================================
// RUN
// =====================================================

app.Run();