using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Message.Avalonia.Controls;
using Message.Avalonia.Models;

namespace Message.Avalonia.Tests;

public class MessageManagerAndProgressTests : IClassFixture<AvaloniaHeadlessFixture>
{
    private readonly AvaloniaHeadlessFixture _fixture;

    public MessageManagerAndProgressTests(AvaloniaHeadlessFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CreateMessage_UsesManagerDefaults()
    {
        _fixture.Session.Dispatch(() =>
        {
            MessageHost.HostList.Clear();

            var host = new MessageHost { HostId = "manager-defaults-host" };
            var manager = new MessageManager
            {
                HostId = "manager-defaults-host",
                Duration = TimeSpan.FromSeconds(5),
            };

            manager.CreateMessage().WithMessage("hello").ShowError();

            var item = DequeueSinglePendingItem(host);
            Assert.Equal(TimeSpan.FromSeconds(5), item.Duration);
            Assert.Equal(MessageType.Error, item.Type);
            Assert.Equal("hello", item.Message);
        }, CancellationToken.None);
    }

    [Fact]
    public void ShowInformationMessage_WithOptions_AppliesHostAndVisualOptions()
    {
        _fixture.Session.Dispatch(() =>
        {
            MessageHost.HostList.Clear();

            var defaultHost = new MessageHost { HostId = "default-host" };
            var targetHost = new MessageHost { HostId = "target-host" };
            var manager = new MessageManager { HostId = "default-host" };

            var options = new MessageOptions
            {
                HostId = "target-host",
                Title = "custom-title",
                Content = "custom-content",
                Duration = TimeSpan.FromSeconds(3),
                HideClose = true,
                HideIcon = true,
            };

            manager.ShowInformationMessage("info-message", options);

            var targetItem = DequeueSinglePendingItem(targetHost);
            Assert.Equal("custom-title", targetItem.Title);
            Assert.Equal("info-message", targetItem.Message);
            Assert.Equal("custom-content", targetItem.Content);
            Assert.Equal(TimeSpan.FromSeconds(3), targetItem.Duration);
            Assert.Equal(MessageType.Information, targetItem.Type);
            Assert.False(targetItem.ShowClose);
            Assert.False(targetItem.ShowIcon);

            Assert.False(TryDequeuePendingItem(defaultHost, out _));
        }, CancellationToken.None);
    }

    [Fact]
    public async Task ProgressMessage_CompletesAndCloses()
    {
        await _fixture.Session.Dispatch(async () =>
        {
            MessageHost.HostList.Clear();
            var host = new MessageHost { HostId = "progress-host" };

            var builder = new ProgressMessageBuilder()
                .WithHost("progress-host")
                .WithTitle("working")
                .HideIcon()
                .WithProgress(async progress =>
                {
                    progress.Report("step", 42);
                    await Task.Delay(20);
                });

            builder.ShowSuccess();

            var item = DequeueSinglePendingItem(host);
            Assert.True(item.IsProgress);
            Assert.False(item.ShowClose);
            Assert.False(item.ShowIcon);
            Assert.Equal("working", item.Title);
            Assert.Equal(MessageType.Success, item.Type);

            await WaitUntilAsync(() => item.ProgressValue == 42, timeoutMs: 3000);
            await WaitUntilAsync(() => item.IsClosing, timeoutMs: 3000);
            Assert.Equal(42, item.ProgressValue);

            return true;
        }, CancellationToken.None);
    }

    [Fact]
    public async Task ProgressMessage_CancelableTask_ReceivesCancellation()
    {
        await _fixture.Session.Dispatch(async () =>
        {
            MessageHost.HostList.Clear();
            var host = new MessageHost { HostId = "cancel-host" };
            var canceledObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var builder = new ProgressMessageBuilder()
                .WithHost("cancel-host")
                .WithProgress(async (_, token) =>
                {
                    try
                    {
                        await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    }
                    catch (OperationCanceledException)
                    {
                        canceledObserved.TrySetResult(true);
                    }
                });

            builder.ShowWarning();

            var item = DequeueSinglePendingItem(host);
            Assert.NotNull(item.ProgressTokenSource);
            Assert.True(item.ProgressTokenSource!.Token.CanBeCanceled);

            item.ProgressTokenSource.Cancel();
            var completed = await Task.WhenAny(canceledObserved.Task, Task.Delay(3000));
            Assert.Same(canceledObserved.Task, completed);
            Assert.True(canceledObserved.Task.Result);

            return true;
        }, CancellationToken.None);
    }

    [Fact]
    public async Task DismissAll_ClosesOnlyRequestedHostMessages()
    {
        await _fixture.Session.Dispatch(async () =>
        {
            MessageHost.HostList.Clear();
            var hostA = new MessageHost { HostId = "host-a" };
            var hostB = new MessageHost { HostId = "host-b" };

            var panelA = AttachItemsPanel(hostA);
            var panelB = AttachItemsPanel(hostB);

            var a1 = new MessageItem();
            var a2 = new MessageItem();
            var b1 = new MessageItem();

            panelA.Children.Add(a1);
            panelA.Children.Add(a2);
            panelB.Children.Add(b1);

            var manager = new MessageManager();
            manager.DismissAll("host-a");

            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            Assert.True(a1.IsClosing);
            Assert.True(a2.IsClosing);
            Assert.False(b1.IsClosing);

            return true;
        }, CancellationToken.None);
    }

    private static MessageItem DequeueSinglePendingItem(MessageHost host)
    {
        var queue = GetPendingQueue(host);
        Assert.True(queue.TryDequeue(out var item));
        Assert.NotNull(item);
        Assert.False(queue.TryDequeue(out _));
        return item!;
    }

    private static bool TryDequeuePendingItem(MessageHost host, out MessageItem? item)
    {
        var queue = GetPendingQueue(host);
        return queue.TryDequeue(out item);
    }

    private static ConcurrentQueue<MessageItem> GetPendingQueue(MessageHost host)
    {
        var field = typeof(MessageHost).GetField("_pendingItemsQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        return Assert.IsType<ConcurrentQueue<MessageItem>>(field!.GetValue(host));
    }

    private static Panel AttachItemsPanel(MessageHost host)
    {
        var field = typeof(MessageHost).GetField("_itemsPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);

        var panelType = field!.FieldType;
        var panel = Assert.IsAssignableFrom<Panel>(Activator.CreateInstance(panelType));
        field.SetValue(host, panel);
        return panel;
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, int timeoutMs)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            if (predicate())
                return;

            await Task.Delay(20);
        }

        Assert.True(predicate(), "Condition was not met before timeout.");
    }
}
