namespace MobileApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Спочатку показуємо LoaderPage
		MainPage = new Views.LoaderPage();

		// Через 1.5 секунди переходимо на Shell
		Task.Run(async () =>
		{
			await Task.Delay(1500);

			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				MainPage = new AppShell();
			});
		});
	}
}
