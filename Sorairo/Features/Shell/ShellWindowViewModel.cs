using System.ComponentModel;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.Shell;

public sealed partial class ShellWindowViewModel(AppState appState) : ViewModelBase, IDisposable
{
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
