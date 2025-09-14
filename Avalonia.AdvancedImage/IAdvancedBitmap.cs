using Avalonia.Media.Imaging;
using Microsoft.IO;

namespace Avalonia.AdvancedImage;

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

    static sealed RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; set; } = new();

    static IAdvancedBitmap Load(Stream stream, bool disposeStream)
        => new SingleAdvancedBitmap(stream, disposeStream);

    static IAdvancedBitmap Load(IReadOnlyCollection<Stream> frameStreams, IReadOnlyCollection<int> delays, bool disposeStream)
        => new MultiAdvancedBitmap(frameStreams, delays, disposeStream);

    static IAdvancedBitmap Load(IReadOnlyCollection<Bitmap> bitmaps, IReadOnlyCollection<int> delays)
        => new AdvancedBitmapSimpleImpl(bitmaps, delays);
}
