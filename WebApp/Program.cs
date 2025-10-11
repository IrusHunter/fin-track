using DotNetEnv;
using FinTrack.Models;
using FinTrack.Repositories;
using FinTrack.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

{
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(ApplicationDbContext.GetConnectionStringFromENV()));

    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();

    // Add services to the container.
    builder.Services.AddControllersWithViews();
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = $"http://localhost:{Environment.GetEnvironmentVariable("AUTH_PORT") ?? throw new Exception("AUTH_PORT is not specified in .env file")}/";
        options.ClientId = "mvc_client";                       // має співпадати з тим, що зареєстровано на IdentityServer
        options.ClientSecret = Environment.GetEnvironmentVariable("SECRET") ?? throw new Exception("SECRET is not specified in .env file");
        options.ResponseType = "code";                         // рекомендується PKCE + authorization code flow
        options.RequireHttpsMetadata = false;

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("roles");

        options.ClaimActions.MapJsonKey("role", "role");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };

        options.SaveTokens = true;

        options.CallbackPath = "/signin-oidc";
    });

builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("IsAccountant", policy =>
            policy.RequireClaim("role", "accountant", "admin"));
        options.AddPolicy("IsUser", policy =>
            policy.RequireClaim("role", "user", "accountant", "admin"));
        options.AddPolicy("IsAdmin", policy =>
            policy.RequireClaim("role", "admin"));
    });

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        Console.WriteLine("User is authenticated");
        foreach (var c in context.User.Claims)
        {
            Console.WriteLine($"{c.Type} = {c.Value}");
        }
    }
    else
    {
        Console.WriteLine("User is NOT authenticated");
    }

    await next();
});

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
