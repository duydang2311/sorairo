using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.Shell;

public sealed partial class ShellWindowViewModel(AppState appState) : ActivatableViewModel
{
    private static readonly double MACOS_TRAFFIC_LIGHTS_WIDTH = 72;
    private static readonly double WINDOWS_DECORATION_WIDTH = 0;

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
                OffscreenMargin.Right,
                0
            ),
            (true, _, _) => new Thickness(
                16 + OffscreenMargin.Left + MACOS_TRAFFIC_LIGHTS_WIDTH,
                0,
                OffscreenMargin.Right,
                0
            ),
            (_, true, _) => new Thickness(
                16 + OffscreenMargin.Left,
                0,
                OffscreenMargin.Right + WINDOWS_DECORATION_WIDTH,
                0
            ),
            _ => new Thickness(),
        };

    protected override void Init() { }

    protected override void OnActivated(ref DisposableBag disposables)
    {
        var vm = this;
        appState.PropertyChanged += OnAppStatePropertyChanged;
        disposables.Add(
            Disposable.Create(
                (vm, appState),
                static (tuple) =>
                {
                    tuple.appState.PropertyChanged -= tuple.vm.OnAppStatePropertyChanged;
                }
            )
        );
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
}
