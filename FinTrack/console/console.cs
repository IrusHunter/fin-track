
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
        private readonly ITransactionService _transactionService;
        private readonly IReportService _reportService;

        public CustomConsole(
            ICategoryService categoryService,
            ITransactionService transactionService,
            IReportService reportService
            )
        {
            _categoryService = categoryService;
            _transactionService = transactionService;
            _reportService = reportService;
        }

        public async Task Run()
        {
            var exit = false;
            do
            {
                try
                {
                    Console.Clear();
                }
                catch (Exception) { Console.WriteLine("\n\n\n\n"); }
                Console.WriteLine("Welcome to FinTrack!\n");
                Console.WriteLine("=== FinTrack Menu ===");
                Console.WriteLine("1 - Category.");
                Console.WriteLine("2 - Transaction.");
                Console.WriteLine("3 - Reports.");
                Console.WriteLine("0 - Exit ");
                Console.WriteLine("=====================");

                switch (SetInt(0, 3, "option"))
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

                    case 3:
                        {
                            Console.WriteLine("Enter start date (yyyy-mm-dd):");
                            DateTime start, end;
                            while (!DateTime.TryParse(Console.ReadLine(), out start))
                                Console.WriteLine("Invalid date. Try again:");
                            Console.WriteLine("Enter end date (yyyy-mm-dd):");
                            while (!DateTime.TryParse(Console.ReadLine(), out end))
                                Console.WriteLine("Invalid date. Try again:");
                            try
                            {
                                var report = await _reportService.GetCategoryReport(start, end);
                                Console.WriteLine($"Category report for period {start:yyyy-MM-dd} - {end:yyyy-MM-dd}:");
                                foreach (var item in report)
                                {
                                    Console.WriteLine($"{item.Key}: {item.Value}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            Console.WriteLine("Press any key to continue...");
                            try
                            {
                                Console.ReadKey();
                            }
                            catch (Exception) { }
                            break;
                        }

                    default: exit = true; break;
                }

            } while (!exit);
            Console.WriteLine("Thank you for using FinTrack. Wishing you financial success. Goodbye!");
        }


        private async Task CategoryMenu()
        {
            var exit = false;
            do
            {
                try
                {
                    Console.Clear();
                }
                catch (Exception) { }
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
                try
                {
                    Console.ReadKey();
                }
                catch (Exception) { }
            }
            while (!exit);
        }

        private async Task TransactionMenu()
        {
            var exit = false;
            do
            {
                try
                {
                    Console.Clear();
                }
                catch (Exception) { }
                Console.WriteLine("=== Transaction Menu ===");
                Console.WriteLine("1 - Create new transaction");
                Console.WriteLine("2 - List all transactions");
                Console.WriteLine("3 - Find transaction");
                Console.WriteLine("4 - Update transaction");
                Console.WriteLine("5 - Delete transaction");
                Console.WriteLine("6 - Show company balance");
                Console.WriteLine("7 - Show month profit");
                Console.WriteLine("8 - Show period profit");
                Console.WriteLine("0 - Exit");
                Console.WriteLine("========================");

                switch (SetInt(0, 8, "option"))
                {
                    case 1:
                        {
                            var transaction = new Transaction();
                            transaction.Name = SetString(1, Transaction.MaxNameLength, "name");
                            Console.WriteLine("Note: Use negative sum for expenses and positive sum for income.");
                            transaction.Sum = SetDecimal(decimal.MinValue, decimal.MaxValue, "sum");
                            transaction.CategoryId = SetInt(1, int.MaxValue, "category id");
                            try
                            {
                                var resTransaction = await _transactionService.Create(transaction);
                                Console.WriteLine($"Transaction ({resTransaction}) created successfully.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        }
                    case 2:
                        {
                            var transactions = await _transactionService.FindAll();
                            foreach (var tr in transactions)
                            {
                                Console.WriteLine(tr);
                            }
                            break;
                        }
                    case 3:
                        {
                            int id = SetInt(1, int.MaxValue, "id");
                            var transaction = await _transactionService.Find(id);
                            if (transaction == null)
                            {
                                Console.WriteLine($"Transaction {id} not found.");
                            }
                            else
                            {
                                Console.WriteLine(transaction);
                            }
                            break;
                        }
                    case 4:
                        {
                            int id = SetInt(1, int.MaxValue, "id");
                            var transaction = await _transactionService.Find(id);
                            if (transaction == null)
                            {
                                Console.WriteLine($"Transaction {id} not found.");
                                break;
                            }
                            Console.WriteLine($"Current transaction data: {transaction}");
                            transaction.Name = SetString(1, Transaction.MaxNameLength, "new name");
                            transaction.Sum = SetDecimal(decimal.MinValue, decimal.MaxValue, "new sum");
                            transaction.CategoryId = SetInt(1, int.MaxValue, "new category id");
                            try
                            {
                                await _transactionService.Update(transaction);
                                Console.WriteLine($"Transaction ({transaction}) updated successfully.");
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
                                await _transactionService.Delete(id);
                                Console.WriteLine($"Transaction {id} deleted successfully.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        }
                    case 6:
                        {
                            try
                            {
                                var balance = await _transactionService.GetCompanyBalance();
                                Console.WriteLine($"Company balance: {balance}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        }
                    case 7:
                        {
                            int month = SetInt(1, 12, "month");
                            int year = SetInt(1, 9999, "year");
                            try
                            {
                                var profit = await _transactionService.GetMonthProfit(month, year);
                                Console.WriteLine($"Profit for {month}/{year}: {profit}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        }
                    case 8:
                        {
                            Console.WriteLine("Enter start date (yyyy-mm-dd):");
                            DateTime start, end;
                            while (!DateTime.TryParse(Console.ReadLine(), out start))
                                Console.WriteLine("Invalid date. Try again:");
                            Console.WriteLine("Enter end date (yyyy-mm-dd):");
                            while (!DateTime.TryParse(Console.ReadLine(), out end))
                                Console.WriteLine("Invalid date. Try again:");
                            try
                            {
                                var profit = await _transactionService.GetPeriodProfit(start, end);
                                Console.WriteLine($"Profit for period {start:yyyy-MM-dd} - {end:yyyy-MM-dd}: {profit}");
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
                try
                {
                    Console.ReadKey();
                }
                catch (Exception) { }
            }
            while (!exit);
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