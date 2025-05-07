using Avalonia.Threading;
using Message.Avalonia.UI;

namespace Message.Avalonia.Models;

public class MessageProgress(MessageItem messageItem)
{
    public void Report(string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            messageItem.Message = message;
        });
    }

    public void Report(double? value)
    {
        var progressValue =
            value == null ? null
            : value < 0 ? 0
            : value > 100 ? 100
            : value;

        Dispatcher.UIThread.Post(() =>
        {
            messageItem.ProgressValue = progressValue;
        });
    }

    public void Report(string message, double? value)
    {
        var progressValue =
            value == null ? null
            : value < 0 ? 0
            : value > 100 ? 100
            : value;
        Dispatcher.UIThread.Post(() =>
        {
            messageItem.Message = message;
            messageItem.ProgressValue = progressValue;
        });
    }
}
