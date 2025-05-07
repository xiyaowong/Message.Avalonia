using System.Collections.Generic;
using System.Threading.Tasks;
using Message.Avalonia.Models;

namespace Message.Avalonia;

public partial class MessageManager
{
    public void ShowInformationMessage(string message)
    {
        CreateMessage().WithMessage(message).ShowInfo();
    }

    public void ShowInformationMessage(string message, MessageOptions options)
    {
        CreateMessage().WithMessage(message).WithOptions(options).ShowInfo();
    }

    public Task<string?> ShowInformationMessage(string message, params List<string> items)
    {
        var tcs = new TaskCompletionSource<string?>();
        CreateMessage().WithMessage(message).WithActions(items, item => tcs.SetResult(item)).ShowInfo();
        return tcs.Task;
    }

    public Task<string?> ShowInformationMessage(string message, MessageOptions options, params List<string> items)
    {
        var tcs = new TaskCompletionSource<string?>();

        CreateMessage()
            .WithMessage(message)
            .WithActions(items, item => tcs.SetResult(item))
            .WithOptions(options)
            .ShowInfo();

        return tcs.Task;
    }

    public Task<MessageAction?> ShowInformationMessage(string message, params List<MessageAction> items)
    {
        var tcs = new TaskCompletionSource<MessageAction?>();
        CreateMessage().WithMessage(message).WithActions(items, item => tcs.SetResult(item)).ShowInfo();
        return tcs.Task;
    }

    public Task<MessageAction?> ShowInformationMessage(
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
            .ShowInfo();

        return tcs.Task;
    }
}
