using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Threading;
using Message.Avalonia.Models;
using Message.Avalonia.UI;
using Message.Avalonia.UI.Host;

namespace Message.Avalonia;

public class MessageBuilder
{
    private MessageItem? _messageItem;
    private string _hostId = MessageHost.DEFAULT_HOST_ID;
    private string? _title;
    private string? _message;
    private TimeSpan? _duration;
    private object? _content;
    private MessageType? _type;
    private List<MessageAction>? _actions;
    private Action<MessageAction?>? _callback;
    private bool? _hideClose;
    private bool? _hideIcon;

    internal MessageBuilder WithOptions(MessageOptions options)
    {
        if (!string.IsNullOrEmpty(options.Title))
            _title = options.Title;

        if (options.Duration is { } duration)
            _duration = duration;

        if (options.Content is not null)
            _content = options.Content;

        if (!string.IsNullOrEmpty(options.HostId))
            _hostId = options.HostId;

        if (options.HideIcon is not null)
            _hideIcon = options.HideIcon;

        if (options.HideClose is not null)
            _hideClose = options.HideClose;

        return this;
    }

    public MessageBuilder WithHost(string hostId)
    {
        _hostId = hostId;
        return this;
    }

    public MessageBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public MessageBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }

    public MessageBuilder WithDuration(TimeSpan duration)
    {
        _duration = duration;
        return this;
    }

    public MessageBuilder WithDuration(uint durationInSeconds)
    {
        _duration = TimeSpan.FromSeconds(durationInSeconds);
        return this;
    }

    public MessageBuilder WithActions(List<MessageAction> actions, Action<MessageAction?> callback)
    {
        _actions = actions;
        _callback = callback;
        return this;
    }

    public MessageBuilder WithActions(List<string> actions, Action<string?> callback)
    {
        var messageActions = actions.Select(a => new MessageAction { Title = a }).ToList();
        var messageActionCallback = new Action<MessageAction?>(action => callback(action?.Title));
        return WithActions(messageActions, messageActionCallback);
    }

    public MessageBuilder WithContent(object content)
    {
        _content = content;
        return this;
    }

    public MessageBuilder HideIcon()
    {
        _hideIcon = true;
        return this;
    }

    public MessageBuilder HideClose()
    {
        _hideClose = true;
        return this;
    }

    private void Show()
    {
        if (_messageItem != null)
        {
            throw new InvalidOperationException("Message is already shown. Cannot show again.");
        }

        Dispatcher.UIThread.Invoke(
            delegate
            {
                _messageItem = new MessageItem();

                if (!string.IsNullOrEmpty(_title))
                    _messageItem.Title = _title;

                if (!string.IsNullOrEmpty(_message))
                    _messageItem.Message = _message;

                if (_duration is { } duration)
                    _messageItem.Duration = duration;

                if (_content != null)
                    _messageItem.Content = _content;

                if (_type is { } type)
                    _messageItem.Type = type;

                if (_actions != null)
                    _messageItem.Actions = new AvaloniaList<MessageAction>(_actions);

                if (_hideClose == true)
                    _messageItem.ShowClose = false;

                if (_hideIcon == true)
                    _messageItem.ShowIcon = false;

                _messageItem.Completed -= OnMessageItemCompleted;
                _messageItem.Completed += OnMessageItemCompleted;

                MessageHost.GetHostById(_hostId).AddMessage(_messageItem);
            }
        );
    }

    private void OnMessageItemCompleted(object? sender, MessageAction? e)
    {
        _callback?.Invoke(e);
        if (_messageItem != null)
            _messageItem.Completed -= OnMessageItemCompleted;
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

    public void Dismiss()
    {
        Dispatcher.UIThread.Invoke(() => _messageItem?.Close());
    }

    #endregion
}
