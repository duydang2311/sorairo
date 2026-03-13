using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.Shell;

public sealed partial class ShellWindowViewModel(AppState appState) : ViewModelBase, IDisposable
{
    private static readonly double MACOS_TRAFFIC_LIGHTS_WIDTH = 72;
    private static readonly double WINDOWS_DECORATION_WIDTH = 138;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowPadding))]
    private WindowState windowState;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowPadding))]
    private Thickness offscreenMargin;

    public Thickness WindowPadding =>
        (OperatingSystem.IsMacOS(), OperatingSystem.IsWindows(), WindowState) switch
        {
            (true, _, WindowState.Maximized or WindowState.FullScreen) => new Thickness(
                16,
                0,
                OffscreenMargin.Right + 16,
                0
            ),
            (true, _, _) => new Thickness(
                16 + OffscreenMargin.Left + MACOS_TRAFFIC_LIGHTS_WIDTH,
                0,
                OffscreenMargin.Right + 16,
                0
            ),
            (_, true, _) => new Thickness(
                16 + OffscreenMargin.Left,
                0,
                OffscreenMargin.Right + WINDOWS_DECORATION_WIDTH + 16,
                0
            ),
            _ => new Thickness(),
        };

    protected override void Init()
    {
        appState.PropertyChanged += OnAppStatePropertyChanged;
    }

    public PlaylistVisibility PlaylistVisibility =>
        (appState.MainView == AppMainView.Playlist, appState.Viewport >= Viewport.Large) switch
        {
            (true, true) => PlaylistVisibility.VisibleAsPanel,
            (true, false) => PlaylistVisibility.VisibleAsOverlay,
            _ => PlaylistVisibility.Invisible,
        };

    public bool IsPlaylistVisbleAsOverlay =>
        appState.MainView == AppMainView.Playlist && appState.Viewport < Viewport.Large;

    private void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName == nameof(AppState.MainView)
            || e.PropertyName == nameof(AppState.Viewport)
        )
        {
            OnPropertyChanged(nameof(PlaylistVisibility));
        }
    }

    public void Dispose()
    {
        appState.PropertyChanged -= OnAppStatePropertyChanged;
    }
}
