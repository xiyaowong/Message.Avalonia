using System;
using System.Collections.Generic;

namespace Message.Avalonia.Models;

public class MessageAction
{
    /// <summary>
    /// Label of the action button.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Additional classes to be added to the action button.
    /// </summary>
    public List<string> Classes = [];

    /// <summary>
    /// Callback to be executed when the action button is clicked.
    /// </summary>
    public Action? Callback { get; set; }

    public MessageAction() { }

    /// <summary>
    /// Action button for the message.
    /// </summary>
    /// <param name="title">The <see cref="Title"/></param>
    /// <param name="classes">The <see cref="Classes"/></param>
    /// <param name="callback">The <see cref="Callback"/></param>
    public MessageAction(string title, List<string>? classes = null, Action? callback = null)
    {
        Title = title;

        if (classes is not null)
        {
            Classes = classes;
        }

        if (callback is not null)
        {
            Callback = callback;
        }
    }
}
