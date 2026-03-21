using Avalonia;
using R3;

namespace Sorairo.Common.UI;

public abstract class ActivatableView : ViewBase
{
    private DisposableBag disposables;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        OnActivated(ref disposables);
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        disposables.Dispose();
        disposables = default;
        base.OnDetachedFromVisualTree(e);
    }

    protected abstract void OnActivated(ref DisposableBag disposables);
}
