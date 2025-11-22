using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using FinTrack.Models;
using FinTrack.Services;
using FinTrack.Repositories;

using MobileApp.ViewModels;

namespace MobileApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit() // Toolkit підключено
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER")
						 ?? "sqlite"; // ← ставимо дефолт

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

		// Репозиторії
		builder.Services.AddTransient<TransactionRepository>();
		builder.Services.AddTransient<CategoryRepository>();
		builder.Services.AddTransient<ReportRepository>();

		// Сервіси
		builder.Services.AddTransient<TransactionService>();
		builder.Services.AddTransient<CategoryService>();
		builder.Services.AddTransient<ReportService>();

		// ====================================
		//   📌 MVVM: ViewModels + Views
		// ====================================
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<MainPage>();

		return builder.Build();
	}
}
