namespace Avalonia.AnimatedImage;

public class AnimatedBitmapFailedEventArgs(Exception exception) : EventArgs
{
    public Exception Exception { get; set; } = exception;
}
