using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sorairo.Common.Messaging;
using Sorairo.Infras.Audio;

namespace Sorairo.Features.MainMenu;

public sealed partial class MainMenuViewModel(IAudioService audioService) : ObservableObject
{
    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var path = await WeakReferenceMessenger.Default.Send(new OpenSingleFileDialogMessage());
        if (path is null)
        {
            return;
        }
        audioService.Play(path);
    }
}
