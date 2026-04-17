# Message.Avalonia

[![NuGet](https://img.shields.io/nuget/v/Message.Avalonia.svg)](https://www.nuget.org/packages/Message.Avalonia)  
[![NuGet](https://img.shields.io/nuget/dt/Message.Avalonia.svg)](https://www.nuget.org/packages/Message.Avalonia)

A lightweight and easy-to-use message/notification/toast library for Avalonia UI.

https://github.com/user-attachments/assets/64f8d668-5c5d-4a76-873c-854a06691aec

## Attribution

This repository is a fork of the original project by
[xiyaowong](https://github.com/xiyaowong/Message.Avalonia).

## Repository Structure

- `Message.Avalonia` — library source code
- `Message.Avalonia.Demo` — shared demo application
- `Message.Avalonia.Demo.Desktop` — desktop demo host
- `Message.Avalonia.Demo.Browser` — browser demo host
- `TestAutoCreatedDefaultHost` — host behavior sample app
- `Message.Avalonia.Tests` — automated tests

## Getting Started

> [!Important]
> The documentation is minimal, but
> the [demo](https://github.com/AvionBlock/Message.Avalonia/tree/dev/Message.Avalonia.Demo) project showcases all
> available features. This library is designed to be
> very developer-friendly.

### 1. Install the NuGet package

```powershell
Install-Package AvionBlock.Message.Avalonia
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

## Usage

### Override the default styles

Colors:

```xaml
<!-- Text and icon colors -->
<Color x:Key="MessageInfoColor">
<Color x:Key="MessageSuccessColor">
<Color x:Key="MessageWarningColor">
<Color x:Key="MessageErrorColor">

<!-- Border colors -->
<Color x:Key="MessageInfoBorderColor">
<Color x:Key="MessageSuccessBorderColor">
<Color x:Key="MessageWarningBorderColor">
<Color x:Key="MessageErrorBorderColor">
```

Icons:

```xaml
<StreamGeometry x:Key="message_info_icon">
<StreamGeometry x:Key="message_success_icon">
<StreamGeometry x:Key="message_warning_icon">
<StreamGeometry x:Key="message_error_icon">
<StreamGeometry x:Key="message_close_icon">
```

---

- [Source code on GitHub](https://github.com/AvionBlock/Message.Avalonia)

## License

MIT
