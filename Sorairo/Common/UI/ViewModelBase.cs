using CommunityToolkit.Mvvm.ComponentModel;

namespace Sorairo.Common.UI;

public abstract class ViewModelBase : ObservableObject
{
    public ViewModelBase()
    {
        Init();
    }

    protected abstract void Init();
}
