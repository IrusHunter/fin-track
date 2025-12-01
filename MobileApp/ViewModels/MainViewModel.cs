using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MobileApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int count;

    [ObservableProperty]
    private string counterText = "Clicked 0 times";

    [RelayCommand]
    private void Increment()
    {
        Count++;

        CounterText = Count == 1 ?
            "Clicked 1 time" :
            $"Clicked {Count} times";
    }
}
