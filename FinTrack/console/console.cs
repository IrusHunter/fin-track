
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
            var exit = false;
            do
            {
                Console.Clear();
                Console.WriteLine("Welcome to FinTrack!\n");
                Console.WriteLine("=== FinTrack Menu ===");
                Console.WriteLine("1 - Category.");
                Console.WriteLine("2 - Transaction.");
                Console.WriteLine("0 - Exit ");
                Console.WriteLine("=====================");

                switch (SetInt(0, 2, "option"))
                {
                    case 1:
                        {
                            await CategoryMenu();
                            break;
                        }
                    case 2:
                        {
                            await TransactionMenu();
                            break;
                        }

                    default: exit = true; break;
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            } while (!exit);
            Console.WriteLine("Thank you for using FinTrack. Wishing you financial success. Goodbye!");
        }


        private async Task CategoryMenu()
        {
            var exit = false;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Category Menu ===");
                Console.WriteLine("1 - Create new category.");
                Console.WriteLine("2 - List all categories");
                Console.WriteLine("3 - Find category");
                Console.WriteLine("4 - Update category");
                Console.WriteLine("5 - Delete category");
                Console.WriteLine("0 - Exit ");
                Console.WriteLine("=====================");

                switch (SetInt(0, 5, "option"))
                {
                    case 1:
                        {
                            var category = new Category();
                            category.Name = SetString(1, Category.MaxNameLength, "name");
                            category.TaxAmount = SetDecimal(Category.MinTaxAmount, Category.MaxTaxAmount, "tax amount");

                            try
                            {
                                var resCategory = await _categoryService.Create(category);
                                Console.WriteLine($"Category ({resCategory}) created successfully.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        }
                    case 2:
                        {
                            var categories = await _categoryService.FindAll();
                            foreach (var category in categories)
                            {
                                Console.WriteLine(category);
                            }
                            break;
                        }

                    case 3:
                        {
                            int id = SetInt(1, int.MaxValue, "id");
                            var category = await _categoryService.Find(id);
                            if (category == null)
                            {
                                Console.WriteLine($"Category {id} not found.");
                            }
                            else
                            {
                                Console.WriteLine(category);
                            }
                            break;
                        }

                    case 4:
                        {
                            int id = SetInt(1, int.MaxValue, "id");
                            var category = await _categoryService.Find(id);
                            if (category == null)
                            {
                                Console.WriteLine($"Category {id} not found.");
                                break;
                            }
                            Console.WriteLine($"Current category data: {category}");
                            category.Name = SetString(1, Category.MaxNameLength, "new name");
                            category.TaxAmount = SetDecimal(Category.MinTaxAmount, Category.MaxTaxAmount, "new tax amount");

                            try
                            {
                                await _categoryService.Update(category);
                                Console.WriteLine($"Category ({category}) updated successfully.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        }

                    case 5:
                        {
                            int id = SetInt(1, int.MaxValue, "id");
                            try
                            {
                                await _categoryService.Delete(id);
                                Console.WriteLine($"Category {id} deleted successfully.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        }

                    default: exit = true; break;
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            while (!exit);
        }


        private async Task TransactionMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Category Menu ===");
            // Console.WriteLine("1 - Create new category.");
            // Console.WriteLine("2 - List all categories");
            // Console.WriteLine("3 - Find category");
            // Console.WriteLine("4 - Update category");
            // Console.WriteLine("5 - Delete category");
            // Console.WriteLine("0 - Exit ");
            Console.Write("Choose option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    {
                        // var category = new Category();
                        // category.Name = Console.ReadLine();
                        // category.TaxAmount = int.Parse(Console.ReadLine());

                        // await _categoryService.Create(category);
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

        private static int SetInt(int min, int max, string name)
        {
            int value = 0;
            bool inputIsCorrect = false;
            while (!inputIsCorrect)
            {
                Console.Write($"Input the {name}: ");
                try
                {
                    value = Convert.ToInt32(Console.ReadLine());
                    inputIsCorrect = true;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Format error. Please try again.");
                    inputIsCorrect = false;
                }
                if (inputIsCorrect)
                {
                    inputIsCorrect = false;
                    if (value > max)
                        Console.WriteLine($"{char.ToUpper(name[0]) + name.Substring(1)} is greater than the maximum allowed value (${max}). Please try again.");
                    else if (value < min)
                        Console.WriteLine($"{char.ToUpper(name[0]) + name.Substring(1)} is less than the minimum allowed value (${min}). Please try again.");
                    else inputIsCorrect = true;
                }
            }
            return value;
        }

        private static string SetString(int minLength, int maxLength, string name)
        {
            string value = string.Empty;
            bool inputIsCorrect = false;
            while (!inputIsCorrect)
            {
                Console.Write($"Input the {name}: ");
                value = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine("Input cannot be empty. Please try again.");
                    continue;
                }
                if (value.Length < minLength)
                {
                    Console.WriteLine($"{char.ToUpper(name[0]) + name.Substring(1)} is shorter than the minimum allowed length ({minLength}). Please try again.");
                }
                else if (value.Length > maxLength)
                {
                    Console.WriteLine($"{char.ToUpper(name[0]) + name.Substring(1)} is longer than the maximum allowed length ({maxLength}). Please try again.");
                }
                else
                {
                    inputIsCorrect = true;
                }
            }
            return value;
        }
        private static decimal SetDecimal(decimal min, decimal max, string name)
        {
            decimal value = 0;
            bool inputIsCorrect = false;
            while (!inputIsCorrect)
            {
                Console.Write($"Input the {name}: ");
                string? input = Console.ReadLine();
                if (!decimal.TryParse(input, out value))
                {
                    Console.WriteLine("Format error. Please try again.");
                    continue;
                }
                if (value > max)
                {
                    Console.WriteLine($"{char.ToUpper(name[0]) + name.Substring(1)} is greater than the maximum allowed value ({max}). Please try again.");
                }
                else if (value < min)
                {
                    Console.WriteLine($"{char.ToUpper(name[0]) + name.Substring(1)} is less than the minimum allowed value ({min}). Please try again.");
                }
                else
                {
                    inputIsCorrect = true;
                }
            }
            return value;
        }
    }
}