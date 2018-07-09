// Copyright (c) 2018 cloudcrate solutions UG (haftungsbeschraenkt)

const storageAssembly = 'Cloudcrate_AspNetCore_Blazor_Browser';
const storageNamespace = `${storageAssembly}_Storage`;

const storages: { [key: string]: Storage } = {
    LocalStorage: localStorage,
    SessionStorage: sessionStorage
};

for (var storageTypeName in storages) {
    if (storages.hasOwnProperty(storageTypeName)) {
        const storage = storages[storageTypeName];
        const storageFullTypeName = `${storageNamespace}_${storageTypeName}`;

        window[storageFullTypeName] = {
            Clear: () => {
                clear(storage);
            },

            GetItem: (key: string) => {
                return getItem(storage, key);
            },

            Key: (index: number) => {
                return key(storage, index);
            },

            Length: () => {
                return getLength(storage);
            },

            RemoveItem: (key: string) => {
                removeItem(storage, key);
            },

            SetItem: (key: string, data: any) => {
                setItem(storage, key, data);
            },

            GetItemString: (key: string) => {
                return getItemString(storage, key);
            },

            SetItemString: (key: string, data: string) => {
                setItemString(storage, key, data);
            },

            GetItemNumber: (index: number) => {
                return getItemNumber(storage, index);
            },

            SetItemNumber: (index: number, data: string) => {
                setItemNumber(storage, index, data);
            }

        };
    }
}

function clear(storage: Storage) {
    storage.clear();
}

function getItem(storage: Storage, key: string) {
    return storage.getItem(key);
}

function key(storage: Storage, index: number) {
    return storage.key(index);
}

function getLength(storage: Storage) {
    return storage.length;
}

function removeItem(storage: Storage, key: string) {
    storage.removeItem(key);
}

function setItem(storage: Storage, key: string, data: any) {
    storage.setItem(key, data);
}

function getItemString(storage: Storage, key: string) {
    return storage[key];
}

function setItemString(storage: Storage, key: string, data: any) {
    storage[key] = data;
}

function getItemNumber(storage: Storage, index: number) {
    return storage[index];
}

function setItemNumber(storage: Storage, index: number, data: string) {
    storage[index] = data;
}
