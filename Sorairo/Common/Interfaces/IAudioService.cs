using OneOf;
using OneOf.Types;
using Sorairo.Common.Models;

namespace Sorairo.Common.Interfaces;

public interface IAudioService : IDisposable
{
    bool IsPlaying { get; }
    bool IsPaused { get; }

    OneOf<AudioError, Success> Play(Uri path);
    void Stop();
    void Pause();
    void Resume();
    TimeSpan GetTotalTime();
    TimeSpan GetElapsedTime();
    void Seek(TimeSpan time);
}
