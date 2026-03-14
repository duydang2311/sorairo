using Avalonia;
using Avalonia.Controls;
using R3;

namespace Sorairo.Common.UI;

public abstract class ViewBase : UserControl
{
    private DisposableBag disposables;

    public ViewBase()
    {
        Init();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OnActivated(ref disposables);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        disposables.Dispose();
    }

    protected abstract void Init();
    protected abstract void OnActivated(ref DisposableBag disposables);
}
