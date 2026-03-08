using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.NowPlaying;

public sealed partial class NowPlayingViewModel(
    IAudioService audioService,
    AudioState audioState,
    IPlaylistService playlistService
) : ViewModelBase
{
    protected override void Init() { }

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

    [RelayCommand]
    private void SkipPrevious()
    {
        playlistService.SkipPrevious();
        playlistService.Play();
    }

    [RelayCommand]
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
}
