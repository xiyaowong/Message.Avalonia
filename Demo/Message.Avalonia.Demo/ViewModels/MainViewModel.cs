using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Message.Avalonia.Demo.Controls;
using Message.Avalonia.Models;

namespace Message.Avalonia.Demo.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly MessageManager messageManager = new() { Duration = TimeSpan.FromSeconds(3) };
    private readonly MessageManager anotherManager = new() { Duration = TimeSpan.FromSeconds(3), HostId = "Another" };

    [ObservableProperty]
    private TextDocument _sourceDocument = new();

    public string Code
    {
        set => SourceDocument.Text = value;
    }

    [RelayCommand]
    private static void CopyCode(TextArea textArea) => ApplicationCommands.Copy.Execute(null, textArea);

    [RelayCommand]
    private void BuilderDefaultManager()
    {
        MessageManager.Default.CreateMessage().WithDuration(3).WithMessage("Hello").ShowInfo();
        MessageManager.Default.CreateMessage().WithHost("Another").WithDuration(3).WithMessage("Hello").ShowInfo();
        Code = """
            MessageManager.Default.CreateMessage().WithDuration(3).WithMessage("Hello").ShowInfo();
            MessageManager.Default.CreateMessage().WithHost("Another").WithDuration(3).WithMessage("Hello").ShowInfo();
            """;
    }

    [RelayCommand]
    private void BuilderHello()
    {
        messageManager.CreateMessage().WithMessage("Hello").ShowInfo();
        Code = """
            messageManager.CreateMessage().WithMessage("Hello").ShowInfo();
            """;
    }

    [RelayCommand]
    private void BuilderImage()
    {
        messageManager
            .CreateMessage()
            .HideIcon()
            .WithContent(
                new Image
                {
                    Source = new Bitmap(
                        AssetLoader.Open(
                            new Uri($"avares://{Assembly.GetExecutingAssembly().FullName}/Assets/avalonia-logo.ico")
                        )
                    ),
                    Width = 128,
                    Height = 128,
                }
            )
            .ShowInfo();
        Code = """
            messageManager
                .CreateMessage()
                .HideIcon()
                .WithContent(
                    new Image
                    {
                        Source = new Bitmap(
                            AssetLoader.Open(
                                new Uri($"avares://{Assembly.GetExecutingAssembly().FullName}/Assets/avalonia-logo.ico")
                            )
                        ),
                        Width = 128,
                        Height = 128,
                    }
                )
                .ShowInfo();
            """;
    }

    [RelayCommand]
    private async Task ChooseColor()
    {
        Code = """
            // These classes are defined in App.axaml
            var action = await messageManager.ShowInformationMessage(
                "Please choose a color:",
                new MessageAction("Red", ["red"]),
                new MessageAction("Green", ["green"]),
                new MessageAction("Blue", ["blue"])
            );
            var color = action?.Title;
            if (color == null)
                return;

            anotherManager
                .CreateMessage()
                .HideIcon()
                .WithContent(new TextBlock { Text = $"OK, {color}.", Foreground = new SolidColorBrush(Color.Parse(color)) })
                .ShowInfo();
            """;

        // These classes are defined in App.axaml
        var action = await messageManager.ShowInformationMessage(
            "Please choose a color:",
            new MessageAction("Red", ["red"]),
            new MessageAction("Green", ["green"]),
            new MessageAction("Blue", ["blue"])
        );
        var color = action?.Title;
        if (color == null)
            return;

        anotherManager
            .CreateMessage()
            .HideIcon()
            .WithContent(new TextBlock { Text = $"OK, {color}.", Foreground = new SolidColorBrush(Color.Parse(color)) })
            .ShowInfo();
    }

    [RelayCommand]
    private void YesOrNo()
    {
        messageManager
            .ShowInformationMessage("Yes or No ?", "Yes", "No")
            .ContinueWith(ret =>
            {
                var info = ret.Result switch
                {
                    "Yes" => "You clicked Yes",
                    "No" => "You clicked No",
                    _ => "You didn't click anything",
                };
                anotherManager.ShowInformationMessage(info);
            });
        Code = """
            messageManager
                .ShowInformationMessage("Yes or No ?", "Yes", "No")
                .ContinueWith(ret =>
                {
                    var info = ret.Result switch
                    {
                        "Yes" => "You clicked Yes",
                        "No" => "You clicked No",
                        _ => "You didn't click anything",
                    };
                    anotherManager.ShowInformationMessage(info);
                });
            """;
    }

    [RelayCommand]
    private void CustomActionStyle()
    {
        // The default one doesn't have duration and auto close
        MessageManager
            .Default.CreateMessage()
            .HideIcon()
            .WithMessage("Do you want to continue?")
            .WithActions(
                // The class `accent` comes from FluentAvalonia. Selector `Button.accent`.
                [new MessageAction("Yes", ["accent"]), new MessageAction("No")],
                action =>
                {
                    var info = action?.Title switch
                    {
                        "Yes" => "Yes!",
                        "No" => "No.",
                        _ => "???",
                    };
                    anotherManager.ShowInformationMessage(info);
                }
            )
            .ShowInfo();
        Code = """
             // The default one doesn't have duration and auto close
             MessageManager
                 .Default.CreateMessage()
                 .HideIcon()
                 .WithMessage("Do you want to continue?")
                 .WithActions(
                     // The class `accent` comes from FluentAvalonia. Selector `Button.accent`.
                     [new MessageAction("Yes", ["accent"]), new MessageAction("No", [])],
                     action =>
                     {
                         var info = action?.Title switch
                         {
                             "Yes" => "Yes!",
                             "No" => "No.",
                             _ => "???",
                         };
                         anotherManager.ShowInformationMessage(info);
                     }
                 )
                 .ShowInfo();
            """;
    }

    [RelayCommand]
    private void ExitApplication()
    {
        messageManager.ShowWarningMessage(
            "Are you sure you want to exit?",
            new MessageAction("Exit", ["accent"], () => Environment.Exit(0)),
            new MessageAction("Cancel")
        );
        Code = """
            messageManager.ShowWarningMessage(
                "Are you sure you want to exit?",
                new MessageAction("Exit", ["accent"], () => Environment.Exit(0)),
                new MessageAction("Cancel")
            );
            """;
    }

    [RelayCommand]
    private void TestType(string type)
    {
        switch (type)
        {
            case "info-title":
            {
                messageManager.ShowInformationMessage("This is a info message.", new MessageOptions { Title = "Info" });
                Code = """
                    messageManager.ShowInformationMessage(
                        "This is a info message.",
                        new MessageOptions { Title = "Info" }
                    );
                    """;
                break;
            }
            case "warning-title":
            {
                messageManager.ShowWarningMessage(
                    "This is a warning message.",
                    new MessageOptions { Title = "Warning" }
                );
                Code = """
                    messageManager.ShowWarningMessage(
                        "This is a warning message.",
                        new MessageOptions { Title = "Warning" }
                    );
                    """;
                break;
            }
            case "error-title":
            {
                messageManager.ShowErrorMessage("This is a error message.", new MessageOptions { Title = "Error" });
                Code = """
                    messageManager.ShowErrorMessage(
                        "This is a error message.",
                        new MessageOptions { Title = "Error" }
                    );
                    """;
                break;
            }
            case "success-title":
            {
                messageManager.ShowSuccessMessage(
                    "This is a success message.",
                    new MessageOptions { Title = "Success" }
                );
                Code = """
                    messageManager.ShowSuccessMessage(
                        "This is a success message.",
                        new MessageOptions { Title = "Success" }
                    );
                    """;
                break;
            }
            case "info":
            {
                messageManager.ShowInformationMessage("This is a info message.");
                Code = """
                    messageManager.ShowInformationMessage("This is a info message.");
                    """;
                break;
            }
            case "warning":
            {
                messageManager.ShowWarningMessage("This is a warning message.");
                Code = """
                    messageManager.ShowWarningMessage("This is a warning message.");
                    """;
                break;
            }
            case "error":
            {
                messageManager.ShowErrorMessage("This is a error message.");
                Code = """
                    messageManager.ShowErrorMessage("This is a error message.");
                    """;
                break;
            }
            case "success":
            {
                messageManager.ShowSuccessMessage("This is a success message.");
                Code = """
                    messageManager.ShowSuccessMessage("This is a success message.");
                    """;
                break;
            }
        }
    }

    [RelayCommand]
    private void ShowLoginCard()
    {
        var msg = MessageManager.Default.CreateMessage();

        var card = new LoginCard();
        card.Completed += (_, @params) =>
        {
            msg.Dismiss();
            if (@params.Cancel)
            {
                anotherManager.ShowInformationMessage("User cancelled login");
            }
            else
            {
                anotherManager
                    .CreateMessage()
                    .WithTitle("Login Parameters")
                    .WithMessage(
                        $"""
                        UserName: {@params.UserName}
                        Password: {@params.Password}
                        Remember: {(@params.Remember ? "Yes" : "No")}
                        """
                    )
                    .ShowSuccess();
            }
        };

        // Hide the close button, so the user must click on the card buttons
        msg.HideIcon().HideClose().WithContent(card).ShowInfo();

        Code = """"
            var msg = MessageManager.Default.CreateMessage();

            var card = new LoginCard();
            card.Completed += (_, @params) =>
            {
                msg.Dismiss();
                if (@params.Cancel)
                {
                    anotherManager.ShowInformationMessage("User cancelled login");
                }
                else
                {
                    anotherManager
                        .CreateMessage()
                        .WithTitle("Login Parameters")
                        .WithMessage(
                            $"""
                            UserName: {@params.UserName}
                            Password: {@params.Password}
                            Remember: {(@params.Remember ? "Yes" : "No")}
                            """
                        )
                        .ShowSuccess();
                }
            };

            // Hide the close button, so the user must click on the card buttons
            msg.HideIcon().HideClose().WithContent(card).ShowInfo();

            """";
    }

    [RelayCommand]
    private void DownloadProgress()
    {
        messageManager
            .CreateProgress()
            .WithTitle("Download dependencies")
            .WithProgress(async progress =>
            {
                progress.Report(0);

                await Task.Delay(500);

                var random = new Random();
                for (var i = 0; i < 100; i++)
                {
                    await Task.Delay(random.Next(2, 50));
                    progress.Report($"Downloading {i} %", i);
                }

                progress.Report("Download complete ✅", 100);
                await Task.Delay(800);
            })
            .ShowInfo();
        Code = """
            messageManager
                .CreateProgress()
                .WithTitle("Download dependencies")
                .WithProgress(async progress =>
                {
                    progress.Report(0);

                    await Task.Delay(500);

                    var random = new Random();
                    for (var i = 0; i < 100; i++)
                    {
                        await Task.Delay(random.Next(2, 50));
                        progress.Report($"Downloading {i} %", i);
                    }

                    progress.Report("Download complete ✅", 100);
                    await Task.Delay(800);
                })
                .ShowInfo();
            """;
    }

    [RelayCommand]
    private void CancelableProgress()
    {
        messageManager
            .CreateProgress()
            .WithProgress(
                async (progress, token) =>
                {
                    try
                    {
                        if (!token.IsCancellationRequested)
                        {
                            progress.Report("🚀 Initializing build environment...");
                            await Task.Delay(1000, token);
                        }

                        if (!token.IsCancellationRequested)
                        {
                            progress.Report("📦 Installing dependencies...");
                            await Task.Delay(1500, token);
                        }

                        if (!token.IsCancellationRequested)
                        {
                            progress.Report("🔧 Running build scripts...");
                            await Task.Delay(1200, token);
                        }

                        if (!token.IsCancellationRequested)
                        {
                            progress.Report("✅ Build completed, generating artifacts...");
                            await Task.Delay(1000, token);
                        }

                        if (!token.IsCancellationRequested)
                        {
                            progress.Report("📤 Uploading artifacts...");
                            await Task.Delay(1500, token);
                        }
                    }
                    catch (TaskCanceledException) { }

                    progress.Report(
                        token.IsCancellationRequested
                            ? "❌ Operation canceled."
                            : "🎉 Operation completed successfully.",
                        token.IsCancellationRequested ? 0 : 100
                    );

                    await Task.Delay(1000, CancellationToken.None);
                }
            )
            .HideIcon()
            .ShowInfo();

        Code = """
            messageManager
              .CreateProgress()
              .WithProgress(
                  async (progress, token) =>
                  {
                      try
                      {
                          if (!token.IsCancellationRequested)
                          {
                              progress.Report("🚀 Initializing build environment...");
                              await Task.Delay(1000, token);
                          }

                          if (!token.IsCancellationRequested)
                          {
                              progress.Report("📦 Installing dependencies...");
                              await Task.Delay(1500, token);
                          }

                          if (!token.IsCancellationRequested)
                          {
                              progress.Report("🔧 Running build scripts...");
                              await Task.Delay(1200, token);
                          }

                          if (!token.IsCancellationRequested)
                          {
                              progress.Report("✅ Build completed, generating artifacts...");
                              await Task.Delay(1000, token);
                          }

                          if (!token.IsCancellationRequested)
                          {
                              progress.Report("📤 Uploading artifacts...");
                              await Task.Delay(1500, token);
                          }
                      }
                      catch (TaskCanceledException) { }

                      progress.Report(
                          token.IsCancellationRequested
                              ? "❌ Operation canceled."
                              : "🎉 Operation completed successfully.",
                          token.IsCancellationRequested ? 0 : 100
                      );

                      await Task.Delay(1000, CancellationToken.None);
                  }
              )
              .HideIcon()
              .ShowInfo();
            """;
    }
}
