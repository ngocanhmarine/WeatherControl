using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WeatherControl.Control2
{
    internal class UncommonField<T>
    {
        private T _defaultValue;
        private int _globalIndex;
        private bool _hasBeenSet;

        public UncommonField()
          : this(default(T))
        {
        }

        public UncommonField(T defaultValue)
        {
            this._defaultValue = defaultValue;
            this._hasBeenSet = false;
            lock (DependencyProperty.Synchronized)
            {
                this._globalIndex = DependencyProperty.GetUniqueGlobalIndex((Type)null, (string)null);
                DependencyProperty.RegisteredPropertyList.Add();
            }
        }

        public void SetValue(DependencyObject instance, T value)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            EntryIndex entryIndex = instance.LookupEntry(this._globalIndex);
            if ((object)value != (object)this._defaultValue)
            {
                instance.SetEffectiveValue(entryIndex, (DependencyProperty)null, this._globalIndex, (PropertyMetadata)null, (object)value, BaseValueSourceInternal.Local);
                this._hasBeenSet = true;
            }
            else
                instance.UnsetEffectiveValue(entryIndex, (DependencyProperty)null, (PropertyMetadata)null);
        }

        public T GetValue(DependencyObject instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (!this._hasBeenSet)
                return this._defaultValue;
            EntryIndex entryIndex = instance.LookupEntry(this._globalIndex);
            if (entryIndex.Found)
            {
                object localValue = instance.EffectiveValues[(int)entryIndex.Index].LocalValue;
                if (localValue != DependencyProperty.UnsetValue)
                    return (T)localValue;
            }
            return this._defaultValue;
        }

        public void ClearValue(DependencyObject instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            EntryIndex entryIndex = instance.LookupEntry(this._globalIndex);
            instance.UnsetEffectiveValue(entryIndex, (DependencyProperty)null, (PropertyMetadata)null);
        }

        internal int GlobalIndex
        {
            get
            {
                return this._globalIndex;
            }
        }
    }

}
