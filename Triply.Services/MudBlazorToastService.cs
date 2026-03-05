using MudBlazor;
using Triply.Core.Interfaces;

namespace Triply.Services;

public class MudBlazorToastService : IToastService
{
    private readonly ISnackbar _snackbar;

    public MudBlazorToastService(ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }

    public void ShowSuccess(string message)
    {
        _snackbar.Add(message, Severity.Success, config =>
        {
            config.VisibleStateDuration = 3000;
            config.ShowCloseIcon = true;
        });
    }

    public void ShowError(string message)
    {
        _snackbar.Add(message, Severity.Error, config =>
        {
            config.VisibleStateDuration = 5000;
            config.ShowCloseIcon = true;
        });
    }

    public void ShowWarning(string message)
    {
        _snackbar.Add(message, Severity.Warning, config =>
        {
            config.VisibleStateDuration = 4000;
            config.ShowCloseIcon = true;
        });
    }

    public void ShowInfo(string message)
    {
        _snackbar.Add(message, Severity.Info, config =>
        {
            config.VisibleStateDuration = 3000;
            config.ShowCloseIcon = true;
        });
    }
}
