using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;

namespace Phone
{
    public static class MauiProgram
    {
        
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCameraView()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("icomoon.ttf", "CustomFontIcons");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                    events.AddAndroid(android =>
                    {
                        android.OnPause(act =>
                        {
                            MainPage.Instance.OnAppPause();
                        }).OnResume(act =>
                        {
                            MainPage.Instance.OnAppResume();
                        });
                    });
#endif
                })
                ;

            return builder.Build();
        }
    }
}