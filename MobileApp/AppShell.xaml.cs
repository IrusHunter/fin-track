namespace MobileApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("about", typeof(Views.AboutPage));
		Routing.RegisterRoute(nameof(Views.TransactionsPage), typeof(Views.TransactionsPage));
		Routing.RegisterRoute(nameof(Views.CategoriesPage), typeof(Views.CategoriesPage));
	}
}
