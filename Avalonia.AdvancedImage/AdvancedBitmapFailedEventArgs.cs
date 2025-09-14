namespace Avalonia.AdvancedImage;

public class AdvancedBitmapFailedEventArgs(Exception exception) : EventArgs
{
    public Exception Exception { get; set; } = exception;
}
