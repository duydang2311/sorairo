using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Sorairo.Common.Helpers;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.StatusBar;

public sealed partial class PlaybackViewModel(IAudioService audioService) : ViewModelBase
{
    protected override void Init() { }

    [RelayCommand]
    private void TogglePlayback()
    {
        if (audioService.IsPlaying)
        {
            audioService.Pause();
        }
        else if (audioService.IsPaused)
        {
            audioService.Resume();
        }
    }
}
