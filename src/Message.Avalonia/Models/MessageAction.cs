using System;
using System.Collections.Generic;

namespace Message.Avalonia.Models;

public class MessageAction
{
    public string Title { get; set; } = null!;

    public List<string> Classes = [];

    public Action? Callback { get; set; }

    public MessageAction() { }

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
