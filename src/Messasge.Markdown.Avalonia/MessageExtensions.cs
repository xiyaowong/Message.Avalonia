using Message.Avalonia;

namespace Messasge.Markdown.Avalonia;

/// <summary>
/// Extensions for Message.Avalonia components to support Markdown content.
/// </summary>
public static class MessageExtensions
{
    /// <summary>
    /// Renders a Markdown string as a message and shows it as an info message.
    /// </summary>
    /// <param name="manager"><see cref="MessageManager"/></param>
    /// <param name="markdown">The markdown string to render.</param>
    public static void ShowMarkdown(this MessageManager manager, string markdown)
    {
        manager.CreateMessage().WithMarkdown(markdown).ShowInfo();
    }

    /// <summary>
    /// Renders a Markdown string as a message content.
    /// </summary>
    /// <param name="builder"><see cref="MessageBuilder"/></param>
    /// <param name="markdown">The markdown string to render.</param>
    /// <param name="hideIcon">Hides the icon, defaults to true. Because the icon does not look good with markdown display.</param>
    /// <returns></returns>
    /// <remarks>Do not call <see cref="MessageBuilder.WithContent"/> again after this.</remarks>
    public static MessageBuilder WithMarkdown(this MessageBuilder builder, string markdown, bool hideIcon = true)
    {
        if (hideIcon)
        {
            builder.HideIcon();
        }
        return builder.WithContent(new MarkdownViewer.Core.Controls.MarkdownViewer() { MarkdownText = markdown });
    }
}
