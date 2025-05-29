using Message.Avalonia;

namespace Messasge.Markdown.Avalonia;

/// <summary>
/// Extensions for Message.Avalonia components to support Markdown content.
/// </summary>
public static class MessageExtensions
{
    /// <summary>
    /// Renders a Markdown string as a message content.
    /// </summary>
    /// <param name="builder"><see cref="MessageBuilder"/></param>
    /// <param name="markdown">The markdown string to render.</param>
    /// <returns></returns>
    public static MessageBuilder WithMarkdown(this MessageBuilder builder, string markdown) =>
        builder.WithContent(new MarkdownViewer.Core.Controls.MarkdownViewer() { MarkdownText = markdown });
}
