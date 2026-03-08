using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Sorairo.Common.Helpers;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.NowPlaying;

public sealed class NowPlayingView(
    AudioState audioState,
    PlaylistState playlistState,
    NowPlayingViewModel vm,
    AppState appState
) : LifecycleViewBase
{
    protected override void Init()
    {
        Content = new DockPanel
        {
            Children =
            {
                new StackPanel
                {
                    Children =
                    {
                        ElapsedSlider(),
                        new Border { Padding = new Thickness(16, 0), Child = ElapsedTexts() },
                        new Border
                        {
                            Padding = new Thickness(16, 8),
                            Child = new Grid
                            {
                                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                                Children =
                                {
                                    PlaybackControls()
                                        .GridColumn(0)
                                        .Bind(
                                            FluentBinding
                                                .OneWay(
                                                    appState,
                                                    a => a.Viewport,
                                                    Grid.ColumnSpanProperty
                                                )
                                                .Convert(viewport =>
                                                    viewport switch
                                                    {
                                                        < Viewport.Small => 1,
                                                        _ => 2,
                                                    }
                                                )
                                        )
                                        .Bind(
                                            FluentBinding
                                                .OneWay(
                                                    appState,
                                                    a => a.Viewport,
                                                    Grid.HorizontalAlignmentProperty
                                                )
                                                .Convert(viewport =>
                                                    viewport switch
                                                    {
                                                        < Viewport.Small =>
                                                            HorizontalAlignment.Left,
                                                        _ => HorizontalAlignment.Center,
                                                    }
                                                )
                                        ),
                                    new Border
                                    {
                                        Child = VolumeSlider(),
                                        Width = 96,
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                    }.GridColumn(1),
                                },
                            },
                        },
                    },
                }
                    .Dock(Dock.Bottom)
                    .BindResource(BorderBrushProperty, "SurfaceBorderBrush")
                    .BindResource(BackgroundProperty, "SurfaceSubtleBrush"),
                new ContentControl().Bind(
                    FluentBinding
                        .Bind(playlistState, a => a.CurrentItem, ContentProperty)
                        .Mode(BindingMode.OneWay)
                        .Convert(a =>
                            a switch
                            {
                                PlaylistItem item => PlayingView(item),
                                _ => null,
                            }
                        )
                ),
            },
        };
    }

    private Border PlayingView(PlaylistItem item)
    {
        IImage? image = default;
        var frontCover = item.GetFrontCover();
        if (frontCover is not null)
        {
            using var ms = new MemoryStream(frontCover);
            image = new Bitmap(ms);
        }
        return new Border
        {
            Padding = new Thickness(16),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto,Auto,Auto"),
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new Border
                    {
                        IsVisible = image is not null,
                        CornerRadius = new CornerRadius(2),
                        ClipToBounds = true,
                        Child = new Image { Source = image, Stretch = Stretch.Uniform },
                    }.GridRow(0),
                    new TextBlock
                    {
                        Text = "Now Playing",
                        Margin = new Thickness(0, 16, 0, 0),
                        FontSize = 10,
                    }
                        .GridRow(1)
                        .BindResource(ForegroundProperty, "FgMutedBrush"),
                    new TextBlock
                    {
                        Text = item.Title,
                        Margin = new Thickness(0, 4, 0, 0),
                        IsVisible = !string.IsNullOrEmpty(item.Title),
                        FontSize = 18,
                        FontWeight = FontWeight.Medium,
                        TextWrapping = TextWrapping.WrapWithOverflow,
                    }
                        .GridRow(2)
                        .BindResource(ForegroundProperty, "FgEmphBrush"),
                    new TextBlock
                    {
                        Text = item.Artist,
                        Margin = new Thickness(0, 4, 0, 0),
                        FontWeight = FontWeight.SemiBold,
                    }
                        .GridRow(3)
                        .BindResource(ForegroundProperty, "PrimaryFgBrush"),
                },
            },
        };
    }

    private Grid VolumeSlider()
    {
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 8,
            Styles =
            {
                new Style(a => a.OfType<Grid>().Child())
                {
                    Setters = { new Setter(VerticalAlignmentProperty, VerticalAlignment.Center) },
                },
            },
            Children =
            {
                new PathIcon
                {
                    Width = 12,
                    Height = 12,
                    Data = Icons.VolumeLowFilled,
                }.GridColumn(0),
                new Slider { Minimum = 0, Maximum = 1 }
                    .GridColumn(1)
                    .Bind(
                        FluentBinding
                            .Bind(audioState, a => a.Volume, Slider.ValueProperty)
                            .Mode(BindingMode.TwoWay)
                    )
                    .Class("thumb"),
            },
        };
    }

    private StackPanel PlaybackControls()
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 12,
            Children =
            {
                new Button
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(12),
                    Content = new PathIcon
                    {
                        Width = 12,
                        Height = 12,
                        Data = Icons.SkipPreviousFilled,
                    },
                    Command = vm.SkipPreviousCommand,
                },
                new Button
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(12),
                    Content = new PathIcon
                    {
                        Width = 12,
                        Height = 12,
                        Data = Icons.StopFilled,
                    },
                    Command = vm.StopCommand,
                },
                new Button
                {
                    Padding = new Thickness(12),
                    VerticalAlignment = VerticalAlignment.Center,
                    Command = vm.PauseOrResumeCommand,
                }
                    .Class("primary", "filled")
                    .Bind(
                        FluentBinding
                            .Bind(audioState, a => a.Status, ContentProperty)
                            .Mode(BindingMode.OneWay)
                            .Convert(status =>
                                status switch
                                {
                                    AudioPlaybackStatus.Playing => new PathIcon
                                    {
                                        Width = 12,
                                        Height = 12,
                                        Data = Icons.PauseFilled,
                                    },
                                    _ => new PathIcon
                                    {
                                        Width = 12,
                                        Height = 12,
                                        Data = Icons.PlayFilled,
                                    },
                                }
                            )
                    ),
                new Button
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(12),
                    Content = new PathIcon
                    {
                        Width = 12,
                        Height = 12,
                        Data = Icons.SkipNextFilled,
                    },
                    Command = vm.SkipNextCommand,
                },
                new Control { Width = 38, IsHitTestVisible = false }, // ghost btn for now
            },
        };
    }

    private Slider ElapsedSlider()
    {
        var elapsedSlider = new Slider { Margin = new Thickness(0, -8, 0, 0), Minimum = 0 }
            .Bind(
                FluentBinding
                    .Bind(audioState, a => a.TotalTime, Slider.MaximumProperty)
                    .Mode(BindingMode.OneWay)
                    .Convert(a => Math.Max(1, a.TotalSeconds))
            )
            .Bind(
                FluentBinding
                    .Bind(audioState, a => a.ElapsedTime, Slider.ValueProperty)
                    .Mode(BindingMode.OneWay)
                    .Convert(a => a.TotalSeconds)
            )
            .Bind(
                FluentBinding
                    .Bind(vm, a => a.IsSeeking, Slider.TransitionsProperty)
                    .Mode(BindingMode.OneWay)
                    .Convert(isSeeking =>
                    {
                        return isSeeking
                            ? null
                            : new Transitions
                            {
                                new DoubleTransition
                                {
                                    Property = Slider.ValueProperty,
                                    Duration = TimeSpan.FromMilliseconds(200),
                                },
                            };
                    })
            );
        elapsedSlider.AddHandler(
            Slider.PointerPressedEvent,
            (_, _) =>
            {
                vm.IsSeeking = true;
            },
            RoutingStrategies.Tunnel
        );
        elapsedSlider.AddHandler(
            Slider.PointerReleasedEvent,
            (_, _) =>
            {
                vm.IsSeeking = false;
                vm.SeekCommand.Execute(elapsedSlider.Value);
            },
            RoutingStrategies.Tunnel
        );
        return elapsedSlider;
    }

    private Grid ElapsedTexts()
    {
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Children =
            {
                new TextBlock { FontSize = 10, FontWeight = FontWeight.Bold }
                    .GridColumn(0)
                    .BindResource(ForegroundProperty, "FgMutedBrush")
                    .Bind(
                        FluentBinding
                            .Bind(audioState, a => a.ElapsedTime, TextBlock.TextProperty)
                            .Mode(BindingMode.OneWay)
                            .Convert(FormatHelper.FormatPlaybackTime)
                    ),
                new TextBlock { FontSize = 10, FontWeight = FontWeight.Bold }
                    .GridColumn(2)
                    .BindResource(ForegroundProperty, "FgMutedBrush")
                    .Bind(
                        FluentBinding
                            .Bind(audioState, a => a.TotalTime, TextBlock.TextProperty)
                            .Mode(BindingMode.OneWay)
                            .Convert(FormatHelper.FormatPlaybackTime)
                    ),
            },
        };
    }

    protected override Action OnVisualTreeAttached()
    {
        return vm.Dispose;
    }
}
