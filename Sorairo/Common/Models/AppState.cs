using CommunityToolkit.Mvvm.ComponentModel;

namespace Sorairo.Common.Models;

public sealed partial class AppState : ObservableObject
{
    [ObservableProperty]
    private AppMainView mainView = AppMainView.None;
}
