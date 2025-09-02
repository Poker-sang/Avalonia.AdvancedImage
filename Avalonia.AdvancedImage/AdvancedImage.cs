using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Rendering.Composition;
using System.Numerics;

namespace Avalonia.AdvancedImage;

public partial class AdvancedImage : Control
{
    private CompositionCustomVisual? _customVisual;

    public static readonly StyledProperty<IAdvancedBitmap?> SourceProperty = AvaloniaProperty.Register<AdvancedImage, IAdvancedBitmap?>(name: nameof(Source), defaultValue: null);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty = AvaloniaProperty.Register<AdvancedImage, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<AdvancedImage, Stretch>(nameof(Stretch), Stretch.UniformToFill);

    [Content]
    public IAdvancedBitmap? Source
    {
        get => GetValue(SourceProperty); 
        set => SetValue(SourceProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty); 
        set => SetValue(StretchProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        switch (change.Property.Name)
        {
            case nameof(Source):
                OnSourcePropertyChanged(change.NewValue as IAdvancedBitmap);
                break;
            case nameof(Stretch):
            case nameof(StretchDirection):
                InvalidateArrange();
                InvalidateMeasure();
                Update();
                break;
            case nameof(Bounds):
                Update();
                break;
        }

        base.OnPropertyChanged(change);
    }
    
    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var compositor = ElementComposition.GetElementVisual(this)?.Compositor;
        if (compositor is null || _customVisual?.Compositor == compositor)
            return;
        _customVisual = compositor.CreateCustomVisual(new CustomVisualHandler());
        ElementComposition.SetElementChildVisual(this, _customVisual);
        _customVisual.SendHandlerMessage(CustomVisualHandler.StartMessage);

        if (Source is { IsInitialized: false, IsFailed: false })
            await Source.InitAsync();
        if (Source is { IsInitialized: true })
            _customVisual?.SendHandlerMessage(Source);

        Update();
        base.OnAttachedToVisualTree(e);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        return Source is { IsInitialized: true }
            ? Stretch.CalculateSize(availableSize, Source.Size, StretchDirection)
            : default;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        return Source is { IsInitialized: true }
            ? Stretch.CalculateSize(finalSize, Source.Size)
            : default;
    }

    private async void OnSourcePropertyChanged(IAdvancedBitmap? newValue)
    {
        if (_customVisual is null)
            return;

        if (newValue is null)
            _customVisual.SendHandlerMessage(CustomVisualHandler.ResetMessage);
        else
        {
            if (Source is { IsInitialized: false, IsFailed: false })
                await Source.InitAsync();
            if (Source is { IsInitialized: true })
                _customVisual.SendHandlerMessage(newValue);
        }

        InvalidateArrange();
        InvalidateMeasure();
        Update();
    }

    private void Update()
    {
        if (_customVisual is null || Source is null)
            return;

        var sourceSize = Source.Size;
        var viewPort = new Rect(Bounds.Size);

        var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
        var scaledSize = sourceSize * scale;
        var destRect = viewPort
            .CenterRect(new Rect(scaledSize))
            .Intersect(viewPort);

        var size = Stretch is Stretch.None ? sourceSize : destRect.Size;
        
        _customVisual.Size = new Vector2((float)size.Width, (float)size.Height);

        _customVisual.Offset = new Vector3((float)destRect.Position.X, (float)destRect.Position.Y, 0);
    }

    private class CustomVisualHandler : CompositionCustomVisualHandler
    {
        private TimeSpan _animationElapsed;
        private TimeSpan? _lastServerTime;
        private IAdvancedBitmap? _currentInstance;
        private int _totalTime;
        private readonly List<int> _frameTimes = [];
        private bool _running;

        public static readonly object StopMessage = new();
        public static readonly object StartMessage = new();
        public static readonly object ResetMessage = new();

        public override void OnMessage(object message)
        {
            if (message == StartMessage)
            {
                _running = true;
                _lastServerTime = null;
                RegisterForNextAnimationFrameUpdate();
            }
            else if (message == StopMessage)
                _running = false;
            else if (message == ResetMessage)
                Clear();
            else if (message is IAdvancedBitmap { IsInitialized: true } instance)
            {
                Clear();
                _currentInstance = instance;
                foreach (var delay in instance.Delays)
                {
                    _frameTimes.Add(_totalTime);
                    _totalTime += delay;
                }
            }
            return;

            void Clear()
            {
                _currentInstance = null;
                _totalTime = 0;
                _frameTimes.Clear();
            }
        }

        public override void OnAnimationFrameUpdate()
        {
            if (!_running)
                return;
            Invalidate();
            RegisterForNextAnimationFrameUpdate();
        }

        public override void OnRender(ImmediateDrawingContext drawingContext)
        {
            if (_running)
            {
                if (_lastServerTime.HasValue)
                    _animationElapsed += CompositionNow - _lastServerTime.Value;
                _lastServerTime = CompositionNow;
            }

            if (_currentInstance is not { IsInitialized: true })
                return;

            var bitmap = ProcessFrameTime((int)_animationElapsed.TotalMilliseconds % _totalTime);
            drawingContext.DrawBitmap(bitmap, new Rect(_currentInstance.Size), GetRenderBounds());
            
            return;
            Bitmap ProcessFrameTime(int ms)
            {
                var i = _frameTimes.BinarySearch(ms);
                return _currentInstance.Frames[i < 0 ? ~i - 1 : i];
            }
        }
    }
}
