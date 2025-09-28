using FinTrack.Models;
using FinTrack.Repositories;

namespace FinTrack.Services
{
    /// <summary>
    /// Interface defining service operations for <see cref="Category"/> entities.
    /// Provides methods for creating, finding, updating, and deleting categories.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Creates a new category with validation.
        /// </summary>
        /// <param name="category">The category to create.</param>
        /// <returns>The created category.</returns>
        public Task<Category> Create(Category category);

        /// <summary>
        /// Finds a category by its Id.
        /// </summary>
        /// <param name="id">The Id of the category.</param>
        /// <returns>The category if found, or null if not found.</returns>
        public Task<Category?> Find(int id);

        /// <summary>
        /// Retrieves all categories that are not soft-deleted.
        /// </summary>
        /// <returns>An array of active categories.</returns>
        public Task<Category[]> FindAll();

        /// <summary>
        /// Updates an existing category with validation.
        /// </summary>
        /// <param name="category">The category with updated values.</param>
        public Task Update(Category category);

        /// <summary>
        /// Deletes a category. Performs a hard delete if no transactions exist;
        /// otherwise, performs a soft delete.
        /// </summary>
        /// <param name="id">The Id of the category to delete.</param>
        public Task Delete(int id);
    }

    /// <summary>
    /// Service class implementing business logic for <see cref="Category"/> entities.
    /// Uses <see cref="ICategoryRepository"/> for data access.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        readonly private ICategoryRepository _categoryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        /// <param name="categoryRepository">The repository used for data access operations.</param>
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <inheritdoc/>
        public async Task<Category> Create(Category category)
        {
            await Validate(category);

            // category.Id = 0;
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            return await _categoryRepository.Create(category);
        }

        /// <inheritdoc/>
        public async Task<Category?> Find(int id)
        {
            return await _categoryRepository.Find(id);
        }

        /// <inheritdoc/>
        public async Task<Category[]> FindAll()
        {
            return await _categoryRepository.FindAll();
        }

        /// <inheritdoc/>
        public async Task Update(Category category)
        {
            await Validate(category);
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.Update(category);
        }

        /// <inheritdoc/>
        public async Task Delete(int id)
        {
            var category = await _categoryRepository.Find(id) ?? throw new Exception($"Category {id} not found");
            await _categoryRepository.FillModel(category);

            if (category.Transactions.LongCount() == 0) { await _categoryRepository.HardDelete(id); }
            else { await _categoryRepository.SoftDelete(id); }
        }

        /// <summary>
        /// Validates the category properties before creation or update.
        /// Checks for name length, tax amount, and uniqueness.
        /// </summary>
        /// <param name="category">The category to validate.</param>
        private async Task Validate(Category category)
        {
            if (category.Name.Length > Category.MaxNameLength) { throw new Exception($"Category name ({category.Name.Length}) is longer than {Category.MaxNameLength}"); }

            if (category.TaxAmount > Category.MaxTaxAmount) { throw new Exception($"Category tax amount ({category.TaxAmount}) bigger than {Category.MaxTaxAmount}%"); }
            if (category.TaxAmount < Category.MinTaxAmount) { throw new Exception($"Category tax amount ({category.TaxAmount}) lower than {Category.MinTaxAmount}%"); }

            var existedCategory = await _categoryRepository.FindByName(category.Name);
            if (existedCategory != null && existedCategory.Id != category.Id)
            {
                throw new Exception($"Category with name {category.Name} already exists ({existedCategory.Id})");
            }
        }

    }
}