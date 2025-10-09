using Microsoft.EntityFrameworkCore;

namespace FinTrack.Models
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the FinTrack application.
    /// Manages access to <see cref="Category"/> and <see cref="Transaction"/> entities.
    /// </summary>
    public class ApplicationDbContext : DbContext
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
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new Exception("DB_HOST is not specified in .env file");
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? throw new Exception("DB_USER is not specified in .env file");
            var password = Environment.GetEnvironmentVariable("DB_USER_PASSWORD") ?? throw new Exception("DB_USER_PASSWORD is not specified in .env file");
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? throw new Exception("DB_PORT is not specified in .env file");
            var name = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new Exception("DB_NAME is not specified in .env file");

            return $"Host = {host};Port={port};Database={name};Username={user};Password={password}";
        }
    }
}
