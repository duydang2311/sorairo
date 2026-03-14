using Ardalis.GuardClauses;
using ATL;
using OneOf;
using OneOf.Types;
using R3;
using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;

namespace Sorairo.Common.Services;

public sealed class PlaylistService : IPlaylistService
{
    private readonly PlaylistState playlistState;
    private readonly IAudioService audioService;

    public PlaylistService(PlaylistState playlistState, IAudioService audioService)
    {
        this.playlistState = playlistState;
        this.audioService = audioService;
        audioService.SoundEnded += OnSoundEnded;
    }

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
        };
        playlistState.Items.Add(item);
        return item;
    }

    public void SetCurrentItem(PlaylistItem? item)
    {
        playlistState.CurrentItem = item;
    }

    public void SkipNext() => Move(1);

    public void SkipPrevious() => Move(-1);

    public OneOf<PlaylistError, AudioError, Success> Play()
    {
        if (playlistState.CurrentItem is null)
        {
            if (playlistState.Items.Count == 0)
            {
                return new PlaylistError(PlaylistErrorKind.EmptyPlaylist, "Playlist is empty");
            }
            if (playlistState.Shuffle.Mode.Value == ShuffleMode.Shuffle)
            {
                var state = playlistState.Shuffle.State.Value;
                Guard.Against.Null(state);
                var id = state.Ids[0];
                var item = playlistState.Items.Find(a => a.Id == id);
                Guard.Against.Null(item);
                playlistState.CurrentItem = item;
            }
            else
            {
                playlistState.CurrentItem = playlistState.Items[0];
            }
        }
        return audioService
            .Play(playlistState.CurrentItem.Path)
            .Match<OneOf<PlaylistError, AudioError, Success>>(a => a, a => a);
    }

    public void Stop()
    {
        audioService.Stop();
    }

    public void Clear()
    {
        playlistState.Items.Clear();
    }

    public void Dispose()
    {
        audioService.SoundEnded -= OnSoundEnded;
    }

    private void OnSoundEnded()
    {
        if (playlistState.Shuffle.Mode.Value == ShuffleMode.Shuffle)
        {
            switch (playlistState.RepeatMode.CurrentValue)
            {
                case RepeatMode.None:
                    var state = playlistState.Shuffle.State.Value;
                    Guard.Against.Null(state);
                    var currentId = state.CurrentId.Value;
                    Guard.Against.Null(currentId);
                    var index = state.Ids.FindIndex(id => id == currentId);
                    if (index == state.Ids.Count - 1)
                    {
                        break;
                    }
                    SkipNext();
                    Play();
                    break;
                case RepeatMode.One:
                    Play();
                    break;
                case RepeatMode.All:
                    SkipNext();
                    Play();
                    break;
            }
            return;
        }
        switch (playlistState.RepeatMode.CurrentValue)
        {
            case RepeatMode.None:
                Guard.Against.Null(playlistState.CurrentItem);
                var index = playlistState.Items.IndexOf(playlistState.CurrentItem);
                if (index == playlistState.Items.Count - 1)
                {
                    break;
                }
                SkipNext();
                Play();
                break;
            case RepeatMode.One:
                Play();
                break;
            case RepeatMode.All:
                SkipNext();
                Play();
                break;
        }
    }

    public void ToggleShuffleMode()
    {
        switch (playlistState.Shuffle.Mode.Value)
        {
            case ShuffleMode.None:
                playlistState.Shuffle.Mode.Value = ShuffleMode.Shuffle;
                var ids = playlistState.Items.Select(a => a.Id).ToList();
                for (int i = ids.Count - 1; i > 0; --i)
                {
                    int j = Random.Shared.Next(i + 1);
                    (ids[i], ids[j]) = (ids[j], ids[i]);
                }
                if (playlistState.CurrentItem is not null)
                {
                    var index = ids.FindIndex(a => a == playlistState.CurrentItem.Id);
                    (ids[0], ids[index]) = (ids[index], ids[0]);
                }
                playlistState.Shuffle.State.Value = new ShuffleState(ids);
                break;
            case ShuffleMode.Shuffle:
                playlistState.Shuffle.Mode.Value = ShuffleMode.None;
                playlistState.Shuffle.State.Value = null;
                break;
        }
    }

    public void ToggleRepeatMode()
    {
        playlistState.RepeatMode.Value = playlistState.RepeatMode.Value switch
        {
            RepeatMode.None => RepeatMode.All,
            RepeatMode.All => RepeatMode.One,
            RepeatMode.One => RepeatMode.None,
            _ => throw new ArgumentOutOfRangeException(nameof(playlistState.RepeatMode)),
        };
    }

    private void Move(int delta)
    {
        if (playlistState.Shuffle.Mode.Value == ShuffleMode.Shuffle)
        {
            var state = Guard.Against.Null(playlistState.Shuffle.State.Value);
            var currentId = Guard.Against.Null(state.CurrentId.Value);

            var index = state.Ids.FindIndex(id => id == currentId);
            var count = state.Ids.Count;

            var nextId = state.Ids[(index + delta + count) % count];
            var item = Guard.Against.Null(playlistState.Items.Find(a => a.Id == nextId));

            state.CurrentId.Value = nextId;
            playlistState.CurrentItem = item;
        }
        else
        {
            var current = Guard.Against.Null(playlistState.CurrentItem);
            var index = playlistState.Items.IndexOf(current);
            var count = playlistState.Items.Count;

            playlistState.CurrentItem = playlistState.Items[(index + delta + count) % count];
        }
    }
}
