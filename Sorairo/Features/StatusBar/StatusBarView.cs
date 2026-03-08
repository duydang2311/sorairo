using System.Linq.Expressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Microsoft.Extensions.DependencyInjection;
using Sorairo.Common.Helpers;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.StatusBar;

public sealed class StatusBarView(AudioState audioState, IServiceProvider serviceProvider)
    : ViewBase
{
    protected override void Init()
    {
        Content = new Border
        {
            Name = "Container",
            Padding = new Thickness(8, 4),
            Child = new StackPanel
            {
                Children =
                {
                    new ContentControl().Bind(
                        FluentBinding
                            .Bind(audioState, a => a.Status, ContentProperty)
                            .Mode(BindingMode.OneWay)
                            .Convert(status =>
                            {
                                return status switch
                                {
                                    AudioPlaybackStatus.Paused or AudioPlaybackStatus.Playing =>
                                        serviceProvider.GetRequiredService<PlaybackView>(),
                                    _ => null,
                                };
                            })
                    ),
                },
            },
        };
    }
}
