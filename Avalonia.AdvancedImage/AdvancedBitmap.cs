using System.Diagnostics.CodeAnalysis;
using Avalonia.Media.Imaging;
using Microsoft.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Avalonia.AdvancedImage;

public class AdvancedBitmap(Stream stream, bool disposeStream) : IAdvancedBitmap
{
    public bool IsInitialized { get => !IsFailed && field; private set; }

    public bool IsFailed { get; private set; }

    public Size Size { get; private set; }
    
    public int FrameCount { get; private set; }

    [field: MaybeNull, AllowNull]
    public IReadOnlyList<Bitmap> Frames
    {
        get => field ?? throw new InvalidOperationException();
        private set;
    }

    public IReadOnlyList<int> Delays { get; private set; } = [];

    public event EventHandler? Initialized;
    
    public event EventHandler<AdvancedBitmapFailedEventArgs>? Failed;
    
    private readonly Stream? _stream = stream ?? throw new ArgumentNullException(nameof(stream));

    public async Task InitAsync()
    {
        if (IsInitialized || IsFailed)
            return;
        try
        {
            if (_stream is null)
                throw new NullReferenceException(nameof(_stream));
            using var image = await Image.LoadAsync<Rgba32>(_stream);
            var delays = new int[image.Frames.Count];
            var frames = new Bitmap[image.Frames.Count];
            var index = 0;

            while (image.Frames.Count is not 1)
            {
                var exportFrame = image.Frames.ExportFrame(0);
                (frames[index], delays[index]) = await GetBitmapAndDelayAsync(exportFrame);
                exportFrame.Dispose();
                index++;
            }
            (frames[index], delays[index]) = await GetBitmapAndDelayAsync(image);

            Size = new Size(image.Size.Width, image.Size.Height);
            FrameCount = image.Frames.Count;
            Delays = delays;
            Frames = frames;
            IsInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            IsFailed = true;
            Failed?.Invoke(this, new AdvancedBitmapFailedEventArgs(e));
        }
        finally
        {
            if (_stream is not null && disposeStream)
                await _stream.DisposeAsync();
        }

        return;

        async Task<(Bitmap bitmap, int delay)> GetBitmapAndDelayAsync(Image frame)
        {
            var delay = 10;
            if (frame.Frames.RootFrame.Metadata.TryGetGifMetadata(out var gifFrameMetadata))
                delay = gifFrameMetadata.FrameDelay * 10;
            else if (frame.Frames.RootFrame.Metadata.TryGetPngMetadata(out var pngFrameMetadata))
                delay = (int) (pngFrameMetadata.FrameDelay.ToDouble() * 10);
            else if (frame.Frames.RootFrame.Metadata.TryGetWebpFrameMetadata(out var webpFrameMetadata))
                delay = (int) webpFrameMetadata.FrameDelay;

            await using var ms = _RecyclableMemoryStreamManager.GetStream();
            await frame.SaveAsBmpAsync(ms);
            ms.Position = 0;
            var bitmap = new Bitmap(ms);
            return (bitmap, delay);
        }
    }

    private static readonly RecyclableMemoryStreamManager _RecyclableMemoryStreamManager = new();
}
