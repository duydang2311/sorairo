using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using Sorairo.Common.Messaging;

namespace Sorairo.Features.MainMenu;

public sealed class MainMenuView : UserControl
{
    public MainMenuView(MainMenuViewModel vm)
    {
        DataContext = vm;
        Content = new Menu
        {
            Items =
            {
                new MenuItem
                {
                    Header = "_File",
                    Items =
                    {
                        new MenuItem { Header = "_Open file", Command = vm.OpenFileCommand },
                        new MenuItem { Header = "_Open folder" },
                        new Separator(),
                        new MenuItem { Header = "_Add files" },
                        new MenuItem { Header = "_Add folder" },
                        new Separator(),
                        new MenuItem { Header = "E_xit" },
                    },
                },
            },
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
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
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        WeakReferenceMessenger.Default.Unregister<OpenSingleFileDialogMessage>(this);
    }
}
