// Copyright (c) 2018 cloudcrate solutions UG (haftungsbeschraenkt)

using System;

namespace Cloudcrate.AspNetCore.Blazor.Browser.Storage
{
    public class StorageEventArgs : EventArgs
    {
        public string Key { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}