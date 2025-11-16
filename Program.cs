
using Microsoft.AspNetCore.Identity.UI.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SparePartsWeb.Data;
using SparePartsWeb.Models;
using SparePartsWeb.Services;
using DotNetEnv;



var builder = WebApplication.CreateBuilder(args);


DotNetEnv.Env.Load(); // load .env file


var dbPassword = Environment.GetEnvironmentVariable("db_password");// get password from env file 
var connectionString = $"Server=localhost;Database=SparePartsDB;User=root;Password={dbPassword}"; 

builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 30))
    )
);


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
    
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); 


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});



// Register Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); //  ensure static files load (CSS/JS/images)

app.UseRouting();

app.UseAuthentication(); // must come before authorization
app.UseAuthorization();

app.MapRazorPages(); // for Identity
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);


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



await app.RunAsync();
