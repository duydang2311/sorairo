using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
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
    NowPlayingViewModel vm
) : ViewBase
{
    protected override void Init()
    {
        Content = new DockPanel
        {
            Children =
            {
                new Border
                {
                    Padding = new Thickness(8),
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    Child = VolumeSliderView(),
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
        if (item.FrontCover is not null)
        {
            using var ms = new MemoryStream(item.FrontCover);
            image = new Bitmap(ms);
        }
        var elapsedSlider = new Slider { Margin = new Thickness(0, 16, 0, 0), Minimum = 0 }
            .Bind(
                FluentBinding
                    .Bind(audioState, a => a.TotalTime, Slider.MaximumProperty)
                    .Mode(BindingMode.OneWay)
                    .Convert(a => a.TotalSeconds)
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
                        Console.WriteLine("Is seeking: " + isSeeking);
                        return isSeeking
                            ? null
                            : Transitions = new Transitions
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
            (_, _) => vm.IsSeeking = true,
            RoutingStrategies.Tunnel
        );
        elapsedSlider.AddHandler(
            Slider.PointerReleasedEvent,
            (_, _) => vm.IsSeeking = false,
            RoutingStrategies.Tunnel
        );
        elapsedSlider.ValueChanged += (_, e) =>
        {
            if (vm.IsSeeking)
            {
                vm.SeekCommand.Execute(e.NewValue);
            }
        };
        return new Border
        {
            Padding = new Thickness(32, 16),
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new Border
                    {
                        IsVisible = image is not null,
                        CornerRadius = new CornerRadius(2),
                        ClipToBounds = true,
                        MaxHeight = 480,
                        Child = new Image { Source = image, Stretch = Stretch.Uniform },
                    },
                    new TextBlock
                    {
                        Text = "Now Playing",
                        Margin = new Thickness(0, 16, 0, 0),
                        FontSize = 10,
                    }.BindResource(ForegroundProperty, "FgMutedBrush"),
                    new TextBlock
                    {
                        Text = item.Title,
                        Margin = new Thickness(0, 4, 0, 0),
                        IsVisible = !string.IsNullOrEmpty(item.Title),
                        FontSize = 18,
                        FontWeight = FontWeight.Medium,
                        TextWrapping = TextWrapping.WrapWithOverflow,
                    }.BindResource(ForegroundProperty, "FgEmphBrush"),
                    new TextBlock
                    {
                        Text = item.Artist,
                        Margin = new Thickness(0, 4, 0, 0),
                        FontWeight = FontWeight.SemiBold,
                    }.BindResource(ForegroundProperty, "PrimaryFgBrush"),
                    elapsedSlider,
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
                        Children =
                        {
                            new TextBlock { FontSize = 10, FontWeight = FontWeight.Bold }
                                .GridColumn(0)
                                .BindResource(ForegroundProperty, "FgMutedBrush")
                                .Bind(
                                    FluentBinding
                                        .Bind(
                                            audioState,
                                            a => a.ElapsedTime,
                                            TextBlock.TextProperty
                                        )
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
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Spacing = 16,
                        Children =
                        {
                            new Button
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                Padding = new Thickness(12),
                                Content = new PathIcon
                                {
                                    Width = 14,
                                    Height = 14,
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
                                    Width = 14,
                                    Height = 14,
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
                                                    Width = 16,
                                                    Height = 16,
                                                    Data = Icons.PauseFilled,
                                                },
                                                _ => new PathIcon
                                                {
                                                    Width = 16,
                                                    Height = 16,
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
                                    Width = 14,
                                    Height = 14,
                                    Data = Icons.SkipNextFilled,
                                },
                                Command = vm.SkipNextCommand,
                            },
                            new Control { Width = 38, IsHitTestVisible = false }, // ghost btn for now
                        },
                    },
                },
            },
        };
    }

    private Grid VolumeSliderView()
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
                    ),
            },
        };
    }
}
