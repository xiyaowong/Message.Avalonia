using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Message.Avalonia;

internal static class TopLevelHelpers
{
    public static TopLevel GetCurrentTopLevel()
    {
        TopLevel? topLevel;
        switch (Application.Current?.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                var window = desktop.Windows.FirstOrDefault(win => win.IsActive) ?? desktop.MainWindow;
                topLevel = TopLevel.GetTopLevel(window);
                break;
            }
            case ISingleViewApplicationLifetime singleView:
                topLevel = TopLevel.GetTopLevel(singleView.MainView);
                break;
            default:
                topLevel = null;
                break;
        }

        if (topLevel == null)
            throw new InvalidOperationException("No top level found.");

        return topLevel;
    }

    public static AdornerLayer? GetAdornerLayer(this TopLevel? topLevel) =>
        topLevel?.FindDescendantOfType<VisualLayerManager>()?.AdornerLayer;
}
