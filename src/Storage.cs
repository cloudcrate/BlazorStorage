// Copyright (c) cloudcrate solutions UG (haftungsbeschraenkt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;

namespace Cloudcrate.AspNetCore.Blazor.Browser.Storage
{
    public abstract class StorageBase
    {
        private readonly IJsRuntimeAccess _jsRuntime;
        private readonly string _fullTypeName;

        private EventHandler<StorageEventArgs> _storageChanged;

        protected abstract string StorageTypeName { get; }

        protected internal StorageBase(IJSRuntime jsRuntime)
        {
            if (jsRuntime is IJSInProcessRuntime rt)
                _jsRuntime = new ClientSideJsRuntimeAccess(rt);
            else _jsRuntime = new ServerSideJsRuntimeAccess(jsRuntime);
            _fullTypeName = GetType().FullName.Replace('.', '_');
        }

        public void Clear()
        {
            JsRuntimeInvoke<object>($"{_fullTypeName}.Clear");
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            return JsRuntimeInvokeAsync<object>($"{_fullTypeName}.Clear", Enumerable.Empty<object>(), cancellationToken);
        }

        public string GetItem(string key)
        {
            return JsRuntimeInvoke<string>($"{_fullTypeName}.GetItem", key);
        }

        public Task<string> GetItemAsync(string key, CancellationToken cancellationToken = default)
        {
            return JsRuntimeInvokeAsync<string>($"{_fullTypeName}.GetItem", new object[] { key }, cancellationToken);
        }

        public T GetItem<T>(string key)
        {
            var json = GetItem(key);
            return string.IsNullOrEmpty(json) ? default(T) : JsonSerializer.Deserialize<T>(json);
        }

        public async Task<T> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var json = await GetItemAsync(key, cancellationToken);
            return string.IsNullOrEmpty(json) ? default(T) : JsonSerializer.Deserialize<T>(json);
        }

        public string Key(int index)
        {
            return JsRuntimeInvoke<string>($"{_fullTypeName}.Key", index);
        }

        public Task<string> KeyAsync(int index, CancellationToken cancellationToken = default)
        {
            return JsRuntimeInvokeAsync<string>($"{_fullTypeName}.Key", new object[] { index }, cancellationToken);
        }

        public int Length => JsRuntimeInvoke<int>($"{_fullTypeName}.Length");

        public Task<int> LengthAsync(CancellationToken cancellationToken = default) => JsRuntimeInvokeAsync<int>($"{_fullTypeName}.Length", Enumerable.Empty<object>(), cancellationToken);

        public void RemoveItem(string key)
        {
            JsRuntimeInvoke<object>($"{_fullTypeName}.RemoveItem", key);
        }

        public Task RemoveItemAsync(string key, CancellationToken cancellationToken = default)
        {
            return JsRuntimeInvokeAsync<object>($"{_fullTypeName}.RemoveItem", new object[] { key }, cancellationToken);
        }

        public void SetItem(string key, string data)
        {
            JsRuntimeInvoke<object>($"{_fullTypeName}.SetItem", key, data);
        }

        public Task SetItemAsync(string key, string data, CancellationToken cancellationToken = default)
        {
            return JsRuntimeInvokeAsync<object>($"{_fullTypeName}.SetItem", new object[] { key, data }, cancellationToken);
        }

        public void SetItem(string key, object data)
        {
            SetItem(key, JsonSerializer.Serialize(data));
        }

        public Task SetItemAsync(string key, object data, CancellationToken cancellationToken = default)
        {
            return SetItemAsync(key, JsonSerializer.Serialize(data), cancellationToken);
        }

        public string this[string key]
        {
            get => JsRuntimeInvoke<string>($"{_fullTypeName}.GetItemString", key);
            set => JsRuntimeInvoke<object>($"{_fullTypeName}.SetItemString", key, value);
        }

        public string this[int index]
        {
            get => JsRuntimeInvoke<string>($"{_fullTypeName}.GetItemNumber", index);
            set => JsRuntimeInvoke<object>($"{_fullTypeName}.SetItemNumber", index, value);
        }

