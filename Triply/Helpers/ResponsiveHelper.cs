using MudBlazor;

namespace Triply.Helpers;

/// <summary>
/// Helper methods for responsive design patterns
/// </summary>
public static class ResponsiveHelper
{
    /// <summary>
    /// Get appropriate MudBlazor size based on context
    /// </summary>
    public static MudBlazor.Size GetResponsiveSize(bool isMobile)
    {
        return isMobile ? MudBlazor.Size.Small : MudBlazor.Size.Medium;
    }

    /// <summary>
    /// Get appropriate card elevation based on device
    /// </summary>
    public static int GetCardElevation(bool isMobile)
    {
        return isMobile ? 2 : 3;
    }

    /// <summary>
    /// Get grid item sizes for responsive layouts
    /// </summary>
    public static (int xs, int sm, int md, int lg) GetGridItemSizes(ResponsiveColumns columns)
    {
        return columns switch
        {
            ResponsiveColumns.Single => (12, 12, 12, 12),
            ResponsiveColumns.TwoColumn => (12, 6, 6, 4),
            ResponsiveColumns.ThreeColumn => (12, 6, 4, 3),
            ResponsiveColumns.FourColumn => (12, 6, 3, 3),
            _ => (12, 12, 12, 12)
        };
    }

    /// <summary>
    /// Determine if device is mobile based on breakpoint
    /// </summary>
    public static bool IsMobile(Breakpoint breakpoint)
    {
        return breakpoint <= Breakpoint.Sm;
    }

    /// <summary>
    /// Determine if device is tablet
    /// </summary>
    public static bool IsTablet(Breakpoint breakpoint)
    {
        return breakpoint == Breakpoint.Md;
    }

    /// <summary>
    /// Determine if device is desktop
    /// </summary>
    public static bool IsDesktop(Breakpoint breakpoint)
    {
        return breakpoint >= Breakpoint.Lg;
    }
}

public enum ResponsiveColumns
{
    Single,
    TwoColumn,
    ThreeColumn,
    FourColumn
}
