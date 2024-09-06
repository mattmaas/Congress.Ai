using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;

namespace App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCompatibility() // Add this line
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
        protected override void ConnectHandler(UIKit.UIImageView platformView)
        {
            base.ConnectHandler(platformView);
            platformView.ClipsToBounds = true;
        }
#elif ANDROID
        protected override void ConnectHandler(Android.Widget.ImageView platformView)
        {
            base.ConnectHandler(platformView);
            platformView.ClipToOutline = true;
        }
#elif WINDOWS
        protected override void ConnectHandler(Microsoft.UI.Xaml.Controls.Image platformView)
        {
            base.ConnectHandler(platformView);
            platformView.Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill;
        }
#endif
    }
}
