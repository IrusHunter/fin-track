using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinTrack.Services;
using FinTrack.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace MobileApp.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;

    public CategoriesViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;

        Categories = new ObservableCollection<Category>();

        // Асинхронне завантаження категорій при створенні ViewModel
        LoadCategoriesCommand.Execute(null);
    }

    // Колекція категорій для UI
    [ObservableProperty]
    private ObservableCollection<Category> categories;

    // Нове ім'я категорії
    [ObservableProperty]
    private string newCategoryName = "";

    // Індикатор завантаження
    [ObservableProperty]
    private bool isBusy;

    // ==========================
    //   Завантаження категорій
    // ==========================

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

    // ==========================
    //       Додавання
    // ==========================

    [RelayCommand]
    private async Task AddCategory()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
            return;

        try
        {
            IsBusy = true;

            var newCategory = new Category
            {
                Name = NewCategoryName,
                TaxAmount = 0 // якщо треба, можна додати поле у UI
            };

            var created = await _categoryService.Create(newCategory);

            Categories.Add(created);
            NewCategoryName = "";
        }
        catch (Exception ex)
        {
            // TODO: можна показувати повідомлення через AlertService
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ==========================
    //        Видалення
    // ==========================

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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
