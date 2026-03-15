using Avalonia;
using Avalonia.Controls;
using R3;

namespace Sorairo.Common.UI;

public abstract class ViewBase : UserControl
{
    public ViewBase()
    {
        Init();
    }

    protected abstract void Init();
}

public abstract class ActivatableView : ViewBase
{
    private DisposableBag disposables;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OnActivated(ref disposables);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        disposables.Dispose();
        disposables = default;
    }

    protected abstract void OnActivated(ref DisposableBag disposables);
}
