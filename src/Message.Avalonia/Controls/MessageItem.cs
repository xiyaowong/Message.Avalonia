using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Message.Avalonia.Models;

namespace Message.Avalonia.Controls;

[PseudoClasses(PC_Information, PC_Success, PC_Warning, PC_Error)]
[PseudoClasses(PC_BottomRight, PC_BottomLeft, PC_BottomCenter, PC_TopRight, PC_TopLeft, PC_TopCenter, PC_CenterCenter)]
[PseudoClasses(PC_HasBorderBrush, PC_HasBorderThickness, PC_HasCornerRadius)]
internal partial class MessageItem : ContentControl
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

    private const string PC_HasBorderBrush = ":has-border-brush";
    private const string PC_HasBorderThickness = ":has-border-thickness";
    private const string PC_HasCornerRadius = ":has-corner-radius";

    private const string PART_CloseButton = "PART_CloseButton";
    private const string PART_ActionsPanel = "PART_ActionsPanel";
    private const string PART_ProgressCancelButton = "PART_ProgressCancelButton";

    private Button? _closeButton;
    private Button? _progressCancelButton;
    private ItemsControl? _actionsPanel;

    internal event EventHandler<MessageAction?>? Completed;
    internal event EventHandler? MessageClosed;

    private bool _isCompleted;

    private readonly Timer _durationTimer = new()
    {
        Interval = 300,
        AutoReset = true,
        Enabled = false,
    };

    private readonly Stopwatch _durationStopwatch = new();

    static MessageItem()
    {
        Button.ClickEvent.AddClassHandler<MessageItem>(ButtonClickEventHandler);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>(PART_CloseButton);
        _actionsPanel = e.NameScope.Find<ItemsControl>(PART_ActionsPanel);
        _progressCancelButton = e.NameScope.Find<Button>(PART_ProgressCancelButton);

        if (_closeButton != null)
        {
            _closeButton.Click += (_, ee) =>
            {
                ee.Handled = true;
                Close();
            };
        }

        if (_actionsPanel != null)
        {
            _actionsPanel.Loaded += (_, _) => UpdateActionClasses();
        }

        if (_progressCancelButton != null)
        {
            _progressCancelButton.Click += (_, ee) =>
            {
                ee.Handled = true;
                ProgressCancelButton_Click();
            };
        }

        UpdateMessageType();

        StartDurationTimer();
    }

    private void ProgressCancelButton_Click()
    {
        // Notify the user to cancel the operation,
        // and the message will be closed like a non-cancelable message,
        // waiting for the user task to complete
        ProgressTokenSource?.Cancel();
    }

    private void UpdateActionClasses()
    {
        if (_actionsPanel == null)
            return;

        foreach (var button in _actionsPanel.GetLogicalDescendants().OfType<Button>())
        {
            if (button.Tag is MessageAction action)
            {
                button.Classes.AddRange(action.Classes);
            }
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        var property = change.Property;

        switch (property)
        {
            case not null when property == ContentProperty || property == TitleProperty || property == MessageProperty:
                IsOnlyTitle = Content == null && string.IsNullOrEmpty(Message);
                break;
            case not null when property == TypeProperty:
                UpdateMessageType();
                break;
            case not null when property == ActionsProperty:
                ValidateActions();
                break;
            case not null when property == IsClosedProperty && IsClosed:
                MessageClosed?.Invoke(this, EventArgs.Empty);
                break;
            case not null when property == BorderBrushProperty:
                PseudoClasses.Set(PC_HasBorderBrush, BorderBrush != null);
                break;
            case not null when property == BorderThicknessProperty:
                PseudoClasses.Set(PC_HasBorderThickness, BorderThickness != default);
                break;
            case not null when property == CornerRadiusProperty:
                PseudoClasses.Set(PC_HasCornerRadius, CornerRadius != default);
                break;
        }

        return;

        void ValidateActions()
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

    private static void ButtonClickEventHandler(MessageItem item, RoutedEventArgs args)
    {
        if (args.Source is not Button { Tag: MessageAction action })
        {
            return;
        }

        args.Handled = true;

        if (item._isCompleted)
        {
            item.Close();
            return;
        }

        item._isCompleted = true;
        item.Close();

        action.Callback?.Invoke();
        item.Completed?.Invoke(item, action);
    }

    #region Duration Logic

    private void StartDurationTimer()
    {
        _durationStopwatch.Restart();

        _durationTimer.Elapsed -= DurationTimerOnElapsed;
        _durationTimer.Elapsed += DurationTimerOnElapsed;
        _durationTimer.Start();
    }

    private void StopDurationTimer()
    {
        _durationStopwatch.Stop();

        _durationTimer.Elapsed -= DurationTimerOnElapsed;
        _durationTimer.Stop();
    }

    private void DurationTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (IsPointerOver || TimeSpan.FromMilliseconds(_durationStopwatch.ElapsedMilliseconds) < Duration)
                return;

            _durationTimer.Dispose();
            _durationStopwatch.Stop();
            Close();
        });
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        StopDurationTimer();
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        StartDurationTimer();
    }

    #endregion


    public void Close()
    {
        if (IsClosing || IsClosed)
            return;

        IsClosing = true;

        StopDurationTimer();

        if (!_isCompleted)
        {
            _isCompleted = true;
            Completed?.Invoke(this, null);
        }
    }

#if DEBUG
    ~MessageItem()
    {
        Console.WriteLine("MessageItem Finalizer");
    }
#endif
}
