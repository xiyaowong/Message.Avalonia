using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Message.Avalonia.Models;

namespace Message.Avalonia.UI.Host;

public class MessageHost : ItemsControl
{
    public const string DEFAULT_HOST_ID = "__DefaultHostId__";

    private static readonly List<WeakReference<MessageHost>> HostList = [];

    private readonly DispatcherTimer _durationTimer =
        new() { Interval = TimeSpan.FromMilliseconds(300), IsEnabled = true };

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
    ///     Gets or sets the position of the toast host.
    /// </summary>
    public MessagePosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    #endregion

    private IEnumerable<MessageItem> MessageItems => Items.OfType<MessageItem>();

    public MessageHost()
    {
        _durationTimer.Tick += delegate
        {
            foreach (var msg in MessageItems)
                msg.OnDurationTimerTick();
        };

        Items.CollectionChanged += delegate
        {
            foreach (var msg in MessageItems)
            {
                msg.Completed -= OnNotificationDismissed;
                msg.Completed += OnNotificationDismissed;
            }

            if (Items.Count > 0)
                _durationTimer.Start();
            else
                _durationTimer.Stop();
        };
    }

    private void OnNotificationDismissed(object? sender, MessageAction? e)
    {
        if (sender is MessageItem msg)
        {
            msg.Completed -= OnNotificationDismissed;
            Task.Delay(500).ContinueWith(_ => Items.Remove(msg), TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    /// <summary>
    ///     Called when the template is applied.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        HostList.Insert(0, new WeakReference<MessageHost>(this));
        Debug.WriteLine($"Insert host {HostId} to list. Count: {HostList.Count}");
        OnPositionChanged(Position);
    }

    /// <summary>
    ///     Called when a property is changed.
    /// </summary>
    /// <param name="change"></param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PositionProperty && change.NewValue is MessagePosition pos)
            OnPositionChanged(pos);
    }

    private void OnPositionChanged(MessagePosition position)
    {
        HorizontalAlignment = position switch
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
        VerticalAlignment = position switch
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
    }

    internal static MessageHost GetHostById(string id = DEFAULT_HOST_ID)
    {
        HostList.RemoveAll(r => !r.TryGetTarget(out _));
        foreach (var r in HostList)
        {
            if (r.TryGetTarget(out var host) && host.HostId == id)
                return host;
        }

        throw new InvalidOperationException($"Host with id '{id}' not found.");
    }
}
