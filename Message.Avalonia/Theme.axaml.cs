using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Message.Avalonia;

public class Theme : Styles
{
    public Theme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
