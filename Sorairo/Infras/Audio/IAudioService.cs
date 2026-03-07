namespace Sorairo.Infras.Audio;

public interface IAudioService : IDisposable
{
    void Play(Uri path);
}
