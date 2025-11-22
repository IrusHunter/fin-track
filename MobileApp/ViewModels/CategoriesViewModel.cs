using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinTrack.Services;
using FinTrack.Models;
using System.Collections.ObjectModel;

namespace MobileApp.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly CategoryService _categoryService;

    public CategoriesViewModel(CategoryService categoryService)
    {
        _categoryService = categoryService;

        Categories = new ObservableCollection<Category>(_categoryService.GetAllCategories());
    }

    [ObservableProperty]
    private ObservableCollection<Category> categories;

    [ObservableProperty]
    private string newCategoryName;

    // Додати
    [RelayCommand]
    private void AddCategory()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
            return;

        var category = new Category { Name = NewCategoryName };
        _categoryService.AddCategory(category);

        Categories.Add(category);
        NewCategoryName = "";
    }

    // Видалити
    [RelayCommand]
    private void DeleteCategory(int categoryId)
    {
        var category = Categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
            return;

        _categoryService.DeleteCategory(categoryId);
        Categories.Remove(category);
    }
}
