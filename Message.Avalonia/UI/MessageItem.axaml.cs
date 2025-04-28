using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Message.Avalonia.Helpers;
using Message.Avalonia.Models;

namespace Message.Avalonia.UI;

[PseudoClasses(PC_Information, PC_Success, PC_Warning, PC_Error)]
public class MessageItem : ContentControl
{
    private const string PC_Information = ":information";
    private const string PC_Success = ":success";
    private const string PC_Warning = ":warning";
    private const string PC_Error = ":error";

    private const string PART_CloseButton = "PART_CloseButton";
    private const string PART_ActionsPanel = "PART_ActionsPanel";

    #region Properties

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<NotificationCard, string>(
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

    public static readonly StyledProperty<bool> IsOnlyTitleProperty = AvaloniaProperty.Register<MessageItem, bool>(
        nameof(IsOnlyTitle)
    );

    public bool IsOnlyTitle
    {
        get => Content == null && string.IsNullOrEmpty(Message);
        set => SetValue(IsOnlyTitleProperty, value);
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

    #endregion

    private Button? _closeButton;
    private ItemsControl? _actionsPanel;

    internal event MessageCompletedHandler? Completed;
    private bool _isCompleted;
    private bool? _isClosed;
    private readonly Stopwatch _durationStopwatch = new();

    public void ExecuteAction(object action)
    {
        if (action is MessageAction act)
        {
            _isCompleted = true;
            act.Callback?.Invoke();
            Completed?.Invoke(this, Actions.FirstOrDefault(a => a.Title == act.Title));
        }

        Close();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>(PART_CloseButton);
        _actionsPanel = e.NameScope.Find<ItemsControl>(PART_ActionsPanel);

        if (_closeButton != null)
        {
            _closeButton.Click += CloseButton_Click;
        }

        if (_actionsPanel != null)
        {
            _actionsPanel.Loaded += (_, _) => UpdateActionClasses();
        }

        UpdatePseudoClasses();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Close();
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

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_Information, Type == MessageType.Information);
        PseudoClasses.Set(PC_Success, Type == MessageType.Success);
        PseudoClasses.Set(PC_Warning, Type == MessageType.Warning);
        PseudoClasses.Set(PC_Error, Type == MessageType.Error);
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
            UpdatePseudoClasses();
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


    public void Show()
    {
        switch (_isClosed)
        {
            case true:
                throw new InvalidOperationException("Message is already closed. Cannot show again.");
            case false:
                throw new InvalidOperationException("Message is already shown. Cannot show again.");
        }

        _isClosed = false;

        this.Animate(OpacityProperty, 0d, 1d, TimeSpan.FromMilliseconds(500));
        this.Animate<double>(MaxHeightProperty, 0, 500, TimeSpan.FromMilliseconds(500));
        this.Animate(MarginProperty, new Thickness(0, 10, 0, -10), new Thickness(), TimeSpan.FromMilliseconds(500));
        _durationStopwatch.Start();
    }

    public void Close()
    {
        if (_isClosed == true)
            return;
        _isClosed = true;

        this.Animate(OpacityProperty, 1d, 0d, TimeSpan.FromMilliseconds(500));
        this.Animate(MarginProperty, new Thickness(), new Thickness(0, 0, 0, -100), TimeSpan.FromMilliseconds(500));
        this.Animate<double>(MaxHeightProperty, 500, 0, TimeSpan.FromMilliseconds(400));

        if (!_isCompleted)
        {
            _isCompleted = true;
            Completed?.Invoke(this, null);
        }
    }
}
