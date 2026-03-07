using Triply.Core.Interfaces;
using System.Linq;

namespace Triply.Services;

public class CameraService : ICameraService
{
    public async Task<byte[]?> CapturePhotoAsync()
    {
        try
        {
            // Check permissions first
            var hasPermission = await CheckCameraPermissionAsync();
            if (!hasPermission)
            {
                hasPermission = await RequestCameraPermissionAsync();
                if (!hasPermission)
                    return null;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null)
                return null;

            return await ReadPhotoAsync(photo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing photo: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> PickPhotoAsync()
    {
        try
        {
            // Check storage permissions
            var hasPermission = await CheckStoragePermissionAsync();
            if (!hasPermission)
            {
                hasPermission = await RequestStoragePermissionAsync();
                if (!hasPermission)
                    return null;
            }

            var photos = await MediaPicker.Default.PickPhotosAsync();
            var photo = photos?.FirstOrDefault();
            if (photo == null)
                return null;

            return await ReadPhotoAsync(photo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error picking photo: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> CaptureOrPickPhotoAsync()
    {
        try
        {
            // On mobile, show choice dialog
            if (DeviceInfo.Current.Platform == DevicePlatform.Android || 
                DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page == null)
                    return await PickPhotoAsync();

                var action = await page.DisplayActionSheetAsync(
                    "Add Photo",
                    "Cancel",
                    null,
                    "Take Photo",
                    "Choose from Gallery");

                return action switch
                {
                    "Take Photo" => await CapturePhotoAsync(),
                    "Choose from Gallery" => await PickPhotoAsync(),
                    _ => null
                };
            }
            else
            {
                // On desktop, just pick from files
                return await PickPhotoAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CaptureOrPickPhotoAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CheckCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
    }

    public async Task<bool> CheckStoragePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        return status == PermissionStatus.Granted;
    }

    public async Task<bool> RequestCameraPermissionAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
    }

    public async Task<bool> RequestStoragePermissionAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.StorageRead>();
        return status == PermissionStatus.Granted;
    }

    public async Task<byte[]?> CompressImageAsync(byte[] imageBytes, int maxWidth = 1024, int maxHeight = 1024, int quality = 85)
    {
        try
        {
            // Load image
            using var stream = new MemoryStream(imageBytes);
            using var image = await Task.Run(() => SkiaSharp.SKBitmap.Decode(stream));
            
            if (image == null)
                return imageBytes;

            // Calculate new dimensions while maintaining aspect ratio
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            // Only resize if image is larger than max dimensions
            if (ratio >= 1)
                return imageBytes;

            // Resize image
            var sampling = new SkiaSharp.SKSamplingOptions(SkiaSharp.SKFilterMode.Linear, SkiaSharp.SKMipmapMode.Linear);
            using var resizedImage = image.Resize(new SkiaSharp.SKImageInfo(newWidth, newHeight), sampling);
            if (resizedImage == null)
                return imageBytes;

            // Encode to JPEG with quality
            using var outputStream = new MemoryStream();
            resizedImage.Encode(outputStream, SkiaSharp.SKEncodedImageFormat.Jpeg, quality);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error compressing image: {ex.Message}");
            return imageBytes;
        }
    }

    private async Task<byte[]?> ReadPhotoAsync(FileResult photo)
    {
        try
        {
            using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            
            var imageBytes = memoryStream.ToArray();
            
            // Compress the image to reduce storage size
            return await CompressImageAsync(imageBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading photo: {ex.Message}");
            return null;
        }
    }
}
