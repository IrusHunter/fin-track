using MobileApp.ViewModels;

namespace MobileApp.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage(TransactionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
