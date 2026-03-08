using CommunityToolkit.Mvvm.Input;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.Playlist;

public sealed partial class PlaylistViewModel(
    IPlaylistService playlistService,
    PlaylistState playlistState
) : ViewModelBase
{
    protected override void Init() { }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play(PlaylistItem item)
    {
        playlistService.SetCurrentItem(item);
        var result = playlistService.Play();
        Console.WriteLine(result.Value);
    }

    private bool CanPlay(PlaylistItem item)
    {
        return playlistState.CurrentItem != item;
    }
}
