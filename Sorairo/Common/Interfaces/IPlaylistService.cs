using OneOf;
using OneOf.Types;
using Sorairo.Common.Models;

namespace Sorairo.Common.Interfaces;

public interface IPlaylistService : IDisposable
{
    Track AddTrack(Uri path);
    void SetCurrentTrack(Track? item);
    void SkipNext();
    void SkipPrevious();
    OneOf<PlaylistError, AudioError, Success> Play();
    void Stop();
    void Clear();
    void ToggleShuffleMode();
    void ToggleRepeatMode();
}
