using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Sorairo.Features.NowPlaying;

public sealed class TrackProgressBar : Control
{
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<
        TrackProgressBar,
        double
    >(nameof(Value));

    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<
        TrackProgressBar,
        double
    >(nameof(Minimum), 0);

    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<
        TrackProgressBar,
        double
    >(nameof(Maximum), 100);

    public static readonly StyledProperty<IBrush?> BackgroundProperty = AvaloniaProperty.Register<
        TrackProgressBar,
        IBrush?
    >(nameof(Background));

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    static TrackProgressBar()
    {
        AffectsRender<TrackProgressBar>(MinimumProperty);
        AffectsRender<TrackProgressBar>(ValueProperty);
        AffectsRender<TrackProgressBar>(MaximumProperty);
        AffectsRender<TrackProgressBar>(BackgroundProperty);
    }

    private IBrush? _immutableBackground;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs changed)
    {
        base.OnPropertyChanged(changed);

        if (changed.Property == BackgroundProperty)
        {
            if (Background is ISolidColorBrush solid)
            {
                _immutableBackground = new ImmutableSolidColorBrush(solid.Color, solid.Opacity);
            }
            else
            {
                _immutableBackground = default;
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        if (_immutableBackground is not null)
        {
            var range = Maximum - Minimum;
            var percent = Math.Clamp((Value - Minimum) / range, 0, 1);
            context.FillRectangle(_immutableBackground, Bounds.WithWidth(percent * Bounds.Width));
        }
    }
}
