namespace Triply.Core.Interfaces;

public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<bool> ConnectivityChanged;
    void StartMonitoring();
    void StopMonitoring();
}
