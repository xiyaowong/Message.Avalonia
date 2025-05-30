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

        const string sampleMarkdown = """
            ## Headings
            # Header 1
            ## Header 2
            ### Header 3

            ## Emphasis
            Use *italics* or _italics_ for emphasizing text.  
            Use **bold** or __bold__ for stronger emphasis.  
            You can **_combine both_** for extra emphasis.

            ## Lists
            ### Unordered List
            - First item
            - Second item
            - Sub-item A
            - Sub-item B

            ### Ordered List
            1. Step one
            2. Step two
            1. Sub-step A
            2. Sub-step B
            """;
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

            var userWantsIcon = _IconSwitch.IsChecked == true;

            _manager.CreateMessage().WithMarkdown(markdown, hideIcon: !userWantsIcon).ShowInfo();
        };
    }
}
