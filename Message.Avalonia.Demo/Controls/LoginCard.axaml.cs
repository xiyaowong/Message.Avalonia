using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Message.Avalonia.Demo.Controls;

public record LoginParams(string? UserName, string? Password, bool Remember = false, bool Cancel = false);

public partial class LoginCard : UserControl
{
    public event EventHandler<LoginParams>? Completed;

    public LoginCard()
    {
        InitializeComponent();

        LoginButton.Click += OnButtonClick;
        CancelButton.Click += OnButtonClick;
    }

    private void OnButtonClick(object? sender, RoutedEventArgs _)
    {
        if (sender is Button btn)
        {
            Completed?.Invoke(
                this,
                btn == LoginButton
                    ? new LoginParams(UsernameBox.Text, PasswordBox.Text, RememberBox.IsChecked == true)
                    : new LoginParams(null, null, false, true)
            );
        }
    }
}
