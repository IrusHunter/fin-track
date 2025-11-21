using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FinTrack.Models;
using FinTrack;
using System.Security.Cryptography;
using System.Text;

Console.WriteLine("FinTrack Seeder");
Console.WriteLine("========================");

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? throw new Exception("DB_PROVIDER is not specified in .env file");

switch (dbProvider)
{
    case "sqlserver":
        optionsBuilder.UseSqlServer(ApplicationDbContext.GetConnectionStringFromENV());
        break;
    case "postgres":
        optionsBuilder.UseNpgsql(ApplicationDbContext.GetConnectionStringFromENV());
        break;
    case "memory":
        optionsBuilder.UseInMemoryDatabase(ApplicationDbContext.GetConnectionStringFromENV());
        break;
    case "sqlite":
    default:
        optionsBuilder.UseSqlite(ApplicationDbContext.GetConnectionStringFromENV());
        break;
}

using var context = new ApplicationDbContext(optionsBuilder.Options);

// Identity потребує UserManager та RoleManager
var store = new UserStore<ApplicationUser>(context);
var hasher = new PasswordHasher<ApplicationUser>();

Console.WriteLine("Ensuring database...");
context.Database.EnsureCreated();


// ---------------------------
// 1️⃣ Seed Users
// ---------------------------
Console.WriteLine("Seeding users...");
var users = new List<ApplicationUser>();

for (int i = 1; i <= 20; i++)
{
    var user = new ApplicationUser
    {
        Id = Guid.NewGuid().ToString(),
        UserName = $"user{i}",
        Email = $"user{i}@example.com",
        FullName = $"Тестовий Користувач {i}",
        PhoneNumber = $"+38050{i:0000000}"
    };

    user.PasswordHash = hasher.HashPassword(user, "Qwerty123!");

    users.Add(user);
}

context.Users.AddRange(users);
await context.SaveChangesAsync();
Console.WriteLine($"Added {users.Count} users");


// ---------------------------
// 2️⃣ Seed Categories
// ---------------------------
Console.WriteLine("Seeding categories...");
var rnd = new Random();
var categories = new List<Category>();

string[] categoryNames =
{
    "Продукти", "Транспорт", "Квартира", "Здоровʼя", "Розваги",
    "Комунальні послуги", "Одяг", "Освіта", "Подарунки", "Податки",
    "Бізнес витрати", "Спорт", "Подорожі", "Паливо", "Кафе",
};

int id = 1;

foreach (var name in categoryNames)
{
    categories.Add(new Category
    {
        Name = name,
        TaxAmount = Math.Round((decimal)(rnd.NextDouble() * 20), 2),
        TaxType = rnd.Next(0, 2) == 0 ? TaxType.GeneralTax : TaxType.ExpenseTax
    });
}

context.Categories.AddRange(categories);
await context.SaveChangesAsync();
Console.WriteLine($"Added {categories.Count} categories");


// ---------------------------
// 3️⃣ Seed Transactions (20–30k)
// ---------------------------
Console.WriteLine("Seeding transactions...");

var transactions = new List<Transaction>();
int total = 25000;
int batch = 2000;

for (int i = 1; i <= total; i++)
{
    var category = categories[rnd.Next(categories.Count)];
    var user = users[rnd.Next(users.Count)];

    decimal sum = Math.Round((decimal)(rnd.NextDouble() * 5000 + 10), 2);
    decimal taxMultiplier = 1 + category.TaxAmount / 100;

    transactions.Add(new Transaction
    {
        Name = $"Транзакція #{i}",
        CategoryId = category.Id,
        UserId = user.Id,
        Sum = sum,
        SumAfterTax = Math.Round(sum * taxMultiplier, 2),
        CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 365)),
        UpdatedAt = DateTime.UtcNow
    });

    if (i % batch == 0)
    {
        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
        Console.WriteLine($"Inserted {i}/{total}");
        transactions.Clear();
    }
}

if (transactions.Any())
{
    context.Transactions.AddRange(transactions);
    await context.SaveChangesAsync();
}

Console.WriteLine("==== Done! ====");
Console.WriteLine($"Users: {context.Users.Count()}");
Console.WriteLine($"Categories: {context.Categories.Count()}");
Console.WriteLine($"Transactions: {context.Transactions.Count()}");

Console.WriteLine($"Database: {conn}");