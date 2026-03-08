using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using Sorairo.Common.Helpers;
using Sorairo.Common.Messages;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.MainMenu;

public sealed class MainMenuView(MainMenuViewModel vm, AppState appState) : LifecycleViewBase
{
    protected override void Init()
    {
        DataContext = vm;
        Content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Children =
            {
                new Menu
                {
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_File",
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
                        },
                        new MenuItem
                        {
                            Header = "_View",
                            Items =
                            {
                                new MenuItem
                                {
                                    Header = "_Playlist",
                                    Command = vm.ShowPlaylistViewCommand,
                                    Icon = new TextBlock
                                    {
                                        Text = "✓",
                                        IsHitTestVisible = false,
                                    }.Bind(
                                        FluentBinding
                                            .Bind(appState, a => a.MainView, IsVisibleProperty)
                                            .Convert(a => a == AppMainView.Playlist)
                                    ),
                                },
                            },
                        },
                    },
                }.GridColumn(0),
                new Button { Command = vm.ToggleRightPanelCommand }
                    .GridColumn(2)
                    .Bind(
                        FluentBinding
                            .OneWay(appState, a => a.MainView, ContentProperty)
                            .Convert(view =>
                                view switch
                                {
                                    AppMainView.None => new PathIcon
                                    {
                                        Width = 12,
                                        Height = 12,
                                        Data = Icons.RightPanel,
                                    },
                                    AppMainView.Playlist => new PathIcon
                                    {
                                        Width = 12,
                                        Height = 12,
                                        Data = Icons.RightPanelFilled,
                                    },
                                    _ => throw new InvalidProgramException(),
                                }
                            )
                    ),
            },
        };
    }

    protected override Action AttachVisualTree()
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
        return () =>
        {
            WeakReferenceMessenger.Default.Unregister<OpenSingleFileDialogMessage>(this);
            WeakReferenceMessenger.Default.Unregister<OpenMultiFilesDialogMessage>(this);
        };
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
}
