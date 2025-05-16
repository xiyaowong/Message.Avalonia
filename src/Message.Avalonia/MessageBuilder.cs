using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Threading;
using Message.Avalonia.Controls;
using Message.Avalonia.Controls.Host;
using Message.Avalonia.Models;

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

    // border customization
    private IBrush? _borderBrush;
    private Thickness? _borderThickness;
    private CornerRadius? _cornerRadius;

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

    /// <summary>
    /// Set the host id for the message.
    /// </summary>
    /// <param name="hostId"></param>
    /// <returns></returns>
    public MessageBuilder WithHost(string hostId)
    {
        _hostId = hostId;
        return this;
    }

    /// <summary>
    /// Set the title for the message.
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public MessageBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Set the message for the message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public MessageBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }

    /// <summary>
    /// Set the duration for the message.
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public MessageBuilder WithDuration(TimeSpan duration)
    {
        _duration = duration;
        return this;
    }

    /// <summary>
    /// Set the duration for the message in seconds.
    /// </summary>
    /// <param name="durationInSeconds"></param>
    /// <returns></returns>
    public MessageBuilder WithDuration(uint durationInSeconds)
    {
        _duration = TimeSpan.FromSeconds(durationInSeconds);
        return this;
    }

    /// <summary>
    /// Set the actions for the message.
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public MessageBuilder WithActions(List<MessageAction> actions, Action<MessageAction?> callback)
    {
        _actions = actions;
        _callback = callback;
        return this;
    }

    /// <summary>
    /// Set the actions for the message.
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public MessageBuilder WithActions(List<string> actions, Action<string?> callback)
    {
        var messageActions = actions.Select(a => new MessageAction { Title = a }).ToList();
        var messageActionCallback = new Action<MessageAction?>(action => callback(action?.Title));
        return WithActions(messageActions, messageActionCallback);
    }

    /// <summary>
    /// Set the custom content for the message.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public MessageBuilder WithContent(object content)
    {
        _content = content;
        return this;
    }

    /// <summary>
    /// Set the border style for the message.
    /// </summary>
    /// <param name="brush">The brush to paint the border of the message</param>
    /// <param name="thickness">The thickness of the border</param>
    /// <param name="cornerRadius">The corner radius of the border</param>
    /// <returns></returns>
    public MessageBuilder WithBorder(
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
    public MessageBuilder WithBorder(string? brush = null, string? thickness = null, string? cornerRadius = null)
    {
        return WithBorder(
            brush is not null ? SolidColorBrush.Parse(brush) : null,
            thickness is not null ? Thickness.Parse(thickness) : null,
            cornerRadius is not null ? CornerRadius.Parse(cornerRadius) : null
        );
    }

    /// <summary>
    /// Hide the icon of the message.
    /// </summary>
    /// <returns></returns>
    public MessageBuilder HideIcon()
    {
        _hideIcon = true;
        return this;
    }

    /// <summary>
    /// Hide the close button of the message.
    /// </summary>
    /// <returns></returns>
    public MessageBuilder HideClose()
    {
        _hideClose = true;
        return this;
    }

    /// <summary>
    /// Show the message.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
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

                if (_borderBrush is { } borderBrush)
                    _messageItem.BorderBrush = borderBrush;

                if (_borderThickness is { } borderThickness)
                    _messageItem.BorderThickness = borderThickness;

                if (_cornerRadius is { } cornerRadius)
                    _messageItem.CornerRadius = cornerRadius;

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

    /// <summary>
    /// Show the message as an information message.
    /// </summary>
    public void ShowInfo()
    {
        _type = MessageType.Information;
        Show();
    }

    /// <summary>
    /// Show the message as a warning message.
    /// </summary>
    public void ShowWarning()
    {
        _type = MessageType.Warning;
        Show();
    }

    /// <summary>
    /// Show the message as a success message.
    /// </summary>
    public void ShowSuccess()
    {
        _type = MessageType.Success;
        Show();
    }

    /// <summary>
    /// Show the message as an error message.
    /// </summary>
    public void ShowError()
    {
        _type = MessageType.Error;
        Show();
    }

    /// <summary>
    /// Close the message.
    /// </summary>
    public void Dismiss()
    {
        Dispatcher.UIThread.Invoke(() => _messageItem?.Close());
    }

    #endregion
}
