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
    }
}