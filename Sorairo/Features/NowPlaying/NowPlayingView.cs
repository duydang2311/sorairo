using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
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
    NowPlayingViewModel vm,
    AudioState audioState,
    PlaylistState playlistState,
    AppState appState
) : ActivatableView
{
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
                        ElapsedProgressBar(),
                        new Border
                        {
                            Padding = new Thickness(16, 4, 16, 0),
                            Child = ElapsedTexts(),
                        },
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
                                                    HorizontalAlignmentProperty
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
                        .Bind(playlistState, a => a.CurrentTrack, ContentProperty)
                        .Mode(BindingMode.OneWay)
                        .Convert(item =>
                        {
                            if (frontCoverImage is not null)
                            {
                                frontCoverImage.Dispose();
                                frontCoverImage = null;
                            }
                            return item is null ? null : PlayingView(item);
                        })
                ),
            },
        };
    }

    Bitmap? frontCoverImage;

    private Border PlayingView(Track item)
    {
        if (frontCoverImage is not null)
        {
            frontCoverImage.Dispose();
            frontCoverImage = null;
        }
        var frontCover = item.GetFrontCover();
        if (frontCover is not null)
        {
            using var ms = new MemoryStream(frontCover);
            frontCoverImage = new Bitmap(ms);
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
                                IsVisible = frontCoverImage is not null,
                                CornerRadius = new CornerRadius(8),
                                ClipToBounds = true,
                                Child = new Image { Source = frontCoverImage }
                                    .Bind(
                                        FluentBinding.OneWay(
                                            vm,
                                            vm => vm.FrontCoverStretch,
                                            Image.StretchProperty
                                        )
                                    )
                                    .Do(image =>
                                    {
                                        RenderOptions.SetBitmapInterpolationMode(
                                            image,
                                            BitmapInterpolationMode.HighQuality
                                        );
                                    }),
                            }.GridRow(0),
                            new Button
                            {
                                VerticalAlignment = VerticalAlignment.Bottom,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Padding = new Thickness(4),
                                MaxHeight = Icons.MD + 8,
                                Margin = new Thickness(0, 0, 0, -Icons.MD - 8 - 4),
                                Content = new PathIcon
                                {
                                    Width = Icons.MD,
                                    Height = Icons.MD,
                                    Data = Icons.Fill16,
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
                                                    Stretch.UniformToFill => Icons.Fill16Filled,
                                                    _ => Icons.Fill16,
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

    private StackPanel VolumeSlider()
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Children =
            {
                new PathIcon
                {
                    Width = Icons.MD,
                    Height = Icons.MD,
                    Data = Icons.VolumeHigh,
                }
                    .GridColumn(0)
                    .Bind(
                        FluentBinding
                            .OneWay(vm, vm => vm.VolumeStatus, PathIcon.DataProperty)
                            .Convert(status =>
                                status switch
                                {
                                    VolumeStatus.Zero => Icons.VolumeZero,
                                    VolumeStatus.Low => Icons.VolumeLow,
                                    VolumeStatus.High => Icons.VolumeHigh,
                                    VolumeStatus.Muted => Icons.VolumeMuted,
                                    _ => Icons.VolumeZero,
                                }
                            )
                    ),
                new Slider
                {
                    Minimum = 0,
                    Maximum = 1,
                    Width = 96,
                }
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
        toggleRepeatButton = new Button
        {
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(6),
            Content = new PathIcon { Width = Icons.MD, Height = Icons.MD }.Bind(
                FluentBinding
                    .OneWay(playlistState, state => state.RepeatMode, PathIcon.DataProperty)
                    .Convert(mode =>
                        mode switch
                        {
                            RepeatMode.None => Icons.RepeatOff,
                            RepeatMode.All => Icons.RepeatAll,
                            RepeatMode.One => Icons.RepeatOne,
                            _ => Icons.RepeatOff,
                        }
                    )
            ),
            Command = vm.ToggleRepeatModeCommand,
        }.Bind(
            FluentBinding
                .OneWay(playlistState, state => state.RepeatMode, ToolTip.TipProperty)
                .Convert(mode =>
                    mode switch
                    {
                        RepeatMode.All => "Enable repeat one",
                        RepeatMode.One => "Disable repeat",
                        _ => "Enable repeat",
                    }
                )
        );
        shuffleButton = new Button
        {
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(6),
            Content = new PathIcon { Width = Icons.MD, Height = Icons.MD }.Bind(
                FluentBinding
                    .OneWay(playlistState.Shuffle, shuffle => shuffle.Mode, PathIcon.DataProperty)
                    .Convert(mode =>
                        mode switch
                        {
                            ShuffleMode.None => Icons.ShuffleOff,
                            ShuffleMode.Shuffle => Icons.Shuffle,
                            _ => Icons.ShuffleOff,
                        }
                    )
            ),
            Command = vm.ToggleShuffleModeCommand,
        }.Bind(
            FluentBinding
                .OneWay(playlistState.Shuffle, shuffle => shuffle.Mode, ToolTip.TipProperty)
                .Convert(mode =>
                    mode switch
                    {
                        ShuffleMode.Shuffle => "Disable shuffle",
                        _ => "Enable shuffle",
                    }
                )
        );
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
                        Width = Icons.MD,
                        Height = Icons.MD,
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
                        Width = Icons.MD,
                        Height = Icons.MD,
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
                                        Width = Icons.MD,
                                        Height = Icons.MD,
                                        Data = Icons.PauseFilled,
                                    },
                                    _ => new PathIcon
                                    {
                                        Width = Icons.MD,
                                        Height = Icons.MD,
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
                        Width = Icons.MD,
                        Height = Icons.MD,
                        Data = Icons.SkipNextFilled,
                    },
                    Command = vm.SkipNextCommand,
                },
                toggleRepeatButton,
                new Control { Width = Icons.MD + 6 + 6, IsHitTestVisible = false }, // for alignment purpose
            },
        };
    }

    private ProgressBar ElapsedProgressBar()
    {
        var progressBar = new ProgressBar { Margin = new Thickness(0, -8, 0, 0), Minimum = 0 }
            .Bind(
                FluentBinding
                    .OneWay(audioState, a => a.TotalTime, ProgressBar.MaximumProperty)
                    .Convert(a => Math.Max(1, a.TotalSeconds))
            )
            .Bind(FluentBinding.OneWay(vm, a => a.ElapsedSeconds, ProgressBar.ValueProperty));
        progressBar.PointerPressed += (sender, e) =>
        {
            var progressBar = (ProgressBar)sender!;
            vm.IsSeeking = true;
            progressBar.Value = GetDragValue(progressBar, e);
        };
        progressBar.PointerMoved += (sender, e) =>
        {
            if (vm.IsSeeking)
            {
                var progressBar = (ProgressBar)sender!;
                progressBar.Value = GetDragValue(progressBar, e);
            }
        };
        progressBar.PointerReleased += (sender, e) =>
        {
            if (vm.IsSeeking)
            {
                var progressBar = (ProgressBar)sender!;
                progressBar.Value = GetDragValue(progressBar, e);
                vm.SeekCommand.Execute(progressBar.Value);
                vm.IsSeeking = false;
            }
        };
        static double GetDragValue(ProgressBar progressBar, PointerEventArgs e)
        {
            var position = e.GetPosition(progressBar);
            var percent = Math.Min(Math.Max(0, position.X) / progressBar.Bounds.Width, 1);
            return percent * progressBar.Maximum;
        }
        return progressBar;
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
        vm.Activate(ref disposables);
        disposables.Add(
            Disposable.Create(
                frontCoverImage,
                static (frontCoverImage) =>
                {
                    if (frontCoverImage is not null)
                    {
                        frontCoverImage.Dispose();
                        frontCoverImage = null;
                    }
                }
            )
        );
        playlistState
            .ObservePropertyChanged(state => state.RepeatMode)
            .Subscribe(mode =>
            {
                toggleRepeatButton.Classes.Set("repeat-mode--one", mode == RepeatMode.One);
                toggleRepeatButton.Classes.Set("repeat-mode--all", mode == RepeatMode.All);
            })
            .AddTo(ref disposables);
        playlistState
            .Shuffle.ObservePropertyChanged(state => state.Mode)
            .Subscribe(mode =>
            {
                shuffleButton.Classes.Set("shuffle-mode--shuffle", mode == ShuffleMode.Shuffle);
            })
            .AddTo(ref disposables);
    }
}
