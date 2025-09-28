
using System.Data.Common;
using System.Threading.Tasks;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repositories
{
    /// <summary>
    /// Interface defining the repository operations for <see cref="Category"/> entities.
    /// Provides methods for creating, finding, updating, and deleting categories.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Creates a new category in the database.
        /// </summary>
        /// <param name="category">The category to create.</param>
        /// <returns>The created category with its generated Id.</returns>
        public Task<Category> Create(Category category);

        /// <summary>
        /// Finds a category by its Id.
        /// </summary>
        /// <param name="id">The Id of the category.</param>
        /// <returns>The category if found, or null if not found.</returns>
        public Task<Category?> Find(int id);

        /// <summary>
        /// Finds a category by its Name.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        /// <returns>The category if found, or null if not found.</returns>
        public Task<Category?> FindByName(string name);

        /// <summary>
        /// Retrieves all categories that are not soft-deleted.
        /// </summary>
        /// <returns>An array of active categories.</returns>
        public Task<Category[]> FindAll();

        /// <summary>
        /// Updates an existing category in the database.
        /// </summary>
        /// <param name="category">The category with updated values.</param>
        public Task Update(Category category);

        /// <summary>
        /// Permanently deletes a category from the database.
        /// </summary>
        /// <param name="id">The Id of the category to delete.</param>
        public Task HardDelete(int id);

        /// <summary>
        /// Performs a soft delete of a category by setting its <see cref="Category.DeletedAt"/> timestamp.
        /// </summary>
        /// <param name="id">The Id of the category to soft delete.</param>
        public Task SoftDelete(int id);

        /// <summary>
        /// Loads related data into the category model, such as its transactions.
        /// </summary>
        /// <param name="category">The category to fill with related data.</param>
        /// <returns>The category with its related data loaded.</returns>
        public Task<Category> FillModel(Category category);
    }

    /// <summary>
    /// Repository class for managing <see cref="Category"/> entities in the database.
    /// Implements <see cref="ICategoryRepository"/> using Entity Framework Core.
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        readonly private ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
        /// </summary>
        /// <param name="db">The database context to use for operations.</param>
        public CategoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<Category> Create(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return category;
        }

        /// <inheritdoc/>
        public async Task<Category?> Find(int id)
        {
            return await _db.Categories.SingleOrDefaultAsync(c => c.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Category?> FindByName(string name)
        {
            return await _db.Categories.SingleOrDefaultAsync(c => c.Name == name);
        }

        /// <inheritdoc/>
        public async Task<Category[]> FindAll()
        {
            return await _db.Categories
                .Where(c => c.DeletedAt == null)
                .ToArrayAsync();
        }

        /// <inheritdoc/>
        public async Task Update(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task HardDelete(int id)
        {
            var category = await Find(id) ?? throw new Exception($"Category {id} not found");

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task SoftDelete(int id)
        {
            var category = await Find(id) ?? throw new Exception($"Category {id} not found");
            category.DeletedAt = DateTime.UtcNow;

            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<Category> FillModel(Category category)
        {
            await _db.Entry(category).Collection(c => c.Transactions).LoadAsync();

            return category;
        }

    }
}