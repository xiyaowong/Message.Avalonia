# Message.Avalonia

[![NuGet](https://img.shields.io/nuget/v/Message.Avalonia.svg)](https://www.nuget.org/packages/Message.Avalonia)  
[![NuGet](https://img.shields.io/nuget/dt/Message.Avalonia.svg)](https://www.nuget.org/packages/Message.Avalonia)

A lightweight and easy-to-use message/notification/toast library for Avalonia UI.

https://github.com/user-attachments/assets/64f8d668-5c5d-4a76-873c-854a06691aec

## Getting Started

> [!Important]
> The documentation is minimal, but the demo project showcases all available features. This library is designed to be
> very developer-friendly.

### 1. Install the NuGet package

```powershell
Install-Package Message.Avalonia
```

### 2. Add the required styles to your `App.xaml`

```xaml
<Application ...
             xmlns:msg="https://github.com/xiyaowong/Message.Avalonia"
             ...>

    <Application.Styles>
        <msg:Theme />
    </Application.Styles>
</Application>
```

### 3. Add the `MessageHost` to your UI layout

```xaml
<Panel>
    <!-- Your main UI content -->
    <msg:MessageHost />
    <msg:MessageHost HostId="Another" Position="BottomCenter" />
</Panel>
```

- `HostId` is optional. If omitted, the default host will be used.
- You can skip defining `MessageHost` entirely — a default host will be created automatically if needed.

### 4. Show messages using `MessageManager`

```csharp
MessageManager.Default.ShowInfomationMessage("Hello");
```

## Resources

- [Source code on GitHub](https://github.com/xiyaowong/Message.Avalonia)

## License

MIT
