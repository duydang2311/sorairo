using ATL;
using OneOf;
using OneOf.Types;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;

namespace Sorairo.Common.Services;

public sealed class PlaylistService(PlaylistState state, IAudioService audioService)
    : IPlaylistService
{
    public PlaylistItem AddItem(Uri path)
    {
        var track = new Track(path.LocalPath);
        var item = new PlaylistItem
        {
            Id = Guid.NewGuid(),
            Path = path,
            Artist = track.Artist,
            Title = track.Title,
            Album = track.Album,
            FrontCover = track
                .EmbeddedPictures.FirstOrDefault(a => a.PicType == PictureInfo.PIC_TYPE.Front)
                ?.PictureData,
        };
        state.Items.Add(item);
        return item;
    }

    public void SetCurrentItem(PlaylistItem? item)
    {
        state.CurrentItem = item;
    }

    public void SkipNext()
    {
        ArgumentNullException.ThrowIfNull(state.CurrentItem);
        var index = state.Items.IndexOf(state.CurrentItem);
        var item = state.Items[(index + 1) % state.Items.Count];
        state.CurrentItem = item;
    }

    public void SkipPrevious()
    {
        ArgumentNullException.ThrowIfNull(state.CurrentItem);
        var index = state.Items.IndexOf(state.CurrentItem);
        var count = state.Items.Count;
        var item = state.Items[(index + count - 1) % state.Items.Count];
        state.CurrentItem = item;
    }

    public OneOf<PlaylistError, AudioError, Success> Play()
    {
        if (state.CurrentItem is null)
        {
            return new PlaylistError(PlaylistErrorKind.NoCurrentItem, "No current item to play");
        }
        return audioService
            .Play(state.CurrentItem.Path)
            .Match<OneOf<PlaylistError, AudioError, Success>>(a => a, a => a);
    }

    public void Stop()
    {
        audioService.Stop();
    }

    public void Clear()
    {
        state.Items.Clear();
    }
}
