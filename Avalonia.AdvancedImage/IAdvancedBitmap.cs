using Avalonia;
using Avalonia.Media.Imaging;

public interface IAdvancedBitmap
{
    bool IsInitialized { get; }

    bool IsFailed { get; }
    
    /// <summary>
    /// Gets the size of the image, in device independent pixels.
    /// </summary>
    Size Size { get; }
    
    int FrameCount { get; }

    IReadOnlyList<Bitmap> Frames { get; }

    IReadOnlyList<int> Delays { get; }

    Task InitAsync();
    
    event EventHandler? Initialized;
    
    event EventHandler<AdvancedBitmapFailedEventArgs>? Failed;
}

public class AdvancedBitmapFailedEventArgs(Exception exception) : EventArgs
{
    public Exception Exception { get; set; } = exception;
}
