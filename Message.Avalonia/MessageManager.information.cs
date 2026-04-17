using System.Collections.Generic;
using System.Threading.Tasks;
using Message.Avalonia.Models;

namespace Message.Avalonia;

public partial class MessageManager
{
    /// <summary>
    /// Shows an information message with the specified message.
    /// </summary>
    /// <param name="message">The information message to display</param>
    public void ShowInformationMessage(string message)
    {
        CreateMessage().WithMessage(message).ShowInfo();
    }

    /// <summary>
    /// Shows an information message with customizable options.
    /// </summary>
    /// <param name="message">The information message to display</param>
    /// <param name="options">Custom options for configuring the message</param>
    public void ShowInformationMessage(string message, MessageOptions options)
    {
        CreateMessage().WithMessage(message).WithOptions(options).ShowInfo();
    }

    /// <summary>
    /// Shows an information message with a list of action buttons.
    /// </summary>
    /// <param name="message">The information message to display</param>
    /// <param name="items">A list of action button labels</param>
    /// <returns>A task that completes with the selected action label, or null if dismissed</returns>
    public Task<string?> ShowInformationMessage(string message, params List<string> items)
    {
        var tcs = new TaskCompletionSource<string?>();
        CreateMessage().WithMessage(message).WithActions(items, item => tcs.SetResult(item)).ShowInfo();
        return tcs.Task;
    }

    /// <summary>
    /// Shows an information message with customizable options and action buttons.
    /// </summary>
    /// <param name="message">The information message to display</param>
    /// <param name="options">Custom options for configuring the message</param>
    /// <param name="items">A list of action button labels</param>
    /// <returns>A task that completes with the selected action label, or null if dismissed</returns>
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

    /// <summary>
    /// Shows an information message with action buttons.
    /// </summary>
    /// <param name="message">The information message to display</param>
    /// <param name="items">A list of <see cref="MessageAction"/> objects defining the available actions</param>
    /// <returns>A task that completes with the selected <see cref="MessageAction"/> or null if dismissed</returns>
    public Task<MessageAction?> ShowInformationMessage(string message, params List<MessageAction> items)
    {
        var tcs = new TaskCompletionSource<MessageAction?>();
        CreateMessage().WithMessage(message).WithActions(items, item => tcs.SetResult(item)).ShowInfo();
        return tcs.Task;
    }

    /// <summary>
    /// Shows an information message with customizable options and action buttons.
    /// </summary>
    /// <param name="message">The information message to display</param>
    /// <param name="options">Custom options for configuring the message</param>
    /// <param name="items">A list of <see cref="MessageAction"/> objects defining the available actions</param>
    /// <returns>A task that completes with the selected <see cref="MessageAction"/> or null if dismissed</returns>
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
