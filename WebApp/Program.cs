using DotNetEnv;
using FinTrack.Models;
using FinTrack.Repositories;
using FinTrack.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(ApplicationDbContext.GetConnectionStringFromENV())
    );

    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();

    // Add services to the container.
    builder.Services.AddControllersWithViews();
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENTID") ?? throw new Exception("GOOGLE_CLIENTID is not specified in .env file"); ;
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? throw new Exception("GOOGLE_CLIENT_SECRET is not specified in .env file"); ;
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

// builder.Services.AddAuthorization(options =>
//     {
//         options.AddPolicy("IsAccountant", policy =>
//             policy.RequireClaim("role", "accountant", "admin"));
//         options.AddPolicy("IsUser", policy =>
//             policy.RequireClaim("role", "user", "accountant", "admin"));
//         options.AddPolicy("IsAdmin", policy =>
//             policy.RequireClaim("role", "admin"));
//     });

var app = builder.Build();

// app.Use(async (context, next) =>
// {
//     if (context.User?.Identity?.IsAuthenticated == true)
//     {
//         Console.WriteLine("User is authenticated");
//         foreach (var c in context.User.Claims)
//         {
//             Console.WriteLine($"{c.Type} = {c.Value}");
//         }
//     }
//     else
//     {
//         Console.WriteLine("User is NOT authenticated");
//     }

//     await next();
// });

// DB Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("MAIN_PORT") ?? throw new Exception("MAIN_PORT is not specified in .env file");
app.Run($"http://localhost:{port}");
