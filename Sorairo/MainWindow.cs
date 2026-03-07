using Avalonia;
using Avalonia.Controls;
using Sorairo.Features.MainMenu;

namespace Sorairo;

public partial class MainWindow : Window
{
    private readonly MainMenuView mainMenuView;

    public MainWindow(MainMenuView mainMenuView)
    {
        this.mainMenuView = mainMenuView;

        Width = 640;
        Height = 480;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaChromeHints = Avalonia
            .Platform
            .ExtendClientAreaChromeHints
            .PreferSystemChrome;
        Content = CreateContent();
    }

    private DockPanel CreateContent()
    {
        return new()
        {
            Children =
            {
                new StackPanel
                {
                    Children =
                    {
                        new Border
                        {
                            Height = 32,
                            Padding = new Thickness(8, 4),
                            BorderThickness = new Thickness(0, 0, 0, 1),
                            Child = new Label { Content = "Sorairo" },
                        }.BindDynamicResource(BorderBrushProperty, "ThemeBorderLowBrush"),
                        mainMenuView,
                    },
                }.Dock(Dock.Top),
                new Panel(),
            },
        };
    }
}
