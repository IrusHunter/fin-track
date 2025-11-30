using MobileApp.ViewModels;
using FinTrack.Models;
using FinTrack.Services;

namespace MobileApp.Views;

public partial class EditTransactionPage : ContentPage
{
       public EditTransactionPage(Transaction tx,
                                  ITransactionService txService,
                                  ICategoryService catService)
       {
              InitializeComponent();
              BindingContext = new EditTransactionViewModel(tx, txService, catService);
       }
}

