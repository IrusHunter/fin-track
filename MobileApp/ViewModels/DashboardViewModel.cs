using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinTrack.Services;
using FinTrack.Models;
using System.Collections.ObjectModel;

namespace MobileApp.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;

    public DashboardViewModel(
        ITransactionService transactionService,
        ICategoryService categoryService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;

        RecentTransactions = new ObservableCollection<Transaction>();
        TopCategories = new ObservableCollection<Category>();

        LoadCommand.Execute(null);
    }

    // --- DATA ---
    [ObservableProperty]
    private decimal totalSpent;

    [ObservableProperty]
    private decimal spentToday;

    [ObservableProperty]
    private decimal spentThisMonth;

    [ObservableProperty]
    private ObservableCollection<Transaction> recentTransactions;

    [ObservableProperty]
    private ObservableCollection<Category> topCategories;

    [ObservableProperty]
    private bool isBusy;

    // --- LOAD DASHBOARD ---
    [RelayCommand]
    private async Task Load()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var tx = await _transactionService.FindAll();
            var cats = await _categoryService.FindAll();

            // Сумарні витрати
            TotalSpent = tx.Sum(t => t.SumAfterTax);

            // За сьогодні
            SpentToday = tx
                .Where(t => t.CreatedAt.Date == DateTime.UtcNow.Date)
                .Sum(t => t.SumAfterTax);

            // За місяць
            SpentThisMonth = tx
                .Where(t => t.CreatedAt.Month == DateTime.UtcNow.Month &&
                            t.CreatedAt.Year == DateTime.UtcNow.Year)
                .Sum(t => t.SumAfterTax);

            // Останні 5 транзакцій
            RecentTransactions.Clear();
            foreach (var t in tx.OrderByDescending(t => t.CreatedAt).Take(5))
                RecentTransactions.Add(t);

            // Популярні категорії (за кількістю транзакцій)
            TopCategories.Clear();
            var groups = tx
                .GroupBy(t => t.Category!)
                .OrderByDescending(g => g.Count())
                .Take(3);

            foreach (var g in groups)
                TopCategories.Add(g.Key);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
