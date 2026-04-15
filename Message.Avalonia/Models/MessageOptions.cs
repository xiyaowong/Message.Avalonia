using System;

namespace Message.Avalonia.Models;

public struct MessageOptions
{
    /// <summary>
    /// The id of the message host to display the message in.
    /// </summary>
    public string? HostId { get; set; }

    /// <summary>
    /// The title of the message.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The custom content of the message.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// Automatically close the message after a certain duration.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Hide the icon of the message.
    /// </summary>
    public bool? HideIcon { get; set; }

    /// <summary>
    /// Hide the close button of the message.
    /// </summary>
    public bool? HideClose { get; set; }
}
