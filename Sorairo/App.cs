using Ardalis.GuardClauses;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Simple;
using Microsoft.Extensions.DependencyInjection;

namespace Sorairo;

public partial class App : Application
{
    private IServiceProvider? serviceProvider;

    public override void Initialize()
    {
        serviceProvider = new ServiceCollection()
            .AddInfras()
            .AddMainWindow()
            .AddMainMenu()
            .BuildServiceProvider();

        RequestedThemeVariant = ThemeVariant.Default;
        Styles.Add(new SimpleTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Guard.Against.Null(serviceProvider);
            desktop.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
