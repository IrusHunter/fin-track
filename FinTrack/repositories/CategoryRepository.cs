
using System.Data.Common;
using System.Threading.Tasks;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repositories
{
    public interface ICategoryRepository
    {
        public Task<Category> Create(Category category);
        public Task<Category?> Find(int id);
        public Task<Category?> FindByName(string name);
        public Task<Category[]> FindAll();
        public Task Update(Category category);
        public Task HardDelete(int id);
        public Task SoftDelete(int id);
        public Task<Category> FillModel(Category category);
    }

    public class CategoryRepository : ICategoryRepository
    {
        readonly private ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Category> Create(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> Find(int id)
        {
            return await _db.Categories.SingleOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> FindByName(string name)
        {
            return await _db.Categories.SingleOrDefaultAsync(c => c.Name == name);
        }


        public async Task<Category[]> FindAll()
        {
            return await _db.Categories
                .Where(c => c.DeletedAt == null)
                .ToArrayAsync();
        }

        public async Task Update(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task HardDelete(int id)
        {
            var category = await Find(id) ?? throw new Exception($"Category {id} not found");

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }

        public async Task SoftDelete(int id)
        {
            var category = await Find(id) ?? throw new Exception($"Category {id} not found");
            category.DeletedAt = DateTime.UtcNow;

            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task<Category> FillModel(Category category)
        {
            await _db.Entry(category).Collection(c => c.Transactions).LoadAsync();

            return category;
        }

    }
}