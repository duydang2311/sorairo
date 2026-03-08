using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sorairo.Common.Models;

public sealed partial class PlaylistState : ObservableObject
{
    public ObservableCollection<PlaylistItem> Items { get; } = [];

    [ObservableProperty]
    private PlaylistItem? currentItem;
}
