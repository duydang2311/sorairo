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

    public Models.Track AddTrack(Uri path)
    {
        var track = new ATL.Track(path.LocalPath);
        var item = new Models.Track
        {
            Id = Guid.NewGuid(),
            Path = path,
            Artist = track.Artist,
            Title = track.Title,
            Album = track.Album,
        };
        playlistState.Tracks.Add(item);
        return item;
    }

    public void SetCurrentTrack(Models.Track? item)
    {
        playlistState.CurrentTrack = item;
    }

    public void SkipNext() => Move(1);

    public void SkipPrevious() => Move(-1);

    public OneOf<PlaylistError, AudioError, Success> Play()
    {
        if (playlistState.CurrentTrack is null)
        {
            if (playlistState.Tracks.Count == 0)
            {
                return new PlaylistError(PlaylistErrorKind.EmptyPlaylist, "Playlist is empty");
            }
            if (playlistState.Shuffle.Mode == ShuffleMode.Shuffle)
            {
                var state = playlistState.Shuffle.State;
                Guard.Against.Null(state);
                var id = state.Ids[0];
                var item = playlistState.Tracks.Find(a => a.Id == id);
                Guard.Against.Null(item);
                playlistState.CurrentTrack = item;
            }
            else
            {
                playlistState.CurrentTrack = playlistState.Tracks[0];
            }
        }
        return audioService
            .Play(playlistState.CurrentTrack.Path)
            .Match<OneOf<PlaylistError, AudioError, Success>>(a => a, a => a);
    }

    public void Stop()
    {
        audioService.Stop();
    }

    public void Clear()
    {
        playlistState.Tracks.Clear();
    }

    public void Dispose()
    {
        audioService.SoundEnded -= OnSoundEnded;
    }

    private void OnSoundEnded()
    {
        if (playlistState.Shuffle.Mode == ShuffleMode.Shuffle)
        {
            switch (playlistState.RepeatMode)
            {
                case RepeatMode.None:
                    var state = playlistState.Shuffle.State;
                    Guard.Against.Null(state);
                    var currentId = state.CurrentId;
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
        switch (playlistState.RepeatMode)
        {
            case RepeatMode.None:
                Guard.Against.Null(playlistState.CurrentTrack);
                var index = playlistState.Tracks.IndexOf(playlistState.CurrentTrack);
                if (index == playlistState.Tracks.Count - 1)
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
        switch (playlistState.Shuffle.Mode)
        {
            case ShuffleMode.None:
                playlistState.Shuffle.Mode = ShuffleMode.Shuffle;
                var ids = playlistState.Tracks.Select(a => a.Id).ToList();
                for (int i = ids.Count - 1; i > 0; --i)
                {
                    int j = Random.Shared.Next(i + 1);
                    (ids[i], ids[j]) = (ids[j], ids[i]);
                }
                if (playlistState.CurrentTrack is not null)
                {
                    var index = ids.FindIndex(a => a == playlistState.CurrentTrack.Id);
                    (ids[0], ids[index]) = (ids[index], ids[0]);
                }
                playlistState.Shuffle.State = new ShuffleState(ids);
                break;
            case ShuffleMode.Shuffle:
                playlistState.Shuffle.Mode = ShuffleMode.None;
                playlistState.Shuffle.State = null;
                break;
        }
    }

    public void ToggleRepeatMode()
    {
        playlistState.RepeatMode = playlistState.RepeatMode switch
        {
            RepeatMode.None => RepeatMode.All,
            RepeatMode.All => RepeatMode.One,
            RepeatMode.One => RepeatMode.None,
            _ => throw new ArgumentOutOfRangeException(nameof(playlistState.RepeatMode)),
        };
    }

    private void Move(int delta)
    {
        if (playlistState.Shuffle.Mode == ShuffleMode.Shuffle)
        {
            var state = Guard.Against.Null(playlistState.Shuffle.State);
            var currentId = Guard.Against.Null(state.CurrentId);

            var index = state.Ids.FindIndex(id => id == currentId);
            var count = state.Ids.Count;

            var nextId = state.Ids[(index + delta + count) % count];
            var item = Guard.Against.Null(playlistState.Tracks.Find(a => a.Id == nextId));

            state.CurrentId = nextId;
            playlistState.CurrentTrack = item;
        }
        else
        {
            var current = Guard.Against.Null(playlistState.CurrentTrack);
            var index = playlistState.Tracks.IndexOf(current);
            var count = playlistState.Tracks.Count;

            playlistState.CurrentTrack = playlistState.Tracks[(index + delta + count) % count];
        }
    }
}
