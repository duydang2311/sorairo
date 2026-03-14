using OneOf;
using OneOf.Types;
using Sorairo.Common.Models;

namespace Sorairo.Common.Interfaces;

public interface IPlaylistService : IDisposable
{
    PlaylistItem AddItem(Uri path);
    void SetCurrentItem(PlaylistItem? item);
    void SkipNext();
    void SkipPrevious();
    OneOf<PlaylistError, AudioError, Success> Play();
    void Stop();
    void Clear();
    void ToggleShuffleMode();
    void ToggleRepeatMode();
}
