using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinTrack.Services;
using FinTrack.Models;
using System.Collections.ObjectModel;

namespace MobileApp.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;

    public TransactionsViewModel(
        ITransactionService transactionService,
        ICategoryService categoryService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;

        Transactions = new ObservableCollection<Transaction>();
        Categories = new ObservableCollection<Category>();

        LoadCommand.Execute(null);
    }

    // --- Properties ------------------------------------------------------------

    [ObservableProperty]
    private ObservableCollection<Transaction> transactions;

    [ObservableProperty]
    private ObservableCollection<Category> categories;

    [ObservableProperty]
    private string? newName;

    [ObservableProperty]
    private string? newSum;

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private bool isBusy;


    // --- Load Data -------------------------------------------------------------

    [RelayCommand]
    private async Task Load()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Load Transactions
            Transactions.Clear();
            var tx = await _transactionService.FindAll();
            foreach (var t in tx)
                Transactions.Add(t);

            // Load Categories
            Categories.Clear();
            var cats = await _categoryService.FindAll();
            foreach (var c in cats)
                Categories.Add(c);
        }
        finally
        {
            IsBusy = false;
        }
    }


    // --- Add Transaction -------------------------------------------------------

    [RelayCommand]
    private async Task AddTransaction()
    {
        if (string.IsNullOrWhiteSpace(NewName)
            || string.IsNullOrWhiteSpace(NewSum)
            || SelectedCategory == null)
            return;

        if (!decimal.TryParse(NewSum, out var sum))
            return;

        try
        {
            IsBusy = true;

            // Calculate SumAfterTax
            var tax = SelectedCategory.TaxAmount; // Наприклад 15 (%)
            var sumAfterTax = sum + (sum * tax / 100m);

            var tx = new Transaction
            {
                Name = NewName,
                Sum = sum,
                SumAfterTax = sumAfterTax,
                CategoryId = SelectedCategory.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = "02540265-ebd3-4cfe-9472-6c94a12e5bab"
            };

            var created = await _transactionService.Create(tx);

            Transactions.Add(created);

            // Clear UI fields
            NewName = "";
            NewSum = "";
            SelectedCategory = null;
        }
        finally
        {
            IsBusy = false;
        }
    }


    // --- Delete Transaction ----------------------------------------------------

    [RelayCommand]
    private async Task DeleteTransaction(int id)
    {
        var item = Transactions.FirstOrDefault(x => x.Id == id);
        if (item == null) return;

        try
        {
            IsBusy = true;
            await _transactionService.Delete(id);
            Transactions.Remove(item);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditTransaction(Transaction tx)
    {
        if (tx == null) return;

        var page = new Views.EditTransactionPage(tx, _transactionService, _categoryService);

        page.Disappearing += async (_, __) =>
        {
            await Load(); // автоматичне оновлення
        };

        await Application.Current.MainPage.Navigation.PushAsync(page);
    }
}
