using System;
using System.Collections.Concurrent;
using System.Threading;
using Avalonia;
using Avalonia.Threading;
using Message.Avalonia.Controls;
using Message.Avalonia.Models;

namespace Message.Avalonia.Tests;

public class MessageCoreTests : IClassFixture<AvaloniaHeadlessFixture>
{
    private readonly AvaloniaHeadlessFixture _fixture;

    public MessageCoreTests(AvaloniaHeadlessFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ShowInfo_MapsBuilderValuesToMessageItem()
    {
        _fixture.Session.Dispatch(() =>
        {
            MessageHost.HostList.Clear();

            var host = new MessageHost { HostId = "test-host" };
            var builder = new MessageBuilder()
                .WithHost("test-host")
                .WithTitle("Title")
                .WithMessage("Body")
                .WithDuration(2u)
                .HideClose()
                .HideIcon();

            builder.ShowInfo();

            var item = DequeueSinglePendingItem(host);
            Assert.Equal("Title", item.Title);
            Assert.Equal("Body", item.Message);
            Assert.Equal(TimeSpan.FromSeconds(2), item.Duration);
            Assert.Equal(MessageType.Information, item.Type);
            Assert.False(item.ShowClose);
            Assert.False(item.ShowIcon);
        }, CancellationToken.None);
    }

    [Fact]
    public void Show_CalledTwice_Throws()
    {
        _fixture.Session.Dispatch(() =>
        {
            MessageHost.HostList.Clear();

            _ = new MessageHost { HostId = "twice-host" };
            var builder = new MessageBuilder().WithHost("twice-host");

            builder.ShowSuccess();

            var ex = Assert.Throws<InvalidOperationException>(() => builder.ShowSuccess());
            Assert.Contains("already shown", ex.Message, StringComparison.OrdinalIgnoreCase);
        }, CancellationToken.None);
    }

    [Fact]
    public void WithBorder_NormalizesZeroValuesToNegativeOne()
    {
        _fixture.Session.Dispatch(() =>
        {
            MessageHost.HostList.Clear();

            var host = new MessageHost { HostId = "border-host" };
            var builder = new MessageBuilder()
                .WithHost("border-host")
                .WithBorder(thickness: new Thickness(0), cornerRadius: new CornerRadius(0));

            builder.ShowWarning();

            var item = DequeueSinglePendingItem(host);
            Assert.Equal(Thickness.Parse("-1"), item.BorderThickness);
            Assert.Equal(CornerRadius.Parse("-1"), item.CornerRadius);
        }, CancellationToken.None);
    }

    [Fact]
    public async Task MessageProgress_Report_ClampsValues()
    {
        await _fixture.Session.Dispatch(async () =>
        {
            var item = new MessageItem();
            var progress = new MessageProgress(item);

            progress.Report(-10);
            await WaitUntilAsync(() => item.ProgressValue == 0, timeoutMs: 1000);
            Assert.Equal(0, item.ProgressValue);

            progress.Report(150);
            await WaitUntilAsync(() => item.ProgressValue == 100, timeoutMs: 1000);
            Assert.Equal(100, item.ProgressValue);

            progress.Report("loading", null);
            await WaitUntilAsync(() => item.Message == "loading" && item.ProgressValue == null, timeoutMs: 1000);
            Assert.Equal("loading", item.Message);
            Assert.Null(item.ProgressValue);

            return true;
        }, CancellationToken.None);
    }

    private static MessageItem DequeueSinglePendingItem(MessageHost host)
    {
        var queueField = typeof(MessageHost).GetField("_pendingItemsQueue",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(queueField);

        var queue = Assert.IsType<ConcurrentQueue<MessageItem>>(queueField!.GetValue(host));
        Assert.True(queue.TryDequeue(out var item));
        Assert.NotNull(item);
        Assert.False(queue.TryDequeue(out _));
        return item!;
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, int timeoutMs)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < TimeSpan.FromMilliseconds(timeoutMs))
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            if (predicate())
                return;

            await Task.Delay(20);
        }

        Assert.True(predicate(), "Condition was not met before timeout.");
    }
}
