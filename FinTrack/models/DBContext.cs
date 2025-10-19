using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Models
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the FinTrack application.
    /// Manages access to <see cref="Category"/> and <see cref="Transaction"/> entities.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Gets or sets the Categories table in the database.
        /// </summary>
        public DbSet<Category> Categories { get; set; }
        /// <summary>
        /// Gets or sets the Transactions table in the database.
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class
        /// with the specified <see cref="DbContextOptions"/>.
        /// </summary>
        /// <param name="options">Options to configure the database context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public static string GetConnectionStringFromENV()
        {
            var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? throw new Exception("DB_PROVIDER is not specified in .env file"); ;

            switch (dbProvider.ToLower())
            {
                case "sqlserver":
                    {
                        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST is not specified in .env file");
                        // var port = Environment.GetEnvironmentVariable("DB_PORT") ?? throw new Exception("DB_PORT is not specified in .env file");
                        var user = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER is not specified in .env file");
                        var password = Environment.GetEnvironmentVariable("DB_USER_PASSWORD") ?? throw new Exception("DB_USER_PASSWORD is not specified in .env file");
                        var name = Environment.GetEnvironmentVariable("MAIN_DB_NAME") ?? throw new Exception("MAIN_DB_NAME is not specified in .env file");
                        return $"Server={host};Database={name};User Id={user};Password={password};TrustServerCertificate=True;";
                    }
                case "postgres":
                    {
                        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST is not specified in .env file");
                        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? throw new Exception("DB_PORT is not specified in .env file");
                        var user = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER is not specified in .env file");
                        var password = Environment.GetEnvironmentVariable("DB_USER_PASSWORD") ?? throw new Exception("DB_USER_PASSWORD is not specified in .env file");
                        var name = Environment.GetEnvironmentVariable("MAIN_DB_NAME") ?? throw new Exception("MAIN_DB_NAME is not specified in .env file");
                        return $"Host={host};Port={port};Database={name};Username={user};Password={password}";
                    }
                case "sqlite":
                    {
                        var path = Environment.GetEnvironmentVariable("MAIN_DB_NAME") + ".db" ?? throw new Exception("MAIN_DB_NAME is not specified in .env file");
                        return $"Data Source={path}";
                    }
                case "memory":
                default:
                    return Environment.GetEnvironmentVariable("MAIN_DB_NAME") ?? throw new Exception("MAIN_DB_NAME is not specified in .env file"); ;
            }
        }
    }
}
