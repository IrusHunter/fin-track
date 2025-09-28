using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FinTrack.Models;
using FinTrack.Repositories;
using FinTrack.Services;
using FinTrack.CustomConsole;

Env.TraversePath().Load();

var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST is not specified in .env file");
var user = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER is not specified in .env file");
var password = Environment.GetEnvironmentVariable("DB_USER_PASSWORD") ?? throw new Exception("DB_USER_PASSWORD is not specified in .env file");
var port = Environment.GetEnvironmentVariable("DB_PORT") ?? throw new Exception("DB_PORT is not specified in .env file");
var name = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new Exception("DB_NAME is not specified in .env file");

var connStr = $"Host = {host};Port={port};Database={name};Username={user};Password={password}";

var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connStr));

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

// Console.WriteLine("Hello, World!");
