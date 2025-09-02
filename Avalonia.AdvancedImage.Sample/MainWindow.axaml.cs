using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.AdvancedImage.Sample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextBox.Text))
            return;
        if (!File.Exists(TextBox.Text))
            return;
        AdvancedImage.Source = new AdvancedBitmap(File.OpenRead(TextBox.Text), true);
    }
}
