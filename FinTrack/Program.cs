using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FinTrack.Models;

Env.Load();

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

Console.WriteLine("Hello, World!");
