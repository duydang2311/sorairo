using Sorairo;
using Sorairo.Features.MainMenu;
using Sorairo.Infras.Audio;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfras(this IServiceCollection services)
    {
        services.AddSingleton<IAudioService, MiniAudioService>();
        return services;
    }

    public static IServiceCollection AddMainWindow(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        return services;
    }

    public static IServiceCollection AddMainMenu(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<MainMenuView>();
        return services;
    }
}
