using FinTrack.Models;
using FinTrack.Repositories;

namespace FinTrack.Services
{
    public interface ICategoryService
    {
        public Task<Category> Create(Category category);
        public Task<Category?> Find(int id);
        public Task<Category[]> FindAll();
        public Task Update(Category category);
        public Task Delete(int id);
    }

    public class CategoryService : ICategoryService
    {
        readonly private ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> Create(Category category)
        {
            await Validate(category);

            // category.Id = 0;
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            return await _categoryRepository.Create(category);
        }

        public async Task<Category?> Find(int id)
        {
            return await _categoryRepository.Find(id);
        }

        public async Task<Category[]> FindAll()
        {
            return await _categoryRepository.FindAll();
        }
        public async Task Update(Category category)
        {
            await Validate(category);
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.Update(category);
        }
        public async Task Delete(int id)
        {
            var category = await _categoryRepository.Find(id) ?? throw new Exception($"Category {id} not found");
            await _categoryRepository.FillModel(category);

            if (category.Transactions.LongCount() == 0) { await _categoryRepository.HardDelete(id); }
            else { await _categoryRepository.SoftDelete(id); }
        }

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