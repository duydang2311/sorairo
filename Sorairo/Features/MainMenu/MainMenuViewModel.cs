using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Messages;
using Sorairo.Common.Models;

namespace Sorairo.Features.MainMenu;

public sealed partial class MainMenuViewModel(IPlaylistService playlistService, AppState appState)
    : ObservableObject
{
    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var path = await WeakReferenceMessenger.Default.Send(new OpenSingleFileDialogMessage());
        if (path is null)
        {
            return;
        }
        var item = playlistService.AddItem(path);
        playlistService.SetCurrentItem(item);
        playlistService.Play();
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        var paths = await WeakReferenceMessenger.Default.Send(new OpenMultiFilesDialogMessage());
        if (paths is null)
        {
            return;
        }
        foreach (var path in paths)
        {
            playlistService.AddItem(path);
        }
    }

    [RelayCommand]
    private void ShowPlaylistView()
    {
        if (appState.MainView == AppMainView.Playlist)
        {
            appState.MainView = AppMainView.None;
        }
        else
        {
            appState.MainView = AppMainView.Playlist;
        }
    }

    [RelayCommand]
    private void NewPlaylist()
    {
        playlistService.Stop();
        playlistService.Clear();
    }

    [RelayCommand]
    private void ToggleRightPanel()
    {
        appState.MainView = appState.MainView switch
        {
            AppMainView.None => AppMainView.Playlist,
            AppMainView.Playlist => AppMainView.None,
            _ => throw new InvalidProgramException(),
        };
    }
}
