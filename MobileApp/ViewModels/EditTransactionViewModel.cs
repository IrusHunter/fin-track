using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinTrack.Models;
using FinTrack.Services;
using System.Collections.ObjectModel;

public partial class EditTransactionViewModel : ObservableObject
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
    private readonly Transaction _original;

    [ObservableProperty] private string name;
    [ObservableProperty] private string sum;
    [ObservableProperty] private Category selectedCategory;
    [ObservableProperty] private ObservableCollection<Category> categories;

    public EditTransactionViewModel(Transaction tx,
                                    ITransactionService transactionService,
                                    ICategoryService categoryService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _original = tx;

        Name = tx.Name;
        Sum = tx.Sum.ToString();
        SelectedCategory = tx.Category!;
        Categories = new ObservableCollection<Category>();
        LoadCategories();
    }

    private async void LoadCategories()
    {
        Categories.Clear();
        var cats = await _categoryService.FindAll();
        foreach (var c in cats)
            Categories.Add(c);
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!decimal.TryParse(Sum, out var s))
        {
            await Application.Current.MainPage.DisplayAlert("Помилка", "Невірна сума", "OK");
            return;
        }

        _original.Name = Name;
        _original.Sum = s;
        _original.CategoryId = SelectedCategory.Id;

        await _transactionService.Update(_original);

        await Application.Current.MainPage.Navigation.PopAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Application.Current.MainPage.Navigation.PopAsync();
    }
}
