using CommunityToolkit.Mvvm.ComponentModel;

namespace Sorairo.Common.Models;

public sealed partial class AudioState : ObservableObject
{
    [ObservableProperty]
    private AudioPlaybackStatus status;

    [ObservableProperty]
    private double volume = 1.0;

    [ObservableProperty]
    private TimeSpan totalTime;

    [ObservableProperty]
    private TimeSpan elapsedTime;
}
