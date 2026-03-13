using Avalonia.Controls;

namespace Sorairo.Common.UI;

public abstract class InitWindowBase : Window
{
    public InitWindowBase()
    {
        Init();
    }

    protected abstract void Init();
}
