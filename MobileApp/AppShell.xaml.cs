namespace MobileApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("dashboard", typeof(Views.DashboardPage));
		Routing.RegisterRoute("transactions", typeof(Views.TransactionsPage));
		Routing.RegisterRoute("categories", typeof(Views.CategoriesPage));
	}
}
