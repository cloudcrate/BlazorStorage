// Copyright (c) 2018 cloudcrate solutions UG (haftungsbeschraenkt)

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;

namespace Cloudcrate.AspNetCore.Blazor.Browser.Storage
{
    public abstract class StorageBase
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly string _fullTypeName;

        protected internal StorageBase(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _fullTypeName = GetType().FullName.Replace('.', '_');
        }

        public void Clear()
        {
            ((IJSInProcessRuntime)_jsRuntime).Invoke<object>($"{_fullTypeName}.Clear");
        }

        public string GetItem(string key)
        {
            return ((IJSInProcessRuntime)_jsRuntime).Invoke<string>($"{_fullTypeName}.GetItem", key);
        }

        public T GetItem<T>(string key)
        {
            var json = GetItem(key);
            return string.IsNullOrEmpty(json) ? default(T) : Json.Deserialize<T>(json);
        }

        public string Key(int index)
        {
            return ((IJSInProcessRuntime)_jsRuntime).Invoke<string>($"{_fullTypeName}.Key", index);
        }

        public int Length => ((IJSInProcessRuntime)_jsRuntime).Invoke<int>($"{_fullTypeName}.Length");

        public void RemoveItem(string key)
        {
            ((IJSInProcessRuntime)_jsRuntime).Invoke<object>($"{_fullTypeName}.RemoveItem", key);
        }

        public void SetItem(string key, string data)
        {
            ((IJSInProcessRuntime)_jsRuntime).Invoke<object>($"{_fullTypeName}.SetItem", key, data);
        }

        public void SetItem(string key, object data)
        {
            SetItem(key, Json.Serialize(data));
        }

        public string this[string key]
        {
            get => ((IJSInProcessRuntime)_jsRuntime).Invoke<string>($"{_fullTypeName}.GetItemString", key);
            set => ((IJSInProcessRuntime)_jsRuntime).Invoke<object>($"{_fullTypeName}.SetItemString", key, value);
        }

        public string this[int index]
        {
            get => ((IJSInProcessRuntime)_jsRuntime).Invoke<string>($"{_fullTypeName}.GetItemNumber", index);
            set => ((IJSInProcessRuntime)_jsRuntime).Invoke<object>($"{_fullTypeName}.SetItemNumber", index, value);
        }
    }

    public sealed class LocalStorage : StorageBase
    {
        public LocalStorage(IJSRuntime jsRuntime) : base(jsRuntime)
        {

        }
    }

    public sealed class SessionStorage : StorageBase
    {
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
