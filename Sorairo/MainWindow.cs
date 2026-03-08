using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Sorairo.Common.Helpers;
using Sorairo.Common.Models;
using Sorairo.Common.UI;
using Sorairo.Features.MainMenu;
using Sorairo.Features.NowPlaying;
using Sorairo.Features.Playlist;

namespace Sorairo;

public sealed class MainWindow(
    MainMenuView mainMenuView,
    IServiceProvider serviceProvider,
    AppState appState,
    NowPlayingView nowPlayingView
) : LifecycleWindowBase
{
    protected override void Init()
    {
        MinWidth = 320;
        Width = 960;
        Height = 720;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaChromeHints = Avalonia
            .Platform
            .ExtendClientAreaChromeHints
            .PreferSystemChrome;
        Padding = new Thickness();
        Margin = new Thickness();
        Content = CreateContent();
    }

    protected override Action WhenOpened()
    {
        appState.PropertyChanged += OnAppStatePropertyChanged;
        return () =>
        {
            appState.PropertyChanged -= OnAppStatePropertyChanged;
        };
    }

    private void OnAppStatePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(AppState.MainView))
        {
            mainContentControl ??= new();
            switch (appState.MainView)
            {
                case AppMainView.None:
                    mainContentControl.IsVisible = false;
                    mainContentControl.Content = null;
                    break;
                case AppMainView.Playlist:
                    mainContentControl.IsVisible = true;
                    mainContentControl.Content = serviceProvider.GetRequiredService<PlaylistView>();
                    break;
            }
        }
    }

    private ContentControl? mainContentControl;

    private Border CreateContent()
    {
        mainContentControl ??= new ContentControl();
        this
        // .Style(
        //     new Style(a => a.OfType<NowPlayingView>())
        //     {
        //         Setters =
        //         {
        //             // new Setter(MinWidthProperty, 400.0),
        //             new Setter(MaxWidthProperty, 600.0),
        //         },
        //     }
        // )
        .Style(
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
                        .Bind(
                            FluentBinding
                                .OneWay(this, a => a.RenderScaling, PaddingProperty)
                                .Convert(scale => new Thickness(0, 0, 138 * scale, 0))
                        )
                        .Bind(
                            FluentBinding
                                .OneWay(this, a => a.OffScreenMargin, PaddingProperty)
                                .Convert(thickness => new Thickness(
                                    Math.Max(16, thickness.Left),
                                    0,
                                    thickness.Right + 138 + 16, // 138 for windows decorations, fixed size for now
                                    0
                                ))
                        )
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
                            nowPlayingView.GridColumn(0).SpanColumn(4),
                            new Border { Background = new SolidColorBrush(Colors.Black, 0.1) }
                                .Bind(
                                    FluentBinding
                                        .OneWay(appState, a => a.MainView, IsVisibleProperty)
                                        .Convert(view => view is AppMainView.Playlist)
                                )
                                .GridColumn(0)
                                .SpanColumn(4),
                            mainContentControl
                                .BindResource(BackgroundProperty, "SurfaceBrush")
                                .Bind(
                                    FluentBinding
                                        .OneWay(appState, a => a.MainView, IsVisibleProperty)
                                        .Convert(view => view is AppMainView.Playlist)
                                )
                                .GridColumn(1)
                                .SpanColumn(3),
                            // .Bind(
                            //     FluentBinding
                            //         .OneWay(appState, a => a.MainView, Grid.ColumnSpanProperty)
                            //         .Convert(view =>
                            //             view switch
                            //             {
                            //                 AppMainView.Playlist => 4,
                            //                 _ => 2,
                            //             }
                            //         )
                            // ),
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
