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

            // Enable edge-to-edge but respect system bars
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                Window?.SetDecorFitsSystemWindows(false);
            }

            // Make status bar and navigation bar transparent
            if (Window != null)
            {
                Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                Window.SetNavigationBarColor(Android.Graphics.Color.Transparent);

                // Light status bar icons (dark icons on light background)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    Window.InsetsController?.SetSystemBarsAppearance(
                        (int)WindowInsetsControllerAppearance.LightStatusBars,
                        (int)WindowInsetsControllerAppearance.LightStatusBars);
                }
            }
        }
    }
}
