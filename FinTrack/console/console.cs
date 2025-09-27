
using FinTrack.Models;
using FinTrack.Services;

namespace FinTrack.CustomConsole
{
    public interface ICustomConsole
    {
        public Task Run();
    }

    public class CustomConsole : ICustomConsole
    {
        private readonly ICategoryService _categoryService;

        public CustomConsole(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task Run()
        {
            Console.WriteLine("Welcome to FinTrack! Choose the option:\n");
            Console.WriteLine("1 - create new category.");

            switch (Console.ReadLine())
            {
                case "1":
                    {
                        var category = new Category();
                        category.Name = Console.ReadLine();
                        category.TaxAmount = int.Parse(Console.ReadLine());

                        await _categoryService.Create(category);
                        break;
                    }
                case "2":
                    {
                        break;
                    }
                case "3":
                    {
                        break;
                    }
                case "4":
                    {
                        break;
                    }

                default: break;
            }
        }

    }
}