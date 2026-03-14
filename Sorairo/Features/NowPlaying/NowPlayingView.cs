using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using R3;
using Sorairo.Common.Helpers;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.NowPlaying;

public sealed class NowPlayingView(
    AudioState audioState,
    PlaylistState playlistState,
    NowPlayingViewModel vm,
    AppState appState
) : ViewBase, IDisposable
{
    private PathIcon toggleRepeatButtonIcon = null!;
    private Button toggleRepeatButton = null!;
    private Button shuffleButton = null!;

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
                    new Panel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new Border
                            {
                                IsVisible = image is not null,
                                CornerRadius = new CornerRadius(8),
                                ClipToBounds = true,
                                Child = new Image { Source = image }.Bind(
                                    FluentBinding.OneWay(
                                        vm,
                                        vm => vm.FrontCoverStretch,
                                        Image.StretchProperty
                                    )
                                ),
                            }.GridRow(0),
                            new Button
                            {
                                VerticalAlignment = VerticalAlignment.Bottom,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Padding = new Thickness(4),
                                MaxHeight = 24,
                                Margin = new Thickness(0, 0, 0, -28),
                                Content = new PathIcon
                                {
                                    Width = 16,
                                    Height = 16,
                                    Data = Icons.Fill,
                                }
                                    .Bind(
                                        FluentBinding
                                            .OneWay(
                                                vm,
                                                vm => vm.FrontCoverStretch,
                                                PathIcon.DataProperty
                                            )
                                            .Convert(stretch =>
                                                stretch switch
                                                {
                                                    Stretch.UniformToFill => Icons.FillFilled,
                                                    _ => Icons.Fill,
                                                }
                                            )
                                    )
                                    .Bind(
                                        FluentBinding
                                            .OneWay(
                                                vm,
                                                vm => vm.FrontCoverStretch,
                                                ToolTip.TipProperty
                                            )
                                            .Convert(stretch =>
                                                stretch switch
                                                {
                                                    Stretch.UniformToFill => "Original size",
                                                    _ => "Fit to window",
                                                }
                                            )
                                    ),
                                Command = vm.ToggleFrontCoverStretchCommand,
                            },
                        },
                    },
                    new TextBlock
                    {
                        Text = "Now Playing",
                        Margin = new Thickness(0, 8, 0, 0),
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

    private Control PlaybackControls()
    {
        toggleRepeatButtonIcon = new PathIcon { Width = 14, Height = 14 };
        toggleRepeatButton = new Button
        {
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(6),
            Content = toggleRepeatButtonIcon,
            Command = vm.ToggleRepeatModeCommand,
        };
        shuffleButton = new Button
        {
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(6),
            Content = new PathIcon
            {
                Width = 14,
                Height = 14,
                Data = Icons.Shuffle,
            },
            Command = vm.ToggleShuffleModeCommand,
        };
        toggleRepeatButton.Styles.Add(
            new Style(selector =>
                Selectors.Or(
                    selector
                        .OfType<Button>()
                        .Class("repeat-mode--one")
                        .Descendant()
                        .OfType<PathIcon>(),
                    selector
                        .OfType<Button>()
                        .Class("repeat-mode--all")
                        .Descendant()
                        .OfType<PathIcon>()
                )
            )
            {
                Setters =
                {
                    new Setter
                    {
                        Property = ForegroundProperty,
                        Value = new DynamicResourceExtension("PrimaryFgBrush"),
                    },
                },
            }
        );
        shuffleButton.Styles.Add(
            new Style(selector =>
                selector
                    .OfType<Button>()
                    .Class("shuffle-mode--shuffle")
                    .Descendant()
                    .OfType<PathIcon>()
            )
            {
                Setters =
                {
                    new Setter
                    {
                        Property = ForegroundProperty,
                        Value = new DynamicResourceExtension("PrimaryFgBrush"),
                    },
                },
            }
        );
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 12,
            Children =
            {
                shuffleButton,
                new Button
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(6),
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
                    Padding = new Thickness(6),
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
                    Command = vm.TogglePlaybackCommand,
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
                                        Width = 14,
                                        Height = 14,
                                        Data = Icons.PauseFilled,
                                    },
                                    _ => new PathIcon
                                    {
                                        Width = 14,
                                        Height = 14,
                                        Data = Icons.PlayFilled,
                                    },
                                }
                            )
                    ),
                new Button
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(6),
                    Content = new PathIcon
                    {
                        Width = 14,
                        Height = 14,
                        Data = Icons.SkipNextFilled,
                    },
                    Command = vm.SkipNextCommand,
                },
                toggleRepeatButton,
                new Control { Width = 14 + 6 + 6, IsHitTestVisible = false }, // for alignment purpose
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
                vm.SeekCommand.Execute(elapsedSlider.Value);
                vm.IsSeeking = false;
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

    protected override void OnActivated(ref DisposableBag disposables)
    {
        playlistState
            .RepeatMode.Subscribe(mode =>
            {
                toggleRepeatButton.Classes.Set("repeat-mode--one", mode == RepeatMode.One);
                toggleRepeatButton.Classes.Set("repeat-mode--all", mode == RepeatMode.All);
                switch (mode)
                {
                    case RepeatMode.None:
                        ToolTip.SetTip(toggleRepeatButton, "Enable repeat");
                        toggleRepeatButtonIcon.Data = Icons.Repeat;
                        break;
                    case RepeatMode.All:
                        ToolTip.SetTip(toggleRepeatButton, "Enable repeat one");
                        toggleRepeatButtonIcon.Data = Icons.Repeat;
                        break;
                    case RepeatMode.One:
                        ToolTip.SetTip(toggleRepeatButton, "Disable repeat");
                        toggleRepeatButtonIcon.Data = Icons.RepeatOne;
                        break;
                }
            })
            .AddTo(ref disposables);
        playlistState
            .Shuffle.Mode.Subscribe(mode =>
            {
                shuffleButton.Classes.Set("shuffle-mode--shuffle", mode == ShuffleMode.Shuffle);
                switch (mode)
                {
                    case ShuffleMode.None:
                        ToolTip.SetTip(shuffleButton, "Enable shuffle");
                        break;
                    case ShuffleMode.Shuffle:
                        ToolTip.SetTip(shuffleButton, "Disable shuffle");
                        break;
                }
            })
            .AddTo(ref disposables);
    }

    public void Dispose()
    {
        vm.Dispose();
    }
}
