using Avalonia.Controls;

namespace Sorairo.Common.UI;

public abstract class ViewBase : UserControl
{
    public ViewBase()
    {
        Init();
    }

    protected abstract void Init();
}
