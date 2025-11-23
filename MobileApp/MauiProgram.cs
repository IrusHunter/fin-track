using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using FinTrack.Models;
using FinTrack.Services;
using FinTrack.Repositories;

using MobileApp.ViewModels;
using MobileApp.Views;

namespace MobileApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseMauiApp<App>()
			// .UseMauiCommunityToolkit() // Toolkit підключено
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "sqlite";

		switch (dbProvider)
		{
			case "sqlserver":
				throw new Exception("SQL Server is not supported on mobile platforms");

			case "postgres":
				throw new Exception("PostgreSQL is not supported on mobile platforms");

			case "memory":
				builder.Services.AddDbContext<ApplicationDbContext>(options =>
					options.UseInMemoryDatabase("fintrack_memory"));
				break;

			case "sqlite":
			default:
				builder.Services.AddDbContext<ApplicationDbContext>(options =>
					options.UseSqlite(ApplicationDbContext.GetConnectionStringFromENV()));
				break;
		}

		builder.Services.AddTransient<ITransactionRepository, TransactionRepository>();
		builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();

		builder.Services.AddTransient<ITransactionService, TransactionService>();
		builder.Services.AddTransient<ICategoryService, CategoryService>();

		builder.Services.AddTransient<TransactionsViewModel>();
		builder.Services.AddTransient<TransactionsPage>();

		builder.Services.AddTransient<CategoriesViewModel>();
		builder.Services.AddTransient<CategoriesPage>();

		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<DashboardPage>();

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
		}

		return app;
	}
}
