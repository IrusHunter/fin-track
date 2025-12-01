using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinTrack.Models;
using FinTrack.Services;
using System.Collections.ObjectModel;

namespace MobileApp.ViewModels;

public partial class EditCategoryViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;
    private readonly Category _original;

    public List<TaxType> TaxTypes { get; }

    public EditCategoryViewModel(Category category, ICategoryService categoryService)
    {
        _original = category;
        _categoryService = categoryService;

        TaxTypes = Enum.GetValues(typeof(TaxType)).Cast<TaxType>().ToList();

        // копія у редаговані поля
        Name = category.Name;
        TaxAmount = category.TaxAmount.ToString();
        SelectedTaxType = category.TaxType;
    }

    // -----------------------------
    //         Поля
    // -----------------------------

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string taxAmount;

    [ObservableProperty]
    private TaxType selectedTaxType;

    [ObservableProperty]
    private bool isBusy;

    // -----------------------------
    //       ЗБЕРЕЖЕННЯ
    // -----------------------------

    [RelayCommand]
    private async Task Save()
    {
        if (IsBusy) return;

        if (!decimal.TryParse(TaxAmount, out var tax))
        {
            await Application.Current.MainPage.DisplayAlert("Помилка", "Невірний формат податку!", "OK");
            return;
        }

        tax = Math.Clamp(tax, Category.MinTaxAmount, Category.MaxTaxAmount);

        try
        {
            IsBusy = true;

            _original.Name = Name;
            _original.TaxAmount = tax;
            _original.TaxType = SelectedTaxType;

            await _categoryService.Update(_original);

            await Application.Current.MainPage.DisplayAlert("Успіх", "Категорію оновлено!", "OK");

            // Повернення назад
            await Shell.Current.GoToAsync(nameof(Views.CategoriesPage));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // -----------------------------
    //         СКАСУВАННЯ
    // -----------------------------

    [RelayCommand]
    private async Task Cancel()
    {
        await Application.Current.MainPage.Navigation.PopAsync();
    }
}
