using System.Diagnostics.CodeAnalysis;
using Avalonia.Media.Imaging;

namespace Avalonia.AnimatedImage;

internal class AnimatedBitmapSimpleImpl : IAnimatedBitmap
{
    public AnimatedBitmapSimpleImpl(IReadOnlyCollection<Bitmap> bitmaps, IReadOnlyCollection<int> delays)
    {
        ArgumentNullException.ThrowIfNull(bitmaps);
        ArgumentNullException.ThrowIfNull(delays);
        if (bitmaps.Count is var bitmapCount && delays.Count != bitmapCount)
            throw new ArgumentException($"{nameof(delays)} inconsistent count with {nameof(bitmaps)}");
        if ((IReadOnlyList<Bitmap>) [..bitmaps] is not [var first, ..] bitmapsCopy)
            throw new ArgumentException($"Invalid {nameof(delays)}.Count");
        Size = first.Size;
        Frames = bitmapsCopy;
        Delays = [..delays];
        FrameCount = bitmapCount;
    }

    public bool IsInitialized => true;

    public bool IsFailed => false;

    public Size Size { get; } 

    public int FrameCount { get; }

    [field: MaybeNull, AllowNull]
    public IReadOnlyList<Bitmap> Frames { get; }

    public IReadOnlyList<int> Delays { get; }

    public event EventHandler? Initialized;
    
    public event EventHandler<AnimatedBitmapFailedEventArgs>? Failed;
    
    public Task InitAsync() => Task.CompletedTask;
}
