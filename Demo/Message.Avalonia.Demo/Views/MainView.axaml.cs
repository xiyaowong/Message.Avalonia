using Avalonia.Controls;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using Message.Avalonia.Models;
using TextMateSharp.Grammars;

namespace Message.Avalonia.Demo.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        var _textEditor = this.FindControl<TextEditor>("Editor");
        var _registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        var _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);
        _textMateInstallation.SetGrammar(
            _registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".cs").Id)
        );

        PositionBox.SelectionChanged += delegate
        {
            var item = PositionBox.SelectedItem as ComboBoxItem;
            if (item?.Content is MessagePosition pos)
            {
                DefaultHost.Position = pos;
            }
        };
        ToggleThemeButton.Click += delegate
        {
            // RequestedThemeVariant =
            //     ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
        };
    }
}