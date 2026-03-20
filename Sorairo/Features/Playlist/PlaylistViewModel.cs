using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ObservableCollections;
using R3;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.Playlist;

public sealed partial class PlaylistViewModel(
    IPlaylistService playlistService,
    PlaylistState playlistState
) : ActivatableViewModel
{
    [ObservableProperty]
    private INotifyCollectionChangedSynchronizedViewList<Track>? items;

    protected override void Init() { }

    protected override void OnActivated(ref DisposableBag disposables)
    {
        Items = playlistState.Tracks.ToNotifyCollectionChangedSlim().AddTo(ref disposables);
    }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play(Track item)
    {
        playlistService.SetCurrentTrack(item);
        var result = playlistService.Play();
        Console.WriteLine(result.Value);
    }

    private bool CanPlay(Track item)
    {
        return playlistState.CurrentTrack != item;
    }
}
