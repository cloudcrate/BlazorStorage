// Copyright (c) 2018 cloudcrate solutions UG (haftungsbeschraenkt)

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;

namespace Cloudcrate.AspNetCore.Blazor.Browser.Storage
{
    public abstract class StorageBase
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IJSInProcessRuntime _jsProcessRuntime;
        private readonly string _fullTypeName;

        private EventHandler<StorageEventArgs> _storageChanged;

        protected abstract string StorageTypeName { get; }


        protected internal StorageBase(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _jsProcessRuntime = (IJSInProcessRuntime)_jsRuntime;
            _fullTypeName = GetType().FullName.Replace('.', '_');
        }

        public void Clear()
        {
            _jsProcessRuntime.Invoke<object>($"{_fullTypeName}.Clear");
        }

        public string GetItem(string key)
        {
            return _jsProcessRuntime.Invoke<string>($"{_fullTypeName}.GetItem", key);
        }

        public T GetItem<T>(string key)
        {
            var json = GetItem(key);
            return string.IsNullOrEmpty(json) ? default(T) : Json.Deserialize<T>(json);
        }

        public string Key(int index)
        {
            return _jsProcessRuntime.Invoke<string>($"{_fullTypeName}.Key", index);
        }

        public int Length => _jsProcessRuntime.Invoke<int>($"{_fullTypeName}.Length");

        public void RemoveItem(string key)
        {
            _jsProcessRuntime.Invoke<object>($"{_fullTypeName}.RemoveItem", key);
        }

        public void SetItem(string key, string data)
        {
            _jsProcessRuntime.Invoke<object>($"{_fullTypeName}.SetItem", key, data);
        }

        public void SetItem(string key, object data)
        {
            SetItem(key, Json.Serialize(data));
        }

        public string this[string key]
        {
            get => _jsProcessRuntime.Invoke<string>($"{_fullTypeName}.GetItemString", key);
            set => _jsProcessRuntime.Invoke<object>($"{_fullTypeName}.SetItemString", key, value);
        }

        public string this[int index]
        {
            get => _jsProcessRuntime.Invoke<string>($"{_fullTypeName}.GetItemNumber", index);
            set => _jsProcessRuntime.Invoke<object>($"{_fullTypeName}.SetItemNumber", index, value);
        }

        public event EventHandler<StorageEventArgs> StorageChanged
        {
            add
            {
                if (_storageChanged == null)
                {
                    this._jsRuntime.InvokeAsync<object>(
                        $"{_fullTypeName}.AddEventListener",
                        new DotNetObjectRef(this)
                    );
                }
                _storageChanged += value;
            }
            remove
            {
                _storageChanged -= value;
                if (_storageChanged == null)
                {
                    this._jsRuntime.InvokeAsync<object>($"{_fullTypeName}.RemoveEventListener");
                }
            }
        }

        [JSInvokable]
        public virtual void OnStorageChanged(string key, object oldValue, object newValue)
        {
            EventHandler<StorageEventArgs> handler = _storageChanged;
            if (handler != null)
            {
                handler(this, new StorageEventArgs
                {
                    Key = key,
                    OldValue = oldValue,
                    NewValue = newValue,
                });
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
        public static void AddStorage(this IServiceCollection col)
        {
            col.TryAddSingleton<LocalStorage>();
            col.TryAddSingleton<SessionStorage>();
        }
    }
}
