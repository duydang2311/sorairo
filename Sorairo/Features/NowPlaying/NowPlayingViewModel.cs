using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.NowPlaying;

public sealed partial class NowPlayingViewModel(
    IAudioService audioService,
    AudioState audioState,
    IPlaylistService playlistService,
    PlaylistState playlistState
) : ActivatableViewModel
{
    protected override void Init() { }

    protected override void OnActivated(ref DisposableBag disposables)
    {
        playlistState
            .ObservePropertyChanged(a => a.CurrentItem)
            .Subscribe(item =>
            {
                SkipPreviousCommand.NotifyCanExecuteChanged();
                SkipNextCommand.NotifyCanExecuteChanged();
            })
            .AddTo(ref disposables);
        audioState
            .ObservePropertyChanged(a => a.Volume)
            .Select(volume =>
                volume switch
                {
                    <= 0 => VolumeStatus.Zero,
                    < 0.5 => VolumeStatus.Low,
                    _ => VolumeStatus.High,
                }
            )
            .Subscribe(status => VolumeStatus = status)
            .AddTo(ref disposables);
    }

    [ObservableProperty]
    private bool isSeeking;

    [ObservableProperty]
    private VolumeStatus volumeStatus;

    [ObservableProperty]
    private Stretch frontCoverStretch = Stretch.UniformToFill;

    [RelayCommand]
    private void TogglePlayback()
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

    [RelayCommand]
    private void ToggleShuffleMode()
    {
        playlistService.ToggleShuffleMode();
    }

    [RelayCommand]
    private void ToggleRepeatMode()
    {
        playlistService.ToggleRepeatMode();
    }

    [RelayCommand]
    private void ToggleFrontCoverStretch()
    {
        FrontCoverStretch = FrontCoverStretch switch
        {
            Stretch.UniformToFill => Stretch.Uniform,
            _ => Stretch.UniformToFill,
        };
    }

    private bool CanSkip()
    {
        return playlistState.CurrentItem is not null;
    }
}
