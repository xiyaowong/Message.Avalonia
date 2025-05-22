using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Message.Avalonia.Controls;
using Message.Avalonia.Models;

namespace Message.Avalonia;

public class ProgressMessageBuilder
{
    private MessageItem? _messageItem;
    private MessageType? _type;

    // configurable
    private string _hostId = MessageHost.DEFAULT_HOST_ID;
    private string? _title;
    private bool? _hideIcon;

    // border customization
    private IBrush? _borderBrush;
    private Thickness? _borderThickness;
    private CornerRadius? _cornerRadius;

    // progress related
    private Func<MessageProgress, Task>? _progressTask;
    private Func<MessageProgress, CancellationToken, Task>? _cancelableProgressTask;
    private CancellationTokenSource? _progressTokenSource;

    /// <summary>
    /// Sets the host ID for the message.
    /// </summary>
    /// <param name="hostId">The ID of the host where the message will be displayed.</param>
    public ProgressMessageBuilder WithHost(string hostId)
    {
        _hostId = hostId;
        return this;
    }

    /// <summary>
    /// Sets the title of the progress message.
    /// </summary>
    /// <param name="title">The title text to display.</param>
    public ProgressMessageBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Configures the message to hide its icon.
    /// </summary>
    public ProgressMessageBuilder HideIcon()
    {
        _hideIcon = true;
        return this;
    }

    /// <summary>
    /// Set the border style for the message.
    /// </summary>
    /// <param name="brush">The brush to paint the border of the message</param>
    /// <param name="thickness">The thickness of the border</param>
    /// <param name="cornerRadius">The corner radius of the border</param>
    /// <returns></returns>
    public ProgressMessageBuilder WithBorder(
        IBrush? brush = null,
        Thickness? thickness = null,
        CornerRadius? cornerRadius = null
    )
    {
        _borderBrush = brush;
        // 0 does not trigger property changed and will not be applied,
        // so use -1 instead (has the same visual effect as 0)
        _borderThickness = thickness is { IsUniform: true, Bottom: 0 } ? Thickness.Parse("-1") : thickness;
        _cornerRadius = cornerRadius is { IsUniform: true, BottomLeft: 0 } ? CornerRadius.Parse("-1") : cornerRadius;
        return this;
    }

    /// <summary>
    /// Set the border style for the message with string parameters.
    /// </summary>
    /// <param name="brush">Border color string to parse (e.g. "#FF0000")</param>
    /// <param name="thickness">Border thickness string to parse (e.g. "1,2,1,2")</param>
    /// <param name="cornerRadius">Corner radius string to parse (e.g. "5")</param>
    /// <returns>The MessageBuilder instance</returns>
    public ProgressMessageBuilder WithBorder(
        string? brush = null,
        string? thickness = null,
        string? cornerRadius = null
    )
    {
        return WithBorder(
            brush is not null ? SolidColorBrush.Parse(brush) : null,
            thickness is not null ? Thickness.Parse(thickness) : null,
            cornerRadius is not null ? CornerRadius.Parse(cornerRadius) : null
        );
    }

    /// <summary>
    /// Sets the progress task to be executed.
    /// </summary>
    /// <param name="taskAction">The task action that receives a MessageProgress instance.</param>
    public ProgressMessageBuilder WithProgress(Func<MessageProgress, Task> taskAction)
    {
        _progressTask = taskAction;
        _cancelableProgressTask = null;
        _progressTokenSource?.Cancel();
        _progressTokenSource = null;
        return this;
    }

    /// <summary>
    /// Sets a cancellable progress task to be executed.
    /// </summary>
    /// <param name="taskAction">The task action that receives a MessageProgress instance and a CancellationToken.</param>
    public ProgressMessageBuilder WithProgress(Func<MessageProgress, CancellationToken, Task> taskAction)
    {
        _progressTask = null;
        _cancelableProgressTask = taskAction;
        _progressTokenSource?.Cancel();
        _progressTokenSource = new CancellationTokenSource();
        return this;
    }

    private void Show()
    {
        if (_messageItem != null)
        {
            throw new InvalidOperationException("Message is already shown. Cannot show again.");
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            _messageItem = new MessageItem
            {
                ShowClose = false,
                Content = null,
                Duration = TimeSpan.MaxValue,
                IsProgress = true,
                ProgressTokenSource = _progressTokenSource,
            };

            if (!string.IsNullOrEmpty(_title))
                _messageItem.Title = _title;

            if (_type is { } type)
                _messageItem.Type = type;

            if (_hideIcon == true)
                _messageItem.ShowIcon = false;

            if (_borderBrush is { } borderBrush)
                _messageItem.BorderBrush = borderBrush;

            if (_borderThickness is { } borderThickness)
                _messageItem.BorderThickness = borderThickness;

            if (_cornerRadius is { } cornerRadius)
                _messageItem.CornerRadius = cornerRadius;

            MessageHost.GetHostById(_hostId).AddMessage(_messageItem);
        });

        Task.Run(async () =>
        {
            var progress = new MessageProgress(_messageItem!);

            var tasks = new List<Task> { Task.Delay(200) };

            if (_cancelableProgressTask != null)
            {
                tasks.Add(_cancelableProgressTask(progress, _progressTokenSource!.Token));
            }

            if (_progressTask != null)
            {
                tasks.Add(_progressTask(progress));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            finally
            {
                Dispatcher.UIThread.Post(() => _messageItem?.Close());
            }
        });
    }

    #region Show

    /// <summary>
    /// Shows an information progress message.
    /// </summary>
    public void ShowInfo()
    {
        _type = MessageType.Information;
        Show();
    }

    /// <summary>
    /// Shows a warning progress message.
    /// </summary>
    public void ShowWarning()
    {
        _type = MessageType.Warning;
        Show();
    }

    /// <summary>
    /// Shows a success progress message.
    /// </summary>
    public void ShowSuccess()
    {
        _type = MessageType.Success;
        Show();
    }

    /// <summary>
    /// Shows an error progress message.
    /// </summary>
    public void ShowError()
    {
        _type = MessageType.Error;
        Show();
    }

    #endregion
}
