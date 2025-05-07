using System;

namespace Message.Avalonia.Models;

public struct MessageOptions
{
    public string? HostId { get; set; }
    public string? Title { get; set; }

    public object? Content { get; set; }

    public TimeSpan? Duration { get; set; }

    public bool? HideIcon { get; set; }

    public bool? HideClose { get; set; }
}
