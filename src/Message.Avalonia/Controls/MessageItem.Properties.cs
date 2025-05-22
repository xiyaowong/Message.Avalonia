using System;
using System.Threading;
using Avalonia;
using Avalonia.Collections;
using Message.Avalonia.Models;

namespace Message.Avalonia.Controls;

internal partial class MessageItem
{
    /// <summary>
    /// Defines the <see cref="Title"/> property
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<MessageItem, string>(
        nameof(Title)
    );

    /// <summary>
    /// Defines the <see cref="Message"/> property
    /// </summary>
    public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<MessageItem, string>(
        nameof(Message)
    );

    /// <summary>
    /// Defines the <see cref="IsOnlyTitle"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsOnlyTitleProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(IsOnlyTitle)
    );

    /// <summary>
    /// Defines the <see cref="Actions"/> property
    /// </summary>
    public static readonly StyledProperty<AvaloniaList<MessageAction>> ActionsProperty = AvaloniaProperty.Register<
        MessageItem,
        AvaloniaList<MessageAction>
    >(nameof(Actions));

    /// <summary>
    /// Defines the <see cref="Type"/> property
    /// </summary>
    public static readonly StyledProperty<MessageType> TypeProperty = AvaloniaProperty.Register<
        MessageItem,
        MessageType
    >(nameof(Type));

    /// <summary>
    /// Defines the <see cref="Duration"/> property
    /// </summary>
    public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<MessageItem, TimeSpan>(
        nameof(Duration),
        TimeSpan.MaxValue
    );

    /// <summary>
    /// Defines the <see cref="ShowClose"/> property
    /// </summary>
    public static readonly StyledProperty<bool> ShowCloseProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(ShowClose),
        true
    );

    /// <summary>
    /// Defines the <see cref="ShowIcon"/> property
    /// </summary>
    public static readonly StyledProperty<bool> ShowIconProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(ShowIcon),
        true
    );

    /// <summary>
    /// Defines the <see cref="IsProgress"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsProgressProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(IsProgress)
    );

    /// <summary>
    /// Defines the <see cref="ProgressTokenSource"/> property
    /// </summary>
    public static readonly StyledProperty<CancellationTokenSource?> ProgressTokenSourceProperty =
        AvaloniaProperty.Register<MessageItem, CancellationTokenSource?>(nameof(ProgressTokenSource));

    /// <summary>
    /// Defines the <see cref="ProgressValue"/> property
    /// </summary>
    public static readonly StyledProperty<double?> ProgressValueProperty = AvaloniaProperty.Register<
        MessageItem,
        double?
    >(nameof(ProgressValue));

    /// <summary>
    /// Defines the <see cref="IsClosing"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsClosingProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(IsClosing)
    );

    /// <summary>
    /// Defines the <see cref="IsClosed"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsClosedProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(IsClosed)
    );

    /// <summary>
    /// Defines the <see cref="Expanded"/> property
    /// </summary>
    public static readonly StyledProperty<bool> ExpandedProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(Expanded)
    );

    /// <summary>
    /// The title of the message.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The message content.
    /// </summary>
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Indicates whether the message is only a title. The Title will be bold.
    /// </summary>
    public bool IsOnlyTitle
    {
        get => GetValue(IsOnlyTitleProperty);
        set => SetValue(IsOnlyTitleProperty, value);
    }

    /// <summary>
    /// Actions that can be performed on the message.
    /// </summary>
    public AvaloniaList<MessageAction> Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>
    /// The type of the message.
    /// </summary>
    public MessageType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// The duration of the message. The default is <see cref="TimeSpan.MaxValue"/>.
    /// This means that the message will not close automatically.
    /// </summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Whether to show the close button.
    /// </summary>
    public bool ShowClose
    {
        get => GetValue(ShowCloseProperty);
        set => SetValue(ShowCloseProperty, value);
    }

    /// <summary>
    /// Whether to show the icon.
    /// </summary>
    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    /// <summary>
    /// Whether the message is a progress message.
    /// </summary>
    public bool IsProgress
    {
        get => GetValue(IsProgressProperty);
        set => SetValue(IsProgressProperty, value);
    }

    /// <summary>
    /// Whether the progress message is cancelable.
    /// </summary>
    public CancellationTokenSource? ProgressTokenSource
    {
        get => GetValue(ProgressTokenSourceProperty);
        set => SetValue(ProgressTokenSourceProperty, value);
    }

    /// <summary>
    /// The progress value of the message. If set to null, the progress bar will be indeterminate.
    /// </summary>
    public double? ProgressValue
    {
        get => GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    /// <summary>
    /// Whether the message is expanded. This is used to animate the expanding of the message.
    /// </summary>
    public bool Expanded
    {
        get => GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    /// <summary>
    /// Whether the message is closing. This is used to animate the closing of the message.
    /// </summary>
    public bool IsClosing
    {
        get => GetValue(IsClosingProperty);
        set => SetValue(IsClosingProperty, value);
    }

    /// <summary>
    /// Whether the message is closed. This is used to animate the closing of the message.
    /// </summary>
    public bool IsClosed
    {
        get => GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }
}
