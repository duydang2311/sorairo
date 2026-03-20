using CommunityToolkit.Mvvm.ComponentModel;
using ObservableCollections;

namespace Sorairo.Common.Models;

public sealed partial class PlaylistState : ObservableObject
{
    public ObservableList<Track> Tracks { get; } = [];

    [ObservableProperty]
    private Track? currentTrack;

    [ObservableProperty]
    private RepeatMode repeatMode = RepeatMode.All;

    public PlaylistShuffle Shuffle { get; } = new();
}

public sealed partial class PlaylistShuffle : ObservableObject
{
    [ObservableProperty]
    private ShuffleMode mode = ShuffleMode.None;

    [ObservableProperty]
    private ShuffleState? state;
}

public sealed partial class ShuffleState : ObservableObject
{
    public ObservableList<Guid> Ids { get; }

    [ObservableProperty]
    private Guid? currentId;

    public ShuffleState(IEnumerable<Guid> ids)
    {
        Ids = [.. ids];
        CurrentId = Ids.ElementAtOrDefault(0);
    }
}
