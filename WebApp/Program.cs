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
using System.IO;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

try
{
    var envPath = Path.Combine(builder.Environment.ContentRootPath, "..", ".env");

    if (File.Exists(envPath))
    {
        DotNetEnv.Env.Load(envPath);
    }
    else
    {
        DotNetEnv.Env.Load();
    }
}
catch (Exception ex)
{
    // Ігноруємо помилку завантаження файлу, якщо вона не критична
}

{
    var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? throw new Exception("DB_PROVIDER is not specified in .env file");

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
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
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

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(
        serviceName: "fin-track-webapp"))
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(); // endpoint береться з OTEL_EXPORTER_OTLP_ENDPOINT
    })
    .WithMetrics(metricProviderBuilder =>
    {
        metricProviderBuilder
            .AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    });


var app = builder.Build();





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

    //     // =======================================================
    //     {

    //         var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //         Console.WriteLine("Ensuring database...");
    //         context.Database.EnsureCreated();

    //         // var store = new UserStore<ApplicationUser>(context);
    //         var hasher = new PasswordHasher<ApplicationUser>();


    //         // ---------------------------
    //         // 1️⃣ Seed Users
    //         // ---------------------------
    //         Console.WriteLine("Seeding users...");
    //         var users = new List<ApplicationUser>();

    //         for (int i = 1; i <= 20; i++)
    //         {
    //             var user = new ApplicationUser
    //             {
    //                 Id = Guid.NewGuid().ToString(),
    //                 UserName = $"user{i}",
    //                 Email = $"user{i}@example.com",
    //                 FullName = $"Тестовий Користувач {i}",
    //                 PhoneNumber = $"+38050{i:0000000}"
    //             };

    //             user.PasswordHash = hasher.HashPassword(user, "Qwerty123!");

    //             users.Add(user);
    //         }

    //         context.Users.AddRange(users);
    //         await context.SaveChangesAsync();
    //         Console.WriteLine($"Added {users.Count} users");


    //         // ---------------------------
    //         // 2️⃣ Seed Categories
    //         // ---------------------------
    //         Console.WriteLine("Seeding categories...");
    //         var rnd = new Random();
    //         var categories = new List<Category>();

    //         string[] categoryNames =
    //         {
    //     "Продукти", "Транспорт", "Квартира", "Здоровʼя", "Розваги",
    //     "Комунальні послуги", "Одяг", "Освіта", "Подарунки", "Податки",
    //     "Бізнес витрати", "Спорт", "Подорожі", "Паливо", "Кафе",
    // };

    //         int id = 1;

    //         foreach (var name in categoryNames)
    //         {
    //             categories.Add(new Category
    //             {
    //                 Name = name,
    //                 TaxAmount = Math.Round((decimal)(rnd.NextDouble() * 20), 2),
    //                 TaxType = rnd.Next(0, 2) == 0 ? TaxType.GeneralTax : TaxType.ExpenseTax
    //             });
    //         }

    //         context.Categories.AddRange(categories);
    //         await context.SaveChangesAsync();
    //         Console.WriteLine($"Added {categories.Count} categories");


    //         // ---------------------------
    //         // 3️⃣ Seed Transactions (20–30k)
    //         // ---------------------------
    //         Console.WriteLine("Seeding transactions...");

    //         var transactions = new List<Transaction>();
    //         int total = 25000;
    //         int batch = 2000;

    //         for (int i = 1; i <= total; i++)
    //         {
    //             var category = categories[rnd.Next(categories.Count)];
    //             var user = users[rnd.Next(users.Count)];

    //             decimal sum = Math.Round((decimal)(rnd.NextDouble() * 5000 + 10), 2);
    //             decimal taxMultiplier = 1 + category.TaxAmount / 100;

    //             transactions.Add(new Transaction
    //             {
    //                 Name = $"Транзакція #{i}",
    //                 CategoryId = category.Id,
    //                 UserId = user.Id,
    //                 Sum = sum,
    //                 SumAfterTax = Math.Round(sum * taxMultiplier, 2),
    //                 CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 365)),
    //                 UpdatedAt = DateTime.UtcNow
    //             });

    //             if (i % batch == 0)
    //             {
    //                 context.Transactions.AddRange(transactions);
    //                 await context.SaveChangesAsync();
    //                 Console.WriteLine($"Inserted {i}/{total}");
    //                 transactions.Clear();
    //             }
    //         }

    //         if (transactions.Any())
    //         {
    //             context.Transactions.AddRange(transactions);
    //             await context.SaveChangesAsync();
    //         }

    //         Console.WriteLine("==== Done! ====");
    //     }
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
app.Run($"http://0.0.0.0:{port}");

public partial class Program { }
