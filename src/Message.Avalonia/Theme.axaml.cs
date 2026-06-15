using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Message.Avalonia;

public partial class Theme : Styles
{
    public Theme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
