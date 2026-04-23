using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Events.Content
{
    public class AssetRequestedEventArgs : EventArgs
    {
        private readonly Func<object> _defaultLoad;
        private Func<object>   _modLoad;
        private Action<object> _modEdit;

        public IAssetName Name { get; }

        internal AssetRequestedEventArgs(IAssetName name, Func<object> defaultLoad) {
            Name         = name;
            _defaultLoad = defaultLoad;
        }

        /// <summary>Provide replacement data for the asset.</summary>
        public void LoadFrom<T>(Func<T> load,
                                 AssetLoadPriority priority = AssetLoadPriority.Medium)
            where T : class {
            _modLoad = () => load();
        }

        /// <summary>Edit the loaded asset data.</summary>
        public void Edit<T>(Action<IAssetData<T>> apply,
                             AssetEditPriority priority = AssetEditPriority.Default)
            where T : class {
            _modEdit = obj => apply(new ContentAssetDataWrapper<T>((T)obj, Name));
        }

        internal object InvokeLoad() => _modLoad?.Invoke() ?? _defaultLoad?.Invoke();
        internal void   InvokeEdit(object data) => _modEdit?.Invoke(data);
        internal bool   HasLoad  => _modLoad  != null;
        internal bool   HasEdit  => _modEdit  != null;
    }

    public enum AssetLoadPriority { Low = -1000, Medium = 0, High = 1000 }
    public enum AssetEditPriority { Early = -1000, Default = 0, Late = 1000 }

    internal sealed class ContentAssetDataWrapper<T> : IAssetData<T> where T : class
    {
        public IAssetName Name     { get; }
        public Type       DataType => typeof(T);
        public T          Data     { get; set; }
        object IAssetData.Data     => Data;

        public ContentAssetDataWrapper(T data, IAssetName name) {
            Data = data; Name = name;
        }

        public IAssetData<TData> GetData<TData>() where TData : class
            => new ContentAssetDataWrapper<TData>((TData)(object)Data, Name);

        public void ReplaceWith(T newData) => Data = newData;
        public void Edit(Action<T> editor) => editor(Data);
    }

    public class AssetReadyEventArgs : EventArgs
    {
        public IAssetName Name { get; }
        internal AssetReadyEventArgs(IAssetName name) { Name = name; }
    }

    public class AssetsInvalidatedEventArgs : EventArgs
    {
        public IReadOnlySet<IAssetName> Names { get; }
        internal AssetsInvalidatedEventArgs(IReadOnlySet<IAssetName> names) { Names = names; }
    }

    public class LocaleChangedEventArgs : EventArgs
    {
        public string OldLocale { get; }
        public string NewLocale { get; }
        internal LocaleChangedEventArgs(string oldLocale, string newLocale) {
            OldLocale = oldLocale; NewLocale = newLocale;
        }
    }
}
