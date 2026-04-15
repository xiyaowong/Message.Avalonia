using Avalonia;
using Avalonia.Headless;

namespace Message.Avalonia.Tests;

public sealed class AvaloniaHeadlessFixture : IDisposable
{
    public HeadlessUnitTestSession Session { get; } = HeadlessUnitTestSession.StartNew(typeof(TestApplication));

    public void Dispose()
    {
        Session.Dispose();
    }
}

public sealed class TestApplication : Application;
