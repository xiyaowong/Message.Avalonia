using System;
using Message.Avalonia.UI.Host;

namespace Message.Avalonia;

public partial class MessageManager
{
    public string HostId { get; set; } = MessageHost.DEFAULT_HOST_ID;
    public TimeSpan Duration { get; set; } = TimeSpan.MaxValue;

    public static MessageManager Default { get; } = new();

    public MessageBuilder CreateMessage() => new MessageBuilder().WithHost(HostId).WithDuration(Duration);

    public ProgressMessageBuilder CreateProgress() => new ProgressMessageBuilder().WithHost(HostId);
}
