using MobileApp.ViewModels;

namespace MobileApp.Views;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage(CategoriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
