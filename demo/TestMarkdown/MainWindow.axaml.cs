using Avalonia.Controls;
using Message.Avalonia;
using Messasge.Markdown.Avalonia;

namespace TestMarkdown;

public partial class MainWindow : Window
{
    private MessageManager _manager => MessageManager.Default;

    public MainWindow()
    {
        InitializeComponent();

        // _manager.Duration = TimeSpan.FromSeconds(3);

        const string sampleMarkdown =
            @"# Markdown Editor Example

## Basic Formatting

This is a **bold** text example, this is *italic* text.


### Roadmap
- [x] Basic Markdown rendering
- [x] Image handling and caching
- [x] Link handling
- [ ] Code syntax highlighting
- [ ] Dark/Light theme support
- [ ] Custom styling options


### List Example

- Item 1
- Item 2
  - Sub-item 2.1
  - Sub-item 2.2
- Item 3

### Code Example

```csharp
public class Example
{
    public void HelloWorld()
    {
        Console.WriteLine(""Hello, World!"");
    }
}
```

### Links and Images

Link: [Avalonia UI](https://avaloniaui.net/)

Image: ![Avalonia Logo](https://avatars.githubusercontent.com/u/14075148?s=200&v=4)

### Table

| Header1 | Header2 | Header3 |
|---------|---------|---------|
| Normal text | **Bold text** | `Code text` |
| [Link](http://example.com) | *Italic text* | Other content |
";
        _MarkdownBox.Text = sampleMarkdown;

        _DismissButton.Click += delegate
        {
            _manager.DismissAll();
        };

        _ShowButton.Click += delegate
        {
            var markdown = _MarkdownBox.Text;
            if (string.IsNullOrEmpty(markdown))
                return;

            var builder = _manager.CreateMessage().WithMarkdown(markdown);

            if (_IconSwitch.IsChecked != true)
                builder.HideIcon();

            builder.ShowInfo();
        };
    }
}
