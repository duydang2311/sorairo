using Ardalis.GuardClauses;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using R3.Avalonia;
using Sorairo.Common.Helpers;
using Sorairo.Common.Models;
using Sorairo.Common.UI;
using Sorairo.Features.MainMenu;
using Sorairo.Features.NowPlaying;
using Sorairo.Features.Playlist;

namespace Sorairo.Features.Shell;

public sealed class ShellWindow(
    MainMenuView mainMenuView,
    IServiceProvider serviceProvider,
    AppState appState,
    NowPlayingView nowPlayingView,
    ShellWindowViewModel vm,
    FrameProviderContext frameProviderContext
) : InitWindowBase
{
    protected override void Init()
    {
        var topLevel = GetTopLevel(this);
        Guard.Against.Null(topLevel);
        frameProviderContext.Initialize(new AvaloniaRenderingFrameProvider(topLevel));
        this.Bind(
            FluentBinding
                .Bind(appState, a => a.WindowWidth, WidthProperty)
                .Mode(BindingMode.OneWayToSource)
        );
        this.Bind(
            FluentBinding
                .Bind(vm, vm => vm.WindowState, WindowStateProperty)
                .Mode(BindingMode.OneWayToSource)
        );
        this.Bind(
            FluentBinding
                .Bind(vm, vm => vm.OffscreenMargin, OffScreenMarginProperty)
                .Mode(BindingMode.OneWayToSource)
        );
        MinWidth = 400;
        Width = 960;
        Height = 720;
        ExtendClientAreaToDecorationsHint = true;
        Content = CreateContent();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        nowPlayingView.Dispose();
        vm.Dispose();
        frameProviderContext.FrameProvider.Dispose();
    }

    private Border CreateContent()
    {
        var mainContentControl = new ContentControl();
        this.Style(
            new Style(a => a.OfType<MainMenuView>())
            {
                Setters = { new Setter(VerticalAlignmentProperty, VerticalAlignment.Center) },
            }
        );
        return new Border
        {
            Child = new DockPanel()
            {
                Children =
                {
                    new Border
                    {
                        Height = 31,
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        Child = new Grid
                        {
                            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "Sorairo",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(0, 0, 16, 0),
                                    FontWeight = FontWeight.Bold,
                                }
                                    .BindResource(ForegroundProperty, "PrimaryFgBrush")
                                    .GridColumn(0),
                                mainMenuView.GridColumn(1),
                            },
                        },
                    }
                        .Dock(Dock.Top)
                        .BindResource(BorderBrushProperty, "SurfaceBorderBrush")
                        .Bind(FluentBinding.OneWay(vm, vm => vm.WindowPadding, PaddingProperty))
                        .Bind(
                            FluentBinding
                                .OneWay(this, a => a.OffScreenMargin, HeightProperty)
                                .Convert(thickness =>
                                {
                                    return 31 - thickness.Top;
                                })
                        ),
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
                        Children =
                        {
                            nowPlayingView
                                .GridColumn(0)
                                .Bind(
                                    FluentBinding
                                        .OneWay(
                                            vm,
                                            a => a.PlaylistVisibility,
                                            Grid.ColumnSpanProperty
                                        )
                                        .Convert(visibility =>
                                            visibility switch
                                            {
                                                PlaylistVisibility.VisibleAsPanel => 2,
                                                _ => 4,
                                            }
                                        )
                                ),
                            new Border { Background = new SolidColorBrush(Colors.Black, 0.1) }
                                .Bind(
                                    FluentBinding
                                        .OneWay(vm, a => a.PlaylistVisibility, IsVisibleProperty)
                                        .Convert(visibility =>
                                            visibility == PlaylistVisibility.VisibleAsOverlay
                                        )
                                )
                                .GridColumn(0)
                                .SpanColumn(4),
                            mainContentControl
                                .BindResource(BackgroundProperty, "SurfaceBrush")
                                .Bind(
                                    FluentBinding
                                        .OneWay(appState, a => a.MainView, IsVisibleProperty)
                                        .Convert(view => view == AppMainView.Playlist)
                                )
                                .Bind(
                                    FluentBinding
                                        .OneWay(appState, a => a.MainView, ContentProperty)
                                        .Convert(view =>
                                            view switch
                                            {
                                                AppMainView.Playlist =>
                                                    serviceProvider.GetRequiredService<PlaylistView>(),
                                                _ => null,
                                            }
                                        )
                                )
                                .Bind(
                                    FluentBinding
                                        .OneWay(vm, a => a.PlaylistVisibility, Grid.ColumnProperty)
                                        .Convert(visibility =>
                                            visibility switch
                                            {
                                                PlaylistVisibility.VisibleAsPanel => 2,
                                                _ => 1,
                                            }
                                        )
                                )
                                .Bind(
                                    FluentBinding
                                        .OneWay(
                                            vm,
                                            a => a.PlaylistVisibility,
                                            Grid.ColumnSpanProperty
                                        )
                                        .Convert(visibility =>
                                            visibility switch
                                            {
                                                PlaylistVisibility.VisibleAsPanel => 2,
                                                _ => 3,
                                            }
                                        )
                                ),
                        },
                    },
                },
            },
        }.WithBind(
            PaddingProperty,
            new Binding(nameof(OffScreenMargin), BindingMode.OneWay) { Source = this }
        );
    }
}
