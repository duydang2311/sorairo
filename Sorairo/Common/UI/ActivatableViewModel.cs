using R3;

namespace Sorairo.Common.UI;

public abstract class ActivatableViewModel : ViewModelBase
{
    public void Activate(ref DisposableBag disposables)
    {
        OnActivated(ref disposables);
    }

    protected abstract void OnActivated(ref DisposableBag disposables);
}
