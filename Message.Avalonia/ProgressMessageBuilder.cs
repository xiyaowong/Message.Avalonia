using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Message.Avalonia.Models;
using Message.Avalonia.UI;
using Message.Avalonia.UI.Host;

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

    public ProgressMessageBuilder WithHost(string hostId)
    {
        _hostId = hostId;
        return this;
    }

    public ProgressMessageBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ProgressMessageBuilder HideIcon()
    {
        _hideIcon = true;
        return this;
    }

    public ProgressMessageBuilder WithProgress(Func<MessageProgress, Task> taskAction)
    {
        _progressTask = taskAction;
        _cancelableProgressTask = null;
        _progressTokenSource?.Cancel();
        _progressTokenSource = null;
        return this;
    }

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

    public void ShowInfo()
    {
        _type = MessageType.Information;
        Show();
    }

    public void ShowWarning()
    {
        _type = MessageType.Warning;
        Show();
    }

    public void ShowSuccess()
    {
        _type = MessageType.Success;
        Show();
    }

    public void ShowError()
    {
        _type = MessageType.Error;
        Show();
    }

    #endregion
}
