using Triply.Core.Interfaces;

namespace Triply.Services;

public class LoadingStateService : ILoadingStateService
{
    private int _loadingCount;
    private string? _loadingMessage;

    public bool IsLoading => _loadingCount > 0;
    public string? LoadingMessage => _loadingMessage;

    public event EventHandler? LoadingStateChanged;

    public void ShowLoading(string? message = null)
    {
        _loadingCount++;
        _loadingMessage = message;
        LoadingStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void HideLoading()
    {
        if (_loadingCount > 0)
        {
            _loadingCount--;
            if (_loadingCount == 0)
            {
                _loadingMessage = null;
            }
            LoadingStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
