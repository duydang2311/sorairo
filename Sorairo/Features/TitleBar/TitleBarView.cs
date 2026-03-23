using Ardalis.GuardClauses;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;
using R3;
using Sorairo.Common.Helpers;
using Sorairo.Common.Messages;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.TitleBar;

public sealed class TitleBarView(TitleBarViewModel vm, AppState appState) : ActivatableView
{
    private Button? minimizeButton;
    private Button? maximizeButton;
    private Button? closeButton;

    protected override void Init()
    {
        Content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto,Auto,Auto"),
            Children =
            {
                new TextBlock
                {
                    Text = "Sorairo",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeight.Bold,
                }
                    .GridColumn(0)
                    .BindResource(ForegroundProperty, "PrimaryFgBrush"),
                new Menu
                {
                    Margin = new Thickness(8, 0, 0, 0),
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_File",
                            CornerRadius = new CornerRadius(),
                            Items =
                            {
                                new MenuItem
                                {
                                    Header = "_Open file",
                                    Command = vm.OpenFileCommand,
                                },
                                new MenuItem { Header = "_Open folder" },
                                new Separator(),
                                new MenuItem
                                {
                                    Header = "_Add files",
                                    Command = vm.AddFilesCommand,
                                },
                                new MenuItem { Header = "_Add folder" },
                                new Separator(),
                                new MenuItem
                                {
                                    Header = "_New playlist",
                                    Command = vm.NewPlaylistCommand,
                                },
                                new Separator(),
                                new MenuItem { Header = "E_xit" },
                            },
                        }.SetElementRole(WindowDecorationsElementRole.User),
                    },
                }.GridColumn(1),
                new Button
                {
                    Width = 48,
                    Height = 32,
                    CornerRadius = new CornerRadius(0),
                    Command = vm.ToggleRightPanelCommand,
                }
                    .GridColumn(2)
                    .Bind(
                        FluentBinding
                            .OneWay(appState, a => a.MainView, ContentProperty)
                            .Convert(view =>
                                view switch
                                {
                                    AppMainView.None => new PathIcon
                                    {
                                        Width = Icons.XS,
                                        Height = Icons.XS,
                                        Data = Icons.RightPanel16,
                                    },
                                    AppMainView.Playlist => new PathIcon
                                    {
                                        Width = Icons.XS,
                                        Height = Icons.XS,
                                        Data = Icons.RightPanel16Filled,
                                    },
                                    _ => throw new InvalidProgramException(),
                                }
                            )
                    )
                    .SetElementRole(WindowDecorationsElementRole.User),
                new Button
                {
                    Width = 48,
                    Height = 32,
                    CornerRadius = new CornerRadius(),
                    Content = new PathIcon
                    {
                        Width = Icons.XS,
                        Height = Icons.XS,
                        Data = Icons.ChromeMinimize,
                    },
                }
                    .GridColumn(3)
                    .SetElementRole(WindowDecorationsElementRole.MinimizeButton)
                    .Do(
                        this,
                        static (button, view) =>
                        {
                            WindowDecorationProperties.SetElementRole(
                                button,
                                WindowDecorationsElementRole.MinimizeButton
                            );
                            AutomationProperties.SetAutomationId(button, "Minimize");
                            AutomationProperties.SetName(button, "Minimize");
                            view.minimizeButton = button;
                        }
                    ),
                new Button
                {
                    Width = 48,
                    Height = 32,
                    CornerRadius = new CornerRadius(),
                    Content = new PathIcon { Width = Icons.XS, Height = Icons.XS }.Do(icon =>
                    {
                        icon.AttachedToVisualTree += static (sender, _) =>
                        {
                            var icon = (PathIcon)sender!;
                            icon.Bind(
                                FluentBinding
                                    .OneWay(
                                        (Window)TopLevel.GetTopLevel(icon)!,
                                        window => window.WindowState,
                                        PathIcon.DataProperty
                                    )
                                    .Convert(state =>
                                        state switch
                                        {
                                            WindowState.Maximized => Icons.ChromeRestore,
                                            _ => Icons.ChromeMaximize,
                                        }
                                    )
                            );
                        };
                    }),
                }
                    .GridColumn(4)
                    .SetElementRole(WindowDecorationsElementRole.MaximizeButton)
                    .Do(
                        this,
                        static (button, view) =>
                        {
                            WindowDecorationProperties.SetElementRole(
                                button,
                                WindowDecorationsElementRole.MaximizeButton
                            );
                            AutomationProperties.SetAutomationId(button, "Maximize");
                            AutomationProperties.SetName(button, "Maximize");
                            view.maximizeButton = button;
                        }
                    ),
                new Button
                {
                    Width = 48,
                    Height = 32,
                    CornerRadius = new CornerRadius(),
                    Content = new PathIcon
                    {
                        Width = Icons.XS,
                        Height = Icons.XS,
                        Data = Icons.ChromeClose,
                    },
                }
                    .GridColumn(5)
                    .Do(
                        this,
                        static (button, view) =>
                        {
                            WindowDecorationProperties.SetElementRole(
                                button,
                                WindowDecorationsElementRole.CloseButton
                            );
                            AutomationProperties.SetAutomationId(button, "Close");
                            AutomationProperties.SetName(button, "Close");
                            view.closeButton = button;
                            button.Styles.Add(
                                new Style(selector =>
                                    Selectors.Or(
                                        selector.OfType<Button>().Class(":pointerover"),
                                        selector.OfType<Button>().Class(":pressed")
                                    )
                                )
                                {
                                    Setters =
                                    {
                                        new Setter(
                                            BackgroundProperty,
                                            new DynamicResourceExtension("ChromeCloseBrush")
                                        ),
                                    },
                                }
                            );
                        }
                    ),
            },
        }.Do(grid =>
        {
            WindowDecorationProperties.SetElementRole(grid, WindowDecorationsElementRole.TitleBar);
            AutomationProperties.SetIsControlElementOverride(grid, true);
            AutomationProperties.SetAutomationId(grid, "AvaloniaTitleBar");
            AutomationProperties.SetName(grid, "TitleBar");
        });
    }

    protected override void OnActivated(ref DisposableBag disposables)
    {
        WeakReferenceMessenger.Default.Register<OpenSingleFileDialogMessage>(
            this,
            (recipient, msg) =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel is null)
                {
                    msg.Reply((Uri?)null);
                    return;
                }

                msg.Reply(OpenFileAsync(topLevel));
            }
        );
        WeakReferenceMessenger.Default.Register<OpenMultiFilesDialogMessage>(
            this,
            (recipient, msg) =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel is null)
                {
                    msg.Reply([]);
                    return;
                }

                msg.Reply(OpenFilesAsync(topLevel));
            }
        );
        Guard.Against.Null(minimizeButton).Click += OnMinimizeButtonClicked;
        Guard.Against.Null(maximizeButton).Click += OnMaximizeButtonClicked;
        Guard.Against.Null(closeButton).Click += OnCloseButtonClicked;
        disposables.Add(
            Disposable.Create(() =>
            {
                minimizeButton.Click -= OnMinimizeButtonClicked;
                maximizeButton.Click -= OnMaximizeButtonClicked;
                closeButton.Click -= OnCloseButtonClicked;
                WeakReferenceMessenger.Default.Unregister<OpenSingleFileDialogMessage>(this);
                WeakReferenceMessenger.Default.Unregister<OpenMultiFilesDialogMessage>(this);
            })
        );
    }

    private static async Task<Uri?> OpenFileAsync(TopLevel topLevel)
    {
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open audio file",
                FileTypeFilter =
                [
                    new("audio")
                    {
                        Patterns = ["*.mp3", "*.wav"],
                        AppleUniformTypeIdentifiers =
                        [
                            "public.mp3",
                            "com.microsoft.waveform-audio",
                        ],
                        MimeTypes = ["audio/mpeg", "audio/wav"],
                    },
                ],
                AllowMultiple = false,
            }
        );

        return files.Count > 0 ? files[0].Path : null;
    }

    private static async Task<List<Uri>> OpenFilesAsync(TopLevel topLevel)
    {
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open audio file",
                FileTypeFilter =
                [
                    new("audio")
                    {
                        Patterns = ["*.mp3", "*.wav"],
                        AppleUniformTypeIdentifiers =
                        [
                            "public.mp3",
                            "com.microsoft.waveform-audio",
                        ],
                        MimeTypes = ["audio/mpeg", "audio/wav"],
                    },
                ],
                AllowMultiple = true,
            }
        );

        return [.. files.Select(a => a.Path)];
    }

    private static void OnMinimizeButtonClicked(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;
        var topLevel = (Window)TopLevel.GetTopLevel(button)!;
        topLevel.WindowState = WindowState.Minimized;
        e.Handled = true;
    }

    private static void OnMaximizeButtonClicked(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;
        var topLevel = (Window)TopLevel.GetTopLevel(button)!;
        topLevel.WindowState = topLevel.WindowState switch
        {
            WindowState.Maximized => WindowState.Normal,
            _ => WindowState.Maximized,
        };
        e.Handled = true;
    }

    private static void OnCloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;
        var topLevel = (Window)TopLevel.GetTopLevel(button)!;
        topLevel.Close();
        e.Handled = true;
    }
}
