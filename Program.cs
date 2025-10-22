
using Microsoft.AspNetCore.Identity.UI.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SparePartsWeb.Data;
using SparePartsWeb.Models;
using SparePartsWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1️⃣ Add MVC (Controllers + Views)
// ----------------------------------------------------
builder.Services.AddControllersWithViews();

// ----------------------------------------------------
// 2️⃣ Configure MySQL Database
// ----------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 30))
    )
);

// ----------------------------------------------------
// 3️⃣ Configure ASP.NET Core Identity
// ----------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // ✅ enables built-in Identity UI

// ----------------------------------------------------
// 4️⃣ Configure Application Cookie
// ----------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ----------------------------------------------------
// 5️⃣ Register Email Service
// ----------------------------------------------------
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Register the email sender
builder.Services.AddScoped<IEmailSender, EmailService>();
// ----------------------------------------------------
// 6️⃣ Build the app
// ----------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------
// 7️⃣ Configure Middleware Pipeline
// ----------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ✅ ensure static files load (CSS/JS/images)

app.UseRouting();

app.UseAuthentication(); // must come before authorization
app.UseAuthorization();

app.MapRazorPages(); // for Identity
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// ----------------------------------------------------
// 8️⃣ Seed Roles & Default Admin User
// ----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error seeding roles: {ex.Message}");
    }
}

// ----------------------------------------------------
// 9️⃣ Run the app
// ----------------------------------------------------
await app.RunAsync();
