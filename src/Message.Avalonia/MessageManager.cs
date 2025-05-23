using System;
using Avalonia.Threading;
using Message.Avalonia.Controls;

namespace Message.Avalonia;

public partial class MessageManager
{
    /// <summary>
    /// Default host id for the message.
    /// </summary>
    public string HostId { get; set; } = MessageHost.DEFAULT_HOST_ID;

    /// <summary>
    /// Default duration for the message.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.MaxValue;

    /// <summary>
    /// Default instance of the <see cref="MessageManager"/>.
    /// </summary>
    public static MessageManager Default { get; } = new();

    /// <summary>
    /// Create a new message builder.
    /// </summary>
    /// <returns></returns>
    public MessageBuilder CreateMessage() => new MessageBuilder().WithHost(HostId).WithDuration(Duration);

    /// <summary>
    /// Create a new progress message builder.
    /// </summary>
    /// <returns></returns>
    public ProgressMessageBuilder CreateProgress() => new ProgressMessageBuilder().WithHost(HostId);

    /// <summary>
    /// Dismiss all messages with the specified host id.
    /// </summary>
    public void DismissAll(string? hostId = null)
    {
        hostId ??= HostId;

        ArgumentException.ThrowIfNullOrEmpty(hostId);

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var host in MessageHost.HostList)
            {
                if (host.HostId == hostId)
                {
                    host.DismissAll();
                }
            }
        });
    }
}
