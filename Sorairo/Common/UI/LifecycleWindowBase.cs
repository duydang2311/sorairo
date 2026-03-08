namespace Sorairo.Common.UI;

public abstract class LifecycleWindowBase : WindowBase
{
    private Action? cleanup;

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        cleanup = WhenOpened();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (cleanup is not null)
        {
            cleanup();
        }
    }

    protected abstract Action WhenOpened();
}
