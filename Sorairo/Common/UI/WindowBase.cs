using Avalonia.Controls;

namespace Sorairo.Common.UI;

public abstract class WindowBase : Window
{
    public WindowBase()
    {
        Init();
    }

    protected abstract void Init();
}
