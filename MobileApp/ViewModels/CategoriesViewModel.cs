using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinTrack.Services;
using FinTrack.Models;
using System.Collections.ObjectModel;

namespace MobileApp.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;

    public CategoriesViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;

        Categories = new ObservableCollection<Category>();

        TaxTypes = Enum.GetValues(typeof(TaxType)).Cast<TaxType>().ToList();

        LoadCategoriesCommand.Execute(null);
    }

    // Колекція категорій
    [ObservableProperty]
    private ObservableCollection<Category> categories;

    // Нові поля
    [ObservableProperty]
    private string newCategoryName = "";

    [ObservableProperty]
    private string newCategoryTaxAmount = "0";

    [ObservableProperty]
    private TaxType newCategoryTaxType = TaxType.GeneralTax;

    public List<TaxType> TaxTypes { get; }

    // Індикатор виконання
    [ObservableProperty]
    private bool isBusy;

    // -----------------------------
    //     Завантаження
    // -----------------------------

    [RelayCommand]
    private async Task LoadCategories()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            Categories.Clear();

            var items = await _categoryService.FindAll();
            foreach (var item in items)
                Categories.Add(item);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // -----------------------------
    //         Додавання
    // -----------------------------

    [RelayCommand]
    private async Task AddCategory()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
            return;

        if (!decimal.TryParse(NewCategoryTaxAmount, out var tax))
            return;

        try
        {
            IsBusy = true;

            var category = new Category
            {
                Name = NewCategoryName,
                TaxAmount = Math.Clamp(tax, Category.MinTaxAmount, Category.MaxTaxAmount),
                TaxType = NewCategoryTaxType
            };

            var created = await _categoryService.Create(category);

            Categories.Add(created);

            NewCategoryName = "";
            NewCategoryTaxAmount = "0";
            NewCategoryTaxType = TaxType.GeneralTax;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // -----------------------------
    //          Видалення
    // -----------------------------

    [RelayCommand]
    private async Task DeleteCategory(int categoryId)
    {
        var category = Categories.FirstOrDefault(x => x.Id == categoryId);
        if (category == null) return;

        try
        {
            IsBusy = true;

            await _categoryService.Delete(categoryId);

            Categories.Remove(category);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditCategory(Category category)
    {
        await Application.Current.MainPage.Navigation.PushAsync(new Views.EditCategoryPage(category, _categoryService));
    }
}
