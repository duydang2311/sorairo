using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ObservableCollections;
using R3;

namespace Sorairo.Common.Models;

public sealed partial class PlaylistState : ObservableObject
{
    public ObservableCollection<PlaylistItem> Items { get; } = [];

    [ObservableProperty]
    private PlaylistItem? currentItem;

    public ReactiveProperty<RepeatMode> RepeatMode { get; } = new(Models.RepeatMode.None);
    public PlaylistShuffle Shuffle { get; } = new();
}

public sealed record PlaylistShuffle
{
    public ReactiveProperty<ShuffleMode> Mode { get; } = new(ShuffleMode.None);
    public ReactiveProperty<ShuffleState?> State { get; set; } = new();
}

public sealed record ShuffleState
{
    public ObservableList<Guid> Ids { get; }
    public ReactiveProperty<Guid?> CurrentId { get; }

    public ShuffleState(IEnumerable<Guid> ids)
    {
        Ids = [.. ids];
        CurrentId = new(Ids.ElementAtOrDefault(0));
    }
}
