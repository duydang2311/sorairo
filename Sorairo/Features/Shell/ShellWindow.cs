using Ardalis.GuardClauses;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using R3;
using R3.Avalonia;
using Sorairo.Common.Helpers;
using Sorairo.Common.Models;
using Sorairo.Common.UI;
using Sorairo.Features.NowPlaying;
using Sorairo.Features.Playlist;
using Sorairo.Features.TitleBar;

namespace Sorairo.Features.Shell;

public sealed class ShellWindow(
    ShellWindowViewModel vm,
    IServiceProvider serviceProvider,
    AppState appState,
    NowPlayingView nowPlayingView,
    TitleBarView titleBarView,
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
        WindowDecorations = WindowDecorations.BorderOnly;
        Content = CreateContent();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        vm.Dispose();
        frameProviderContext.FrameProvider.Dispose();
    }

    private Border CreateContent()
    {
        var mainContentControl = new ContentControl();
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
                        Child = titleBarView,
                    }
                        .Dock(Dock.Top)
                        .BindResource(BorderBrushProperty, "SurfaceBorderBrush")
                        .BindResource(BackgroundProperty, "SurfaceBrush")
                        .Bind(FluentBinding.OneWay(vm, vm => vm.WindowPadding, PaddingProperty))
                        .Bind(
                            FluentBinding
                                .OneWay(this, a => a.OffScreenMargin, HeightProperty)
                                .Convert(thickness =>
                                {
                                    return 31 - thickness.Top;
                                })
                        )
                        .Do(border =>
                        {
                            WindowDecorationProperties.SetElementRole(
                                border,
                                WindowDecorationsElementRole.TitleBar
                            );
                        }),
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
        }.Bind(FluentBinding.OneWay(this, window => window.OffScreenMargin, PaddingProperty));
    }
}
