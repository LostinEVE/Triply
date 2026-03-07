using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;

namespace Triply
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, 
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Window == null)
                return;

            var sdkInt = (int)Build.VERSION.SdkInt;

            // Edge-to-edge and inset controller APIs require Android 11+.
#pragma warning disable CA1416, CA1422
            if (sdkInt >= 30)
            {
                Window.SetDecorFitsSystemWindows(false);
                Window.InsetsController?.SetSystemBarsAppearance(
                    (int)WindowInsetsControllerAppearance.LightStatusBars,
                    (int)WindowInsetsControllerAppearance.LightStatusBars);
            }

            // Color APIs are obsolete on Android 35+; keep compatibility for older API levels.
            if (sdkInt < 35)
            {
                Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                Window.SetNavigationBarColor(Android.Graphics.Color.Transparent);
            }
#pragma warning restore CA1416, CA1422
        }
    }
}
