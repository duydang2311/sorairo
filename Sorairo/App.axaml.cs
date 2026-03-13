using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sorairo.Features.Shell;

namespace Sorairo;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);

        var serviceProvider = new ServiceCollection()
            .AddCommon()
            .AddMainWindow()
            .AddMainMenu()
            .AddPlaylist()
            .AddStatusBar()
            .AddNowPlaying()
            .BuildServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = serviceProvider.GetRequiredService<ShellWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
