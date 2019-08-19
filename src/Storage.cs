// Copyright (c) 2018 cloudcrate solutions UG (haftungsbeschraenkt)
﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;

namespace Cloudcrate.AspNetCore.Blazor.Browser.Storage
{
    public abstract class StorageBase
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IJSInProcessRuntime _jsProcessRuntime;
        private bool IsServerSideBlazor = false;
        private readonly string _fullTypeName;

        private EventHandler<StorageEventArgs> _storageChanged;

        protected abstract string StorageTypeName { get; }

        protected internal StorageBase(IJSRuntime jsRuntime)
        {
            if(jsRuntime is IJSInProcessRuntime)
            {
                _jsProcessRuntime = (IJSInProcessRuntime) jsRuntime;
            }
            else
            {
                IsServerSideBlazor = true;
                _jsRuntime = jsRuntime;
            }
            _fullTypeName = GetType().FullName.Replace('.', '_');
        }

        public async Task ClearAsync()
        {
            await InvokeOnJs<object>($"{_fullTypeName}.Clear");
        }

        public async Task<string> GetItemAsync(string key)
        {
            return await InvokeOnJs<string>($"{_fullTypeName}.GetItem", key);
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            var json = await GetItemAsync(key);
            return string.IsNullOrEmpty(json) ? default(T) : JsonSerializer.Deserialize<T>(json);
        }

        public async Task RemoveItemAsync(string key)
        {
            await InvokeOnJs<object>($"{_fullTypeName}.RemoveItem", key);
        }

        public async Task SetItemAsync(string key, string data)
        {
            await InvokeOnJs<object>($"{_fullTypeName}.SetItem", key, data);
        }

        public async Task SetItemAsync(string key, object data)
        {
            await SetItemAsync(key, JsonSerializer.Serialize(data));
        }
        public event EventHandler<StorageEventArgs> StorageChanged
        {
            add
            {
                if (_storageChanged == null)
                {
                    _jsRuntime.InvokeAsync<object>(
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
                    _jsRuntime.InvokeAsync<object>($"{_fullTypeName}.RemoveEventListener");
                }
            }
        }

        private async Task<TValue> InvokeOnJs<TValue>(string identifier, params object[] args)
        {
            if(IsServerSideBlazor)
            {
                return await _jsRuntime.InvokeAsync<TValue>(identifier, args);
            }
            else
            {
                return _jsProcessRuntime.Invoke<TValue>(identifier, args);
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
        public static IServiceCollection AddStorage(this IServiceCollection col)
        {
            col.TryAddScoped<LocalStorage>();
            col.TryAddScoped<SessionStorage>();
            return col;
        }
    }
}
