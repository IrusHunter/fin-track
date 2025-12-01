using MobileApp.ViewModels;
using FinTrack.Models;
using FinTrack.Services;

namespace MobileApp.Views;

public partial class EditCategoryPage : ContentPage
{
    public EditCategoryPage(Category category, ICategoryService s)
    {
        InitializeComponent();
        BindingContext = new EditCategoryViewModel(category, s);
    }
}
