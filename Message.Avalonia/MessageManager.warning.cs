using System.Collections.Generic;
using System.Threading.Tasks;
using Message.Avalonia.Models;

namespace Message.Avalonia;

public partial class MessageManager
{
    public void ShowWarningMessage(string message)
    {
        CreateMessage().WithMessage(message).ShowWarning();
    }

    public void ShowWarningMessage(string message, MessageOptions options)
    {
        CreateMessage().WithMessage(message).WithOptions(options).ShowWarning();
    }

    public Task<string?> ShowWarningMessage(string message, params List<string> items)
    {
        var tcs = new TaskCompletionSource<string?>();
        CreateMessage().WithMessage(message).WithActions(items, item => tcs.SetResult(item)).ShowWarning();
        return tcs.Task;
    }

    public Task<string?> ShowWarningMessage(string message, MessageOptions options, params List<string> items)
    {
        var tcs = new TaskCompletionSource<string?>();

        CreateMessage()
            .WithMessage(message)
            .WithActions(items, item => tcs.SetResult(item))
            .WithOptions(options)
            .ShowWarning();

        return tcs.Task;
    }

    public Task<MessageAction?> ShowWarningMessage(string message, params List<MessageAction> items)
    {
        var tcs = new TaskCompletionSource<MessageAction?>();
        CreateMessage().WithMessage(message).WithActions(items, item => tcs.SetResult(item)).ShowWarning();
        return tcs.Task;
    }

    public Task<MessageAction?> ShowWarningMessage(
        string message,
        MessageOptions options,
        params List<MessageAction> items
    )
    {
        var tcs = new TaskCompletionSource<MessageAction?>();

        CreateMessage()
            .WithMessage(message)
            .WithActions(items, item => tcs.SetResult(item))
            .WithOptions(options)
            .ShowWarning();

        return tcs.Task;
    }
}
