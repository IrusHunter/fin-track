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

    var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? throw new Exception("DB_PROVIDER is not specified in .env file"); ;

    switch (dbProvider)
    {
        case "sqlserver":
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(ApplicationDbContext.GetConnectionStringFromENV()));
            break;
        case "postgres":
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(ApplicationDbContext.GetConnectionStringFromENV()));
            break;
        case "memory":
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(ApplicationDbContext.GetConnectionStringFromENV()));
            break;
        case "sqlite":
        default:
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(ApplicationDbContext.GetConnectionStringFromENV()));
            break;
    }

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

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Explorer для Swagger
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Формат груп для Swagger: v1, v2
    options.SubstituteApiVersionInUrl = true; // важливо для {version:apiVersion} у маршрутах
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1.0", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FinTrack API",
        Version = "v1.0",
        Description = "RESTful API for FinTrack - Version 1.0"
    });

    options.SwaggerDoc("v2.0", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FinTrack API",
        Version = "v2.0",
        Description = "RESTful API for FinTrack - Version 2.0"
    });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var actionDescriptor = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (actionDescriptor == null) return false;

        var apiVersion = actionDescriptor.MethodInfo
            .GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Mvc.MapToApiVersionAttribute>()
            .FirstOrDefault();

        if (apiVersion == null) return false;

        return apiVersion.Versions.Any(v => $"v{v.ToString()}" == docName);
    });
});

var app = builder.Build();

// DB Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
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

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "FinTrack API v1.0");
    options.SwaggerEndpoint("/swagger/v2.0/swagger.json", "FinTrack API v2.0");
    options.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("MAIN_PORT") ?? throw new Exception("MAIN_PORT is not specified in .env file");
app.Run($"http://localhost:{port}");

public partial class Program { }
