using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FinTrack.Models;
using FinTrack.Repositories;
using FinTrack.Services;
using FinTrack.CustomConsole;

Env.TraversePath().Load();

var connStr = ApplicationDbContext.GetConnectionStringFromENV();

var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("fintrack"));

var provider = services.BuildServiceProvider();

using var db = provider.GetRequiredService<ApplicationDbContext>();
db.Database.EnsureCreated();

var categoryRepository = new CategoryRepository(db);
var categoryService = new CategoryService(categoryRepository);
var reportRepository = new ReportRepository(db);

var transactionRepository = new TransactionRepository(db);
var transactionService = new TransactionService(transactionRepository, categoryRepository);
var reportService = new ReportService(reportRepository);
var cc = new CustomConsole(categoryService, transactionService, reportService);

await cc.Run();

Console.WriteLine("Hello, World!");