        public event EventHandler<StorageEventArgs> StorageChanged
        {
            add
            {
                if (_storageChanged == null)
                {
                    JsRuntimeInvokeAsync<object>(
                        $"{_fullTypeName}.AddEventListener",
                        DotNetObjectRef.Create(this)
                    );
                }
                _storageChanged += value;
            }
            remove
            {
                _storageChanged -= value;
                if (_storageChanged == null)
                {
                    JsRuntimeInvokeAsync<object>($"{_fullTypeName}.RemoveEventListener");
                }
            }
        }

        [JSInvokable]
        public virtual void OnStorageChanged(string key, object oldValue, object newValue)
        {
            EventHandler<StorageEventArgs> handler = _storageChanged;
            handler?.Invoke(this, new StorageEventArgs
            {
                Key = key,
                OldValue = oldValue,
                NewValue = newValue,
            });
        }

        private T JsRuntimeInvoke<T>(string identifier, params object[] args)
        {
            return _jsRuntime.Invoke<T>(identifier, args);
        }

        private Task<T> JsRuntimeInvokeAsync<T>(string identifier, params object[] args)
        {
            return _jsRuntime.InvokeAsync<T>(identifier, args);
        }

        private Task<T> JsRuntimeInvokeAsync<T>(string identifier, IEnumerable<object> args, CancellationToken cancellationToken = default)
        {
            return _jsRuntime.InvokeAsync<T>(identifier, args, cancellationToken);
        }

        private interface IJsRuntimeAccess
        {
            T Invoke<T>(string identifier, params object[] args);

            Task<T> InvokeAsync<T>(string identifier, params object[] args);

            Task<T> InvokeAsync<T>(string identifier, IEnumerable<object> args,
                CancellationToken cancellationToken = default);
        }

        private abstract class JsRuntimeAccessBase<TJsRuntime> : IJsRuntimeAccess where TJsRuntime : IJSRuntime
        {
            protected TJsRuntime JsRuntime { get; }

            protected JsRuntimeAccessBase(TJsRuntime jsRuntime)
            {
                JsRuntime = jsRuntime;
            }

            public abstract T Invoke<T>(string identifier, params object[] args);

            public Task<T> InvokeAsync<T>(string identifier, params object[] args)
            {
                return JsRuntime.InvokeAsync<T>(identifier, args);
            }

            public Task<T> InvokeAsync<T>(string identifier, IEnumerable<object> args, CancellationToken cancellationToken = default)
            {
                return JsRuntime.InvokeAsync<T>(identifier, args, cancellationToken);
            }
        }

        private class ServerSideJsRuntimeAccess : JsRuntimeAccessBase<IJSRuntime>
        {
            public ServerSideJsRuntimeAccess(IJSRuntime jsRuntime) : base(jsRuntime) { }

            public override T Invoke<T>(string identifier, params object[] args)
            {
                throw new NotSupportedException("Synchronous storage access is not supported in a server-side app. Please use the asynchronous implementation.");
            }
        }

        private class ClientSideJsRuntimeAccess : JsRuntimeAccessBase<IJSInProcessRuntime>
        {
            public ClientSideJsRuntimeAccess(IJSInProcessRuntime jsRuntime) : base(jsRuntime) { }

            public override T Invoke<T>(string identifier, params object[] args)
            {
                return JsRuntime.Invoke<T>(identifier, args);
            }
        }
    }

    public sealed class LocalStorage : StorageBase
    {
        protected override string StorageTypeName => nameof(LocalStorage);

        public LocalStorage(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }
    }

    public sealed class SessionStorage : StorageBase
    {
        protected override string StorageTypeName => nameof(SessionStorage);

        public SessionStorage(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorage(this IServiceCollection services)
        {
            services.TryAddScoped<LocalStorage>();
            services.TryAddScoped<SessionStorage>();
            return services;
        }
    }
}
