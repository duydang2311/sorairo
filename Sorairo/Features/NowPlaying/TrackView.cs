using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;
using Sorairo.Common.Helpers;
using Sorairo.Common.UI;

namespace Sorairo.Features.NowPlaying;

public sealed class TrackView(TrackViewModel vm) : ActivatableView
{
    protected override void Init()
    {
        Content = BuildContent();
    }

    protected override void OnActivated(ref DisposableBag disposables)
    {
        vm.Activate(ref disposables);
    }

    private Border BuildContent()
    {
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
                                CornerRadius = new CornerRadius(8),
                                ClipToBounds = true,
                                Child = new Image { }
                                    .Bind(
                                        FluentBinding.OneWay(
                                            vm,
                                            vm => vm.FrontCoverStretch,
                                            Image.StretchProperty
                                        )
                                    )
                                    .Bind(
                                        FluentBinding.OneWay(
                                            vm,
                                            vm => vm.FrontCoverImage,
                                            Image.SourceProperty
                                        )
                                    )
                                    .Do(image =>
                                    {
                                        RenderOptions.SetBitmapInterpolationMode(
                                            image,
                                            BitmapInterpolationMode.HighQuality
                                        );
                                    }),
                            }
                                .GridRow(0)
                                .Bind(
                                    FluentBinding
                                        .OneWay(vm, vm => vm.FrontCoverImage, IsVisibleProperty)
                                        .Convert(image => image is not null)
                                ),
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
                        Margin = new Thickness(0, 4, 0, 0),
                        FontSize = 18,
                        FontWeight = FontWeight.Medium,
                        TextWrapping = TextWrapping.WrapWithOverflow,
                    }
                        .GridRow(2)
                        .BindResource(ForegroundProperty, "FgEmphBrush")
                        .Bind(
                            FluentBinding
                                .OneWay(vm, vm => vm.Track, TextBlock.TextProperty)
                                .Convert(track => track.Title)
                        )
                        .Bind(
                            FluentBinding
                                .OneWay(vm, vm => vm.Track, IsVisibleProperty)
                                .Convert(track => !string.IsNullOrEmpty(track.Title))
                        ),
                    new TextBlock
                    {
                        Margin = new Thickness(0, 4, 0, 0),
                        FontWeight = FontWeight.SemiBold,
                    }
                        .GridRow(3)
                        .BindResource(ForegroundProperty, "PrimaryFgBrush")
                        .Bind(
                            FluentBinding
                                .OneWay(vm, vm => vm.Track, TextBlock.TextProperty)
                                .Convert(track => track.Artist)
                        ),
                },
            },
        };
    }
}
