using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Sorairo.Common.Services;

namespace Sorairo.Features.NowPlaying;

public class VisualizerView : Control
{
    public override void Render(DrawingContext context)
    {
        float barWidth = (float)(Bounds.Width / VisualizerService.BAR_COUNT);
        float maxHeight = (float)Bounds.Height;
        const float SCALE = 0.5f; // tune this
        const float riseSpeed = 0.4f;
        const float fallSpeed = 0.4f;

        for (int i = 0; i < VisualizerService.BAR_COUNT; i++)
        {
            float target = VisualizerService.targetBars[i];
            float current = VisualizerService.animatedBars[i];
            float speed = target > current ? riseSpeed : fallSpeed;

            VisualizerService.animatedBars[i] += (target - current) * speed;

            float barHeight = Math.Clamp(
                VisualizerService.animatedBars[i] * maxHeight * SCALE,
                0f,
                maxHeight
            );
            float x = i * barWidth;
            float y = maxHeight - barHeight;

            context.FillRectangle(Brushes.SkyBlue, new Rect(x + 2, y, barWidth - 4, barHeight), 4);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RequestNextFrame();
    }

    private void RequestNextFrame()
    {
        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnFrame);
    }

    private void OnFrame(TimeSpan time)
    {
        InvalidateVisual();
        RequestNextFrame();
    }
}
