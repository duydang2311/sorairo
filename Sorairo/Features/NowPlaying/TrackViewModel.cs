using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.NowPlaying;

public sealed partial class TrackViewModel(Track track) : ActivatableViewModel
{
    [ObservableProperty]
    private Track track = track;

    [ObservableProperty]
    private Stretch frontCoverStretch = Stretch.UniformToFill;

    [ObservableProperty]
    private Bitmap? frontCoverImage;

    protected override void Init() { }

    protected override void OnActivated(ref DisposableBag disposables)
    {
        this.ObservePropertyChanged(vm => vm.Track)
            .Select(track => track.GetFrontCover())
            .Subscribe(frontCover =>
            {
                FrontCoverImage?.Dispose();
                if (frontCover is not null)
                {
                    using var ms = new MemoryStream(frontCover);
                    FrontCoverImage = new Bitmap(ms);
                }
                else
                {
                    FrontCoverImage = null;
                }
            })
            .AddTo(ref disposables);
        disposables.Add(
            Disposable.Create(
                this,
                static state =>
                {
                    if (state.FrontCoverImage is not null)
                    {
                        state.FrontCoverImage.Dispose();
                        state.FrontCoverImage = null;
                    }
                }
            )
        );
    }

    [RelayCommand]
    private void ToggleFrontCoverStretch()
    {
        FrontCoverStretch = FrontCoverStretch switch
        {
            Stretch.UniformToFill => Stretch.Uniform,
            _ => Stretch.UniformToFill,
        };
    }
}
