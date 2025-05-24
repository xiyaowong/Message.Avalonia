using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Message.Avalonia.Models;

namespace Message.Avalonia.Controls;

public class MessageHost : TemplatedControl
{
    public const string DEFAULT_HOST_ID = "__DefaultHostId__";

    private static readonly List<MessageHost> HostList = [];
    private readonly ConcurrentQueue<MessageItem> _pendingItemsQueue = [];
    private bool _isAutoCreated;

    private ScrollViewer? _container;
    private ReversibleStackPanel? _itemsPanel;

    #region Properties

    /// <summary>
    /// Defines the <see cref="HostId"/> property
    /// </summary>
    public static readonly StyledProperty<string> HostIdProperty = AvaloniaProperty.Register<MessageHost, string>(
        nameof(HostId),
        DEFAULT_HOST_ID
    );

    /// <summary>
    /// Defines the <see cref="Position"/> property
    /// </summary>
    public static readonly StyledProperty<MessagePosition> PositionProperty = AvaloniaProperty.Register<
        MessageHost,
        MessagePosition
    >(nameof(Position));

    /// <summary>
    /// The id of the host. This is used to identify the host when there are multiple hosts in the application.
    /// </summary>
    public string HostId
    {
        get => GetValue(HostIdProperty);
        set => SetValue(HostIdProperty, value);
    }

    /// <summary>
    /// The position of the host. This is used to determine where the messages will be displayed.
    /// </summary>
    public MessagePosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    #endregion

    public MessageHost()
    {
        HostList.Insert(0, this);
    }

    internal void AddMessage(MessageItem msg)
    {
        if (_itemsPanel == null)
        {
            _pendingItemsQueue.Enqueue(msg);
            return;
        }

        msg.MessageClosed += (sender, _) =>
        {
            _itemsPanel?.Children.Remove((sender as MessageItem)!);
        };
        msg.UpdatePosition(Position);
        // Cancel the expanding animation when there is no previous message
        msg.Expanded = _itemsPanel.Children.Count == 0;

        _itemsPanel.Children.Add(msg);
    }

    // <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Insert back if the host be replaced
        if (!HostList.Contains(this))
        {
            HostList.Insert(0, this);
        }

        _itemsPanel = e.NameScope.Find<ReversibleStackPanel>("PART_Items");
        _container = e.NameScope.Find<ScrollViewer>("PART_Container");

        while (_pendingItemsQueue.TryDequeue(out var msg))
            AddMessage(msg);

        OnPositionChanged(Position);
    }

    // <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PositionProperty && change.NewValue is MessagePosition pos)
        {
            OnPositionChanged(pos);
        }
    }

    private void OnPositionChanged(MessagePosition position)
    {
        if (_container != null)
        {
            _container.HorizontalAlignment = position switch
            {
                MessagePosition.BottomRight => HorizontalAlignment.Right,
                MessagePosition.BottomCenter => HorizontalAlignment.Center,
                MessagePosition.BottomLeft => HorizontalAlignment.Left,
                MessagePosition.TopRight => HorizontalAlignment.Right,
                MessagePosition.TopCenter => HorizontalAlignment.Center,
                MessagePosition.TopLeft => HorizontalAlignment.Left,
                MessagePosition.CenterCenter => HorizontalAlignment.Center,
                _ => throw new ArgumentOutOfRangeException(nameof(position)),
            };
            _container.VerticalAlignment = position switch
            {
                MessagePosition.BottomRight => VerticalAlignment.Bottom,
                MessagePosition.BottomCenter => VerticalAlignment.Bottom,
                MessagePosition.BottomLeft => VerticalAlignment.Bottom,
                MessagePosition.TopRight => VerticalAlignment.Top,
                MessagePosition.TopCenter => VerticalAlignment.Top,
                MessagePosition.TopLeft => VerticalAlignment.Top,
                MessagePosition.CenterCenter => VerticalAlignment.Center,
                _ => throw new ArgumentOutOfRangeException(nameof(position)),
            };
        }

        if (_itemsPanel != null)
        {
            _itemsPanel.ReverseOrder =
                position
                    is MessagePosition.TopLeft
                        or MessagePosition.TopCenter
                        or MessagePosition.TopRight
                        or MessagePosition.CenterCenter;

            foreach (var msg in _itemsPanel.Children.OfType<MessageItem>())
            {
                msg.UpdatePosition(position);
            }
        }
    }

    internal static MessageHost GetHostById(string id = DEFAULT_HOST_ID)
    {
        if (id == DEFAULT_HOST_ID)
            return Dispatcher.UIThread.Invoke(GetDefaultHost);

        var host = HostList.FirstOrDefault(host => host.HostId == id);

        if (host is not null)
            return host;

        throw new InvalidOperationException($"Host with id '{id}' not found.");
    }

    private static MessageHost GetDefaultHost()
    {
        Dispatcher.UIThread.VerifyAccess();

        var defaultHosts = HostList.Where(host => host is { HostId: DEFAULT_HOST_ID }).ToList();

        // Return user created host if exists
        var notAutoCreatedHost = defaultHosts.FirstOrDefault(host => host is { _isAutoCreated: false });
        if (notAutoCreatedHost is not null)
            return notAutoCreatedHost;

        // Return the host installed on the current top level
        var currentTopLevel = TopLevelHelpers.GetCurrentTopLevel();
        var adornerLayer = currentTopLevel.GetAdornerLayer();
        var defaultHost = defaultHosts.FirstOrDefault(host =>
            host is { Parent: AdornerLayer _adornerLayer } && _adornerLayer == adornerLayer
        );
        if (defaultHost is not null)
            return defaultHost;

        // Create a new default host for the current top level
        var newDefaultHost = new MessageHost { _isAutoCreated = true };
        newDefaultHost.InstallFromTopLevel(currentTopLevel);
        return newDefaultHost;
    }

    private void InstallFromTopLevel(TopLevel topLevel)
    {
        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        topLevel.TemplateApplied += TopLevelOnTemplateApplied;

        var adorner = topLevel.GetAdornerLayer();
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
        InstallFromTopLevel(topLevel);
    }

    private void Uninstall()
    {
        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        }

        if (Parent is AdornerLayer adornerLayer)
        {
            adornerLayer.Children.Remove(this);
            AdornerLayer.SetAdornedElement(this, null);
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

#if DEBUG
        Console.WriteLine($"OnDetachedFromVisualTree: {HostId}");
#endif

        HostList.Remove(this);

        if (_itemsPanel != null)
        {
            foreach (var messageItem in _itemsPanel.Children.OfType<MessageItem>())
            {
                messageItem.Close();
            }
        }

        if (_isAutoCreated)
        {
            Uninstall();
        }
    }

#if DEBUG
    ~MessageHost()
    {
        Console.WriteLine("MessageHost finalized");
    }
#endif
}
