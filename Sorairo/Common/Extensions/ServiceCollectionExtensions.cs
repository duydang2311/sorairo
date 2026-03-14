using Sorairo.Common.Interfaces;
using Sorairo.Common.Models;
using Sorairo.Common.Services;
using Sorairo.Features.MainMenu;
using Sorairo.Features.NowPlaying;
using Sorairo.Features.Playlist;
using Sorairo.Features.Shell;
using Sorairo.Features.StatusBar;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;

#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        services.AddSingleton<IAudioService, MiniAudioService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<AppState>();
        services.AddSingleton<PlaylistState>();
        services.AddSingleton<AudioState>();
        services.AddSingleton<FrameProviderContext>();
        return services;
    }

    public static IServiceCollection AddMainWindow(this IServiceCollection services)
    {
        services.AddTransient<ShellWindow>();
        services.AddTransient<ShellWindowViewModel>();
        return services;
    }

    public static IServiceCollection AddMainMenu(this IServiceCollection services)
    {
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<MainMenuView>();
        return services;
    }

    public static IServiceCollection AddPlaylist(this IServiceCollection services)
    {
        services.AddTransient<PlaylistViewModel>();
        services.AddTransient<PlaylistView>();
        return services;
    }

    public static IServiceCollection AddStatusBar(this IServiceCollection services)
    {
        services.AddTransient<StatusBarView>();
        services.AddTransient<PlaybackView>();
        services.AddTransient<PlaybackViewModel>();
        return services;
    }

    public static IServiceCollection AddNowPlaying(this IServiceCollection services)
    {
        services.AddTransient<NowPlayingView>();
        services.AddTransient<NowPlayingViewModel>();
        return services;
    }
}
