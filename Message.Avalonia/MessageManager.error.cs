using System.Collections.Generic;
using System.Threading.Tasks;
using Message.Avalonia.Models;

namespace Message.Avalonia;

public partial class MessageManager
{
    public void ShowErrorMessage(string message)
    {
        CreateMessage().WithMessage(message).ShowError();
    }

    public void ShowErrorMessage(string message, MessageOptions options)
    {
        CreateMessage().WithMessage(message).WithOptions(options).ShowError();
    }

    public Task<string?> ShowErrorMessage(string message, params List<string> items)
    {
        var tcs = new TaskCompletionSource<string?>();
        CreateMessage().WithMessage(message).WithActions(items, item => tcs.SetResult(item)).ShowError();
        return tcs.Task;
    }

    public Task<string?> ShowErrorMessage(string message, MessageOptions options, params List<string> items)
    {
        var tcs = new TaskCompletionSource<string?>();

        CreateMessage()
            .WithMessage(message)
            .WithActions(items, item => tcs.SetResult(item))
            .WithOptions(options)
            .ShowError();

        return tcs.Task;
    }

    public Task<MessageAction?> ShowErrorMessage(string message, params List<MessageAction> items)
    {
        var tcs = new TaskCompletionSource<MessageAction?>();
        CreateMessage().WithMessage(message).WithActions(items, item => tcs.SetResult(item)).ShowError();
        return tcs.Task;
    }

    public Task<MessageAction?> ShowErrorMessage(
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
            .ShowError();

        return tcs.Task;
    }
}
