# Message Markdown Avalonia

Add markdown support for `Message.Avalonia`. Based on [Avalonia.MarkdownViewer](https://github.com/LITTOMA/Avalonia.MarkdownViewer)

```csharp
using Messasge.Markdown.Avalonia;

MessageManager.Default
    .CreateMessage()
    .WithMarkdown("# Hello World")
    .HideIcon()
    .ShowInfo();
```