using R3.Avalonia;

namespace Sorairo.Common.Models;

public sealed record FrameProviderContext
{
    public AvaloniaRenderingFrameProvider FrameProvider { get; private set; } = null!;

    public void Initialize(AvaloniaRenderingFrameProvider frameProvider)
    {
        FrameProvider = frameProvider;
    }
}
