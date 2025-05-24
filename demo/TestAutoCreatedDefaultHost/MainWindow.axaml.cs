using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Message.Avalonia;

namespace TestAutoCreatedDefaultHost;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Hello_Click(object? sender, RoutedEventArgs e)
    {
        MessageManager.Default.ShowInformationMessage("Hello world!");
    }

    private void NewWindow_Click(object? sender, RoutedEventArgs e)
    {
        var window = new MainWindow();
        window.Show();
    }

    private void GC_Collect_Click(object? sender, RoutedEventArgs e)
    {
        GC.Collect();
    }
}
