namespace TasksCM;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionToolkit()
            .ConfigureMauiHandlers(handlers =>
            {
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });

#if DEBUG
		builder.Logging.AddDebug();
		builder.Services.AddLogging(configure => configure.AddDebug());
#endif


        //Register Desktop pages here as singleton services
        builder.Services.AddSingleton<HomePageD>();
        builder.Services.AddSingleton<UpSertSingleTaskD>();        
        builder.Services.AddSingleton<TasksCMWindow>();

        //Register mobile pages here as singleton services
        builder.Services.AddSingleton<HomePageM>();
        builder.Services.AddSingleton<UpSertSingleTaskM>();
        return builder.Build();
    }
}
