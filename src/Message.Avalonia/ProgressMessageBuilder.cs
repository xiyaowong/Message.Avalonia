using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Message.Avalonia.Controls;
using Message.Avalonia.Controls.Host;
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
