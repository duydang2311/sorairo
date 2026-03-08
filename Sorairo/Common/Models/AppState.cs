using CommunityToolkit.Mvvm.ComponentModel;
using Sorairo.Common.UI;

namespace Sorairo.Common.Models;

public sealed partial class AppState : ObservableObject
{
    [ObservableProperty]
    private AppMainView mainView = AppMainView.None;

    [ObservableProperty]
    private double windowWidth;

    [ObservableProperty]
    private Viewport viewport;

    partial void OnWindowWidthChanged(double value)
    {
        Viewport = value switch
        {
            < 480 => Viewport.XSmall,
            < 768 => Viewport.Small,
            < 1024 => Viewport.Medium,
            < 1280 => Viewport.Large,
            _ => Viewport.XLarge,
        };
    }
}
