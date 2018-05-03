
# Blazor Local and Session Storage Support

## Installation

```powershell
PM> Install-Package Cloudcrate.AspNetCore.Blazor.Browser.Storage
```

## Usage

### Add Services to Dependency Injection

```csharp
var serviceProvider = new BrowserServiceProvider(services =>
{
    services.AddStorage();
});
```

### Inject and use Storage

```razor
@using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
@inject LocalStorage Storage

<input type="text" bind="@value" />
<button onclick="@SetValue">Set</button>
<button onclick="@GetValue">Get</button>


@functions
{
    string value;

    void SetValue()
    {
        Storage["Value"] = value;
    }

    void GetValue()
    {
        value = Storage["Value"];
    }
}

```
