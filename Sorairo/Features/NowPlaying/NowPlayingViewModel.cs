using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.NowPlaying;

public sealed partial class NowPlayingViewModel(
    IAudioService audioService,
    AudioState audioState,
    IPlaylistService playlistService,
    PlaylistState playlistState
) : ViewModelBase, IDisposable
{
    protected override void Init()
    {
        playlistState.PropertyChanged += OnPlaylistStatePropertyChanged;
    }

    [ObservableProperty]
    private bool isSeeking;

    [RelayCommand]
    private void PauseOrResume()
    {
        switch (audioState.Status)
        {
            case AudioPlaybackStatus.Playing:
                audioService.Pause();
                break;
            case AudioPlaybackStatus.Paused:
                audioService.Resume();
                break;
            case AudioPlaybackStatus.None:
                playlistService.Play();
                break;
        }
    }

    [RelayCommand]
    private void Stop()
    {
        audioService.Stop();
    }

    [RelayCommand(CanExecute = nameof(CanSkip))]
    private void SkipPrevious()
    {
        playlistService.SkipPrevious();
        playlistService.Play();
    }

    [RelayCommand(CanExecute = nameof(CanSkip))]
    private void SkipNext()
    {
        playlistService.SkipNext();
        playlistService.Play();
    }

    [RelayCommand]
    private void Seek(double seconds)
    {
        audioService.Seek(TimeSpan.FromSeconds(seconds));
    }

    public void Dispose()
    {
        playlistState.PropertyChanged -= OnPlaylistStatePropertyChanged;
    }

    private bool CanSkip()
    {
        return playlistState.CurrentItem is not null;
    }

    private void OnPlaylistStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlaylistState.CurrentItem))
        {
            SkipPreviousCommand.NotifyCanExecuteChanged();
            SkipNextCommand.NotifyCanExecuteChanged();
        }
    }
}
