using Avalonia;

namespace Sorairo.Common.UI;

public abstract class LifecycleViewBase : ViewBase
{
    private Action? cleanup;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        cleanup = OnVisualTreeAttached();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (cleanup is not null)
        {
            cleanup();
        }
    }

    protected abstract Action OnVisualTreeAttached();
}
