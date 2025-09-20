using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Avalonia.AnimatedImage.Sample;

public partial class MainWindow : Window
{
    public Stretch[] Stretches { get; } = Enum.GetValues<Stretch>();

    public StretchDirection[] StretchDirections { get; } = Enum.GetValues<StretchDirection>();

    public MainWindow() => InitializeComponent();

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextBox.Text))
            return;
        if (!File.Exists(TextBox.Text))
            return;
        switch ((object) AnimatedImage)
        {
            case AnimatedImage advancedImage:
                advancedImage.Source = IAnimatedBitmap.Load(File.OpenRead(TextBox.Text), true);
                break;
            case Image image:
                image.Source = new Bitmap(File.OpenRead(TextBox.Text));
                break;
        }
    }
}
