using Avalonia.Threading;
using Message.Avalonia.Controls;

namespace Message.Avalonia.Models;

public class MessageProgress
{
    private readonly MessageItem _messageItem;

    internal MessageProgress(MessageItem messageItem)
    {
        _messageItem = messageItem;
    }

    /// <summary>
    /// Update the message of the progress.
    /// </summary>
    /// <param name="message"></param>
    public void Report(string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _messageItem.Message = message;
        });
    }

    /// <summary>
    /// Update the progress value of the message.
    /// </summary>
    /// <param name="value"></param>
    public void Report(double? value)
    {
        var progressValue =
            value == null ? null
            : value < 0 ? 0
            : value > 100 ? 100
            : value;

        Dispatcher.UIThread.Post(() =>
        {
            _messageItem.ProgressValue = progressValue;
        });
    }

    /// <summary>
    /// Update the message and progress value of the message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="value"></param>
    public void Report(string message, double? value)
    {
        var progressValue =
            value == null ? null
            : value < 0 ? 0
            : value > 100 ? 100
            : value;
        Dispatcher.UIThread.Post(() =>
        {
            _messageItem.Message = message;
            _messageItem.ProgressValue = progressValue;
        });
    }
}
