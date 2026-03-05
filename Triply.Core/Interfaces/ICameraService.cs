namespace Triply.Core.Interfaces;

public interface ICameraService
{
    Task<byte[]?> CapturePhotoAsync();
    Task<byte[]?> PickPhotoAsync();
    Task<byte[]?> CaptureOrPickPhotoAsync();
    Task<bool> CheckCameraPermissionAsync();
    Task<bool> CheckStoragePermissionAsync();
    Task<bool> RequestCameraPermissionAsync();
    Task<bool> RequestStoragePermissionAsync();
    Task<byte[]?> CompressImageAsync(byte[] imageBytes, int maxWidth = 1024, int maxHeight = 1024, int quality = 85);
}
