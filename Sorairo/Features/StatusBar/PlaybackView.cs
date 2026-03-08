using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Sorairo.Common.Helpers;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.StatusBar;

public sealed class PlaybackView(PlaybackViewModel vm, AudioState audioState) : ViewBase
{
    protected override void Init()
    {
        Content = new Button
        {
            Width = 24,
            Height = 24,
            Content = new PathIcon().Bind(
                FluentBinding
                    .Bind(audioState, a => a.Status, PathIcon.DataProperty)
                    .Convert(status =>
                        status switch
                        {
                            AudioPlaybackStatus.Playing => Icons.PauseFilled,
                            _ => Icons.PlayFilled,
                        }
                    )
            ),
            Command = vm.TogglePlaybackCommand,
        };
    }
}
