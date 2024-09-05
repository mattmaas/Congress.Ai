using Microsoft.Extensions.Logging;

namespace App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<Microsoft.Maui.Controls.Image, CustomImageHandler>();
            });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    public class CustomImageHandler : Microsoft.Maui.Handlers.ImageHandler
    {
#if IOS || MACCATALYST
        protected override void ConnectHandler(Microsoft.Maui.Platform.PlatformView platformView)
        {
            base.ConnectHandler(platformView);
            if (platformView is UIKit.UIImageView imageView)
            {
                imageView.ClipsToBounds = true;
            }
        }
#elif ANDROID
        protected override void ConnectHandler(Microsoft.Maui.Platform.ContentViewGroup platformView)
        {
            base.ConnectHandler(platformView);
            platformView.ClipsToBounds = true;
        }
#elif WINDOWS
        protected override void ConnectHandler(Microsoft.Maui.Platform.PlatformView platformView)
        {
            base.ConnectHandler(platformView);
            if (platformView is Microsoft.UI.Xaml.Controls.Image image)
            {
                image.Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill;
            }
        }
#endif
    }
}
