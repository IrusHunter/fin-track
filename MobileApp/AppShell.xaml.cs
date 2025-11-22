namespace MobileApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("main", typeof(Views.MainPage));
		Routing.RegisterRoute("transactions", typeof(Views.TransactionsPage));
		Routing.RegisterRoute("categories", typeof(Views.CategoriesPage));
		Routing.RegisterRoute("reports", typeof(Views.ReportsPage));
	}
}
