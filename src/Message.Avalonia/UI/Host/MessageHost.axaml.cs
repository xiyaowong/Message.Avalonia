using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Message.Avalonia.Models;

namespace Message.Avalonia.UI.Host;

public class MessageHost : TemplatedControl
{
    public const string DEFAULT_HOST_ID = "__DefaultHostId__";

    private static readonly List<WeakReference<MessageHost>> HostList = [];

    // Have no idea why the DispatcherTimer always automatically stops, so use a System.Timers.Timer instead.
    private readonly Timer _durationTimer = new() { Interval = 300, AutoReset = true };

    #region Properties

    public static readonly StyledProperty<string> HostIdProperty = AvaloniaProperty.Register<MessageHost, string>(
        nameof(HostId),
        DEFAULT_HOST_ID
    );

    public string HostId
    {
        get => GetValue(HostIdProperty);
        set => SetValue(HostIdProperty, value);
    }

    /// <summary>
    ///     Defines the position of the toast host relative to its parent.
    /// </summary>
    public static readonly StyledProperty<MessagePosition> PositionProperty = AvaloniaProperty.Register<
        MessageHost,
        MessagePosition
    >(nameof(Position));

    /// <summary>
    ///     Gets or sets the position of the message host.
    /// </summary>
    public MessagePosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    #endregion

    private readonly IList<MessageItem> _pendingItems = [];

    private Panel? _itemsPanel;
    private IList? _items => _itemsPanel?.Children;

    private IEnumerable<MessageItem> MessageItems => _items != null ? _items.OfType<MessageItem>() : [];

    public MessageHost()
    {
        _durationTimer.Elapsed += (_, _) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                foreach (var msg in MessageItems)
                {
                    msg.OnDurationTimerTick();
                }
            });
        };
    }

    internal void AddMessage(MessageItem msg)
    {
        if (_items == null)
        {
            _pendingItems.Add(msg);
            return;
        }

        _pendingItems.Remove(msg);

        msg.MessageClosed += (sender, _) =>
        {
            var item = (MessageItem)sender!;
            _items.Remove(item);
            _durationTimer.Enabled = MessageItems.Any();
        };
        msg.UpdatePosition(Position);
        // Cancel the expand animation when there is no previous message
        msg.Expanded = _items.Count == 0;

        _items.Add(msg);
        _durationTimer.Enabled = MessageItems.Any();
    }

    // <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        HostList.Insert(0, new WeakReference<MessageHost>(this));

        _itemsPanel = e.NameScope.Find<Panel>("PART_Items");

        _pendingItems.ToList().ForEach(AddMessage);

        OnPositionChanged(Position);
    }

    // <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PositionProperty && change.NewValue is MessagePosition pos)
            OnPositionChanged(pos);
    }

    private void OnPositionChanged(MessagePosition position)
    {
        if (_itemsPanel == null)
            return;

        _itemsPanel.HorizontalAlignment = position switch
        {
            MessagePosition.BottomRight => HorizontalAlignment.Right,
            MessagePosition.BottomCenter => HorizontalAlignment.Center,
            MessagePosition.BottomLeft => HorizontalAlignment.Left,
            MessagePosition.TopRight => HorizontalAlignment.Right,
            MessagePosition.TopCenter => HorizontalAlignment.Center,
            MessagePosition.TopLeft => HorizontalAlignment.Left,
            MessagePosition.CenterCenter => HorizontalAlignment.Center,
            _ => throw new ArgumentOutOfRangeException(),
        };
        _itemsPanel.VerticalAlignment = position switch
        {
            MessagePosition.BottomRight => VerticalAlignment.Bottom,
            MessagePosition.BottomCenter => VerticalAlignment.Bottom,
            MessagePosition.BottomLeft => VerticalAlignment.Bottom,
            MessagePosition.TopRight => VerticalAlignment.Top,
            MessagePosition.TopCenter => VerticalAlignment.Top,
            MessagePosition.TopLeft => VerticalAlignment.Top,
            MessagePosition.CenterCenter => VerticalAlignment.Center,
            _ => throw new ArgumentOutOfRangeException(),
        };

        if (_itemsPanel is ReversibleStackPanel panel)
        {
            panel.ReverseOrder = VerticalAlignment is VerticalAlignment.Top or VerticalAlignment.Center;
        }

        foreach (var messageItem in MessageItems)
        {
            messageItem.UpdatePosition(position);
        }
    }

    internal static MessageHost GetHostById(string id = DEFAULT_HOST_ID)
    {
        HostList.RemoveAll(r => !r.TryGetTarget(out _));
        foreach (var r in HostList)
        {
            if (r.TryGetTarget(out var host) && host.HostId == id)
                return host;
        }

        if (id == DEFAULT_HOST_ID)
        {
            var host = Dispatcher.UIThread.Invoke(() =>
            {
                var defaultHost = new MessageHost();
                defaultHost.InstallDefaultHostFromTopLevel();
                return defaultHost;
            });
            return host;
        }

        throw new InvalidOperationException($"Host with id '{id}' not found.");
    }

    private void InstallDefaultHostFromTopLevel()
    {
        var topLevel = Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => TopLevel.GetTopLevel(desktop.MainWindow),
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null,
        };

        if (topLevel == null)
            return;

        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        topLevel.TemplateApplied += TopLevelOnTemplateApplied;
        var adorner = topLevel.FindDescendantOfType<VisualLayerManager>()?.AdornerLayer;
        if (adorner is not null)
        {
            adorner.Children.Add(this);
            AdornerLayer.SetAdornedElement(this, adorner);
        }
    }

    private void TopLevelOnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (Parent is AdornerLayer adornerLayer)
        {
            adornerLayer.Children.Remove(this);
            AdornerLayer.SetAdornedElement(this, null);
        }

        var topLevel = (TopLevel)sender!;
        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        InstallDefaultHostFromTopLevel();
    }
}
