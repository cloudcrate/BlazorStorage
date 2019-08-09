
# Blazor Local and Session Storage Support

## Installation

```powershell
PM> Install-Package Cloudcrate.AspNetCore.Blazor.Browser.Storage
```

## See it in Action

Check out [Steve Sanderson's demo at NDC Minnesota, at minute 48](https://youtu.be/JU-6pAxqAa4?t=2875)

## Usage

Add Services to Dependency Injection

```csharp
services.AddStorage();
```

## Client-Side

Inject and use Storage

```razor
@using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
@inject LocalStorage Storage

<input type="text" @bind="value" />
<button @onclick="SetValue">Set</button>
<button @onclick="GetValue">Get</button>


@code
{
    string value;

    async void SetValue()
    {
        Storage.SetItem("storageKey", value);
    }

    async void GetValue()
    {
        value = Storage.GetItem("storageKey");
    }
}
```

## Server-Side

Add Javascript file to your server-side page

at `_Host.cshtml` in `<body>`
```
<app>@(await Html.RenderComponentAsync<App>())</app>
<script src="_framework/blazor.server.js"></script>
<script src="_content/Cloudcrate.AspNetCore.Blazor.Browser.Storage/Storage.js"></script>
```

Inject and use Storage

```razor
@using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
@inject LocalStorage Storage

<input type="text" @bind="value" />
<button @onclick="SetValue">Set</button>
<button @onclick="GetValue">Get</button>


@code
{
    string value;

    Task SetValue()
    {
        await Storage.SetItemAsync("storageKey", value);
    }

    Task GetValue()
    {
        value = await Storage.GetItemAsync("storageKey");
    }
}
```

## Events

Using `storage` native event: [StorageEvent](https://developer.mozilla.org/en-US/docs/Web/API/StorageEvent)

```csharp
protected override void OnInit()
{
    Storage.StorageChanged += HandleStorageChanged;
}

void HandleStorageChanged(object sender, StorageEventArgs e)  
{  
    Console.WriteLine($"Value for key {e.Key} changed from {e.OldValue} to {e.NewValue}");
} 

public void Dispose()
{
    Storage.StorageChanged -= HandleStorageChanged;
}
```

## Contributors

* StorageEvent implementation by [@peterblazejewicz](https://github.com/peterblazejewicz)
* Server-side support by [@konradbartecki](https://github.com/konradbartecki/)
