using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Message.Avalonia.Models;

namespace Message.Avalonia.UI;

[PseudoClasses(
    PC_Information,
    PC_Success,
    PC_Warning,
    PC_Error,
    PC_BottomRight,
    PC_BottomLeft,
    PC_BottomCenter,
    PC_TopRight,
    PC_TopLeft,
    PC_TopCenter,
    PC_CenterCenter
)]
public class MessageItem : ContentControl
{
    private const string PC_Information = ":information";
    private const string PC_Success = ":success";
    private const string PC_Warning = ":warning";
    private const string PC_Error = ":error";

    private const string PC_BottomRight = ":bottom-right";
    private const string PC_BottomLeft = ":bottom-left";
    private const string PC_BottomCenter = ":bottom-center";
    private const string PC_TopRight = ":top-right";
    private const string PC_TopLeft = ":top-left";
    private const string PC_TopCenter = ":top-center";
    private const string PC_CenterCenter = ":center-center";

    private const string PART_CloseButton = "PART_CloseButton";
    private const string PART_ActionsPanel = "PART_ActionsPanel";

    private Button? _closeButton;
    private ItemsControl? _actionsPanel;

    internal event EventHandler<MessageAction?>? Completed;
    internal event EventHandler? MessageClosed;

    private bool _isCompleted;
    private readonly Stopwatch _durationStopwatch = new();

    #region Properties

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<MessageItem, string>(
        nameof(Title)
    );

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<MessageItem, string>(
        nameof(Message)
    );

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    private bool _isOnlyTitle;

    public static readonly DirectProperty<MessageItem, bool> IsOnlyTitleProperty = AvaloniaProperty.RegisterDirect<
        MessageItem,
        bool
    >(nameof(IsOnlyTitle), o => o.IsOnlyTitle);

    public bool IsOnlyTitle
    {
        get => _isOnlyTitle;
        set => SetAndRaise(IsOnlyTitleProperty, ref _isOnlyTitle, value);
    }

    public static readonly StyledProperty<AvaloniaList<MessageAction>> ActionsProperty = AvaloniaProperty.Register<
        MessageItem,
        AvaloniaList<MessageAction>
    >(nameof(Actions));

    public AvaloniaList<MessageAction> Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public static readonly StyledProperty<MessageType> TypeProperty = AvaloniaProperty.Register<
        MessageItem,
        MessageType
    >(nameof(Type));

    public MessageType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<MessageItem, TimeSpan>(
        nameof(Duration),
        TimeSpan.MaxValue
    );

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly StyledProperty<bool> ShowCloseProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(ShowClose),
        true
    );

    public bool ShowClose
    {
        get => GetValue(ShowCloseProperty);
        set => SetValue(ShowCloseProperty, value);
    }

    public static readonly StyledProperty<bool> ShowIconProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(ShowIcon),
        true
    );

    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    private bool _isClosing;

    public static readonly DirectProperty<MessageItem, bool> IsClosingProperty = AvaloniaProperty.RegisterDirect<
        MessageItem,
        bool
    >(nameof(IsClosing), o => o.IsClosing);

    public bool IsClosing
    {
        get => _isClosing;
        set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }

    public static readonly StyledProperty<bool> IsClosedProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(IsClosed)
    );

    public bool IsClosed
    {
        get => GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    #endregion


    public void ExecuteAction(object obj)
    {
        if (obj is MessageAction action && !_isCompleted)
        {
            _isCompleted = true;
            Dispatcher.UIThread.Post(Close);

            action.Callback?.Invoke();
            Completed?.Invoke(this, action);
        }
        else
        {
            Close();
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>(PART_CloseButton);
        _actionsPanel = e.NameScope.Find<ItemsControl>(PART_ActionsPanel);

        if (_closeButton != null)
        {
            _closeButton.Click += (_, s) =>
            {
                e.Handled = true;
                Close();
            };
        }

        if (_actionsPanel != null)
        {
            _actionsPanel.Loaded += (_, _) => UpdateActionClasses();
        }

        UpdateMessageType();

        _durationStopwatch.Start();
    }

    private void UpdateActionClasses()
    {
        if (_actionsPanel == null)
            return;

        foreach (var button in _actionsPanel.GetLogicalDescendants().OfType<Button>())
        {
            if (button.Tag is not string title)
                continue;

            var action = Actions.FirstOrDefault(v => v.Title == title);
            if (action == null)
                continue;

            button.Classes.AddRange(action.Classes);
        }
    }

    private void UpdateMessageType()
    {
        PseudoClasses.Set(PC_Information, Type == MessageType.Information);
        PseudoClasses.Set(PC_Success, Type == MessageType.Success);
        PseudoClasses.Set(PC_Warning, Type == MessageType.Warning);
        PseudoClasses.Set(PC_Error, Type == MessageType.Error);
    }

    internal void UpdatePosition(MessagePosition pos)
    {
        PseudoClasses.Set(PC_TopLeft, pos == MessagePosition.TopLeft);
        PseudoClasses.Set(PC_TopCenter, pos == MessagePosition.TopCenter);
        PseudoClasses.Set(PC_TopRight, pos == MessagePosition.TopRight);
        PseudoClasses.Set(PC_BottomLeft, pos == MessagePosition.BottomLeft);
        PseudoClasses.Set(PC_BottomCenter, pos == MessagePosition.BottomCenter);
        PseudoClasses.Set(PC_BottomRight, pos == MessagePosition.BottomRight);
        PseudoClasses.Set(PC_CenterCenter, pos == MessagePosition.CenterCenter);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (
            change.Property == ContentProperty
            || change.Property == TitleProperty
            || change.Property == MessageProperty
        )
        {
            IsOnlyTitle = Content == null && string.IsNullOrEmpty(Message);
        }
        else if (change.Property == TypeProperty)
        {
            UpdateMessageType();
        }
        else if (change.Property == ActionsProperty)
        {
            if (Actions.Any(a => (string?)a.Title == null))
            {
                throw new InvalidOperationException("Action title must be a string.");
            }

            var titles = Actions.Select(a => a.Title).ToList();
            if (titles.Distinct().Count() != titles.Count)
            {
                throw new InvalidOperationException("Actions must have unique titles.");
            }
        }
        else if (change.Property == IsClosedProperty)
        {
            if (IsClosed)
            {
                MessageClosed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    #region Duration Logic

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        _durationStopwatch.Stop();
        _durationStopwatch.Reset();
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _durationStopwatch.Restart();
    }

    internal void OnDurationTimerTick()
    {
        if (!IsPointerOver && TimeSpan.FromMilliseconds(_durationStopwatch.ElapsedMilliseconds) >= Duration)
            Close();
    }

    #endregion


    public void Close()
    {
        if (IsClosing || IsClosed)
            return;

        IsClosing = true;

        if (_isCompleted)
            return;

        _isCompleted = true;
        Completed?.Invoke(this, null);
    }
}
