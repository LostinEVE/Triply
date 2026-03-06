namespace Triply.Core.Interfaces;

public interface ILoadingStateService
{
    bool IsLoading { get; }
    string? LoadingMessage { get; }
    void ShowLoading(string? message = null);
    void HideLoading();
    event EventHandler? LoadingStateChanged;
}
