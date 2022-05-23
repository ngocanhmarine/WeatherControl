using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WeatherControl.Control2
{
    [DefaultEvent("SelectionChanged"), DefaultProperty("SelectedIndex"), Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public abstract class MySelector : ItemsControl
    {
        // Fields
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(MySelector));
        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MySelector));
        public static readonly RoutedEvent UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MySelector));
        internal static readonly DependencyPropertyKey IsSelectionActivePropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsSelectionActive", typeof(bool), typeof(MySelector), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsSelectionActiveProperty = IsSelectionActivePropertyKey.DependencyProperty;
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(MySelector), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty = DependencyProperty.Register("IsSynchronizedWithCurrentItem", typeof(bool?), typeof(MySelector), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(MySelector.OnIsSynchronizedWithCurrentItemChanged)));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(MySelector), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(MySelector.OnSelectedIndexChanged), new CoerceValueCallback(MySelector.CoerceSelectedIndex)), new ValidateValueCallback(MySelector.ValidateSelectedIndex));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(MySelector), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(MySelector.OnSelectedItemChanged), new CoerceValueCallback(MySelector.CoerceSelectedItem)));
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(object), typeof(MySelector), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(MySelector.OnSelectedValueChanged), new CoerceValueCallback(MySelector.CoerceSelectedValue)));
        public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(MySelector), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(MySelector.OnSelectedValuePathChanged)));
        private static readonly DependencyPropertyKey SelectedItemsPropertyKey = DependencyProperty.RegisterReadOnly("SelectedItems", typeof(IList), typeof(MySelector), new FrameworkPropertyMetadata(null));
        internal static readonly DependencyProperty SelectedItemsImplProperty = SelectedItemsPropertyKey.DependencyProperty;
        private static readonly BindingExpressionUncommonField ItemValueBindingExpression = new BindingExpressionUncommonField();
        internal InternalSelectedItemsStorage _selectedItems = new InternalSelectedItemsStorage(1, MatchExplicitEqualityComparer);
        private Point _lastMousePosition;
        private SelectionChanger _selectionChangeInstance;
        private BitVector32 _cacheValid = new BitVector32(2);
        private EventHandler _focusEnterMainFocusScopeEventHandler;
        private DependencyObject _clearingContainer;
        private static readonly UncommonField<ItemsControl.ItemInfo> PendingSelectionByValueField = new UncommonField<ItemsControl.ItemInfo>();
        private static readonly ItemInfoEqualityComparer MatchExplicitEqualityComparer = new ItemInfoEqualityComparer(false);
        private static readonly ItemInfoEqualityComparer MatchUnresolvedEqualityComparer = new ItemInfoEqualityComparer(true);
        private static readonly UncommonField<ChangeInfo> ChangeInfoField = new UncommonField<ChangeInfo>();

        // Events
        [Category("Behavior")]
        public event SelectionChangedEventHandler SelectionChanged
        {
            add
            {
                base.AddHandler(SelectionChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(SelectionChangedEvent, value);
            }
        }

        // Methods
        static MySelector()
        {
            EventManager.RegisterClassHandler(typeof(MySelector), SelectedEvent, new RoutedEventHandler(MySelector.OnSelected));
            EventManager.RegisterClassHandler(typeof(MySelector), UnselectedEvent, new RoutedEventHandler(MySelector.OnUnselected));
        }

        protected MySelector()
        {
            base.Items.CurrentChanged += new EventHandler(this.OnCurrentChanged);
            base.ItemContainerGenerator.StatusChanged += new EventHandler(this.OnGeneratorStatusChanged);
            this._focusEnterMainFocusScopeEventHandler = new EventHandler(this.OnFocusEnterMainFocusScope);
            KeyboardNavigation.Current.FocusEnterMainFocusScope += this._focusEnterMainFocusScopeEventHandler;
            ObservableCollection<object> observables = new SelectedItemCollection(this);
            base.SetValue(SelectedItemsPropertyKey, observables);
            observables.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnSelectedItemsCollectionChanged);
            base.SetValue(IsSelectionActivePropertyKey, BooleanBoxes.FalseBox);
        }

        public static void AddSelectedHandler(DependencyObject element, RoutedEventHandler handler)
        {
            AddHandler(element, SelectedEvent, handler);
        }

        public static void AddUnselectedHandler(DependencyObject element, RoutedEventHandler handler)
        {
            AddHandler(element, UnselectedEvent, handler);
        }

        internal override void AdjustItemInfoOverride(NotifyCollectionChangedEventArgs e)
        {
            base.AdjustItemInfos(e, this._selectedItems);
            base.AdjustItemInfoOverride(e);
        }

        internal virtual void AdjustItemInfosAfterGeneratorChangeOverride()
        {
            base.AdjustItemInfosAfterGeneratorChange(this._selectedItems, true);
        }

        private void AdjustNewContainers()
        {
            if (this._cacheValid[0x40])
            {
                base.LayoutUpdated -= new EventHandler(this.OnLayoutUpdated);
                this._cacheValid[0x40] = false;
            }
            this.AdjustItemInfosAfterGeneratorChangeOverride();
            if (base.HasItems)
            {
                this.SelectionChange.Begin();
                try
                {
                    for (int i = 0; i < this._selectedItems.Count; i++)
                    {
                        this.ItemSetIsSelected(this._selectedItems[i], true);
                    }
                }
                finally
                {
                    this.SelectionChange.Cancel();
                }
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            if (!((IGeneratorHost)this).IsItemItsOwnContainer(item))
            {
                try
                {
                    this._clearingContainer = element;
                    element.ClearValue(IsSelectedProperty);
                }
                finally
                {
                    this._clearingContainer = null;
                }
            }
        }

        private static object CoerceSelectedIndex(DependencyObject d, object value)
        {
            MySelector selector = (MySelector)d;
            if (!(value is int) || (((int)value) < selector.Items.Count))
            {
                return value;
            }
            return DependencyProperty.UnsetValue;
        }

        private static object CoerceSelectedItem(DependencyObject d, object value)
        {
            MySelector selector = (MySelector)d;
            if ((value == null) || selector.SkipCoerceSelectedItemCheck)
            {
                return value;
            }
            int selectedIndex = selector.SelectedIndex;
            if ((((selectedIndex > -1) && (selectedIndex < selector.Items.Count)) && (selector.Items[selectedIndex] == value)) || selector.Items.Contains(value))
            {
                return value;
            }
            return DependencyProperty.UnsetValue;
        }

        private static object CoerceSelectedValue(DependencyObject d, object value)
        {
            MySelector selector = (MySelector)d;
            if (selector.SelectionChange.IsActive)
            {
                selector._cacheValid[0x10] = false;
            }
            else if ((selector.SelectItemWithValue(value, false) == DependencyProperty.UnsetValue) && selector.HasItems)
            {
                value = null;
            }
            return value;
        }

        internal bool DidMouseMove()
        {
            Point position = Mouse.GetPosition(this);
            if (!(position != this._lastMousePosition))
            {
                return false;
            }
            this._lastMousePosition = position;
            return true;
        }

        private object FindItemWithValue(object value, out int index)
        {
            index = -1;
            if (base.HasItems)
            {
                BindingExpression expression = this.PrepareItemValueBinding(base.Items.GetRepresentativeItem());
                if (expression == null)
                {
                    return DependencyProperty.UnsetValue;
                }
                if (string.IsNullOrEmpty(this.SelectedValuePath))
                {
                    if (!string.IsNullOrEmpty(expression.ParentBinding.Path.Path))
                    {
                        return SystemXmlHelper.FindXmlNodeWithInnerText(base.Items, value, out index);
                    }
                    index = base.Items.IndexOf(value);
                    return ((index < 0) ? DependencyProperty.UnsetValue : value);
                }
                Type knownType = (value != null) ? value.GetType() : null;
                object obj2 = value;
                DynamicValueConverter converter = new DynamicValueConverter(false);
                index = 0;
                using (IEnumerator enumerator = ((IEnumerable)base.Items).GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        object current = enumerator.Current;
                        expression.Activate(current);
                        object itemValue = expression.Value;
                        if (!this.VerifyEqual(value, knownType, itemValue, converter))
                        {
                            index++;
                            continue;
                        }
                        expression.Deactivate();
                        return current;
                    }
                }
                expression.Deactivate();
                index = -1;
            }
            return DependencyProperty.UnsetValue;
        }

        internal void FinishSelectedItemsChange()
        {
            ChangeInfo info = ChangeInfoField.GetValue(this);
            if (info != null)
            {
                bool isActive = this.SelectionChange.IsActive;
                if (!isActive)
                {
                    this.SelectionChange.Begin();
                }
                this.UpdateSelectedItems(info.ToAdd, info.ToRemove);
                if (!isActive)
                {
                    this.SelectionChange.End();
                }
            }
        }

        [AttachedPropertyBrowsableForChildren]
        public static bool GetIsSelected(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(IsSelectedProperty);
        }

        public static bool GetIsSelectionActive(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(IsSelectionActiveProperty);
        }

        private bool InfoGetIsSelected(ItemsControl.ItemInfo info)
        {
            DependencyObject container = info.Container;
            if (container != null)
            {
                return (bool)container.GetValue(IsSelectedProperty);
            }
            if (!this.IsItemItsOwnContainerOverride(info.Item))
            {
                return false;
            }
            DependencyObject item = info.Item as DependencyObject;
            return ((item != null) && ((bool)item.GetValue(IsSelectedProperty)));
        }

        private void InvokeSelectionChanged(List<ItemsControl.ItemInfo> unselectedInfos, List<ItemsControl.ItemInfo> selectedInfos)
        {
            SelectionChangedEventArgs e = new SelectionChangedEventArgs(unselectedInfos, selectedInfos)
            {
                Source = this
            };
            this.OnSelectionChanged(e);
        }

        internal static bool ItemGetIsSelectable(object item)
        {
            return ((item != null) && !(item is Separator));
        }

        private void ItemSetIsSelected(ItemsControl.ItemInfo info, bool value)
        {
            if (info != null)
            {
                DependencyObject container = info.Container;
                if ((container != null) && !ReferenceEquals(container, ItemsControl.ItemInfo.RemovedContainer))
                {
                    if (GetIsSelected(container) != value)
                    {
                        container.SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.Box(value));
                    }
                }
                else
                {
                    object item = info.Item;
                    if (this.IsItemItsOwnContainerOverride(item))
                    {
                        DependencyObject element = item as DependencyObject;
                        if ((element != null) && (GetIsSelected(element) != value))
                        {
                            element.SetCurrentValueInternal(IsSelectedProperty, BooleanBoxes.Box(value));
                        }
                    }
                }
            }
        }

        internal void LocateSelectedItems(List<Tuple<int, int>> ranges = null, bool deselectMissingItems = false)
        {
            List<int> list = new List<int>(this._selectedItems.Count);
            int num = 0;
            foreach (ItemsControl.ItemInfo info2 in (IEnumerable<ItemsControl.ItemInfo>)this._selectedItems)
            {
                if (info2.Index < 0)
                {
                    num++;
                    continue;
                }
                list.Add(info2.Index);
            }
            int count = list.Count;
            list.Sort();
            ItemsControl.ItemInfo info = new ItemsControl.ItemInfo(null, ItemsControl.ItemInfo.KeyContainer, -1);
            for (int i = 0; (num > 0) && (i < base.Items.Count); i++)
            {
                if (list.BinarySearch(0, count, i, null) < 0)
                {
                    info.Reset(base.Items[i]);
                    info.Index = i;
                    ItemsControl.ItemInfo info3 = this._selectedItems.FindMatch(info);
                    if (info3 != null)
                    {
                        info3.Index = i;
                        list.Add(i);
                        num--;
                    }
                }
            }
            if (ranges != null)
            {
                ranges.Clear();
                list.Sort();
                list.Add(-1);
                int num4 = -1;
                int num5 = -2;
                foreach (int num6 in list)
                {
                    if (num6 == (num5 + 1))
                    {
                        num5 = num6;
                        continue;
                    }
                    if (num4 >= 0)
                    {
                        ranges.Add(new Tuple<int, int>(num4, (num5 - num4) + 1));
                    }
                    num4 = num5 = num6;
                }
            }
            if (deselectMissingItems)
            {
                foreach (ItemsControl.ItemInfo info4 in (IEnumerable<ItemsControl.ItemInfo>)this._selectedItems)
                {
                    if (info4.Index < 0)
                    {
                        info4.Container = ItemsControl.ItemInfo.RemovedContainer;
                        this.SelectionChange.Unselect(info4);
                    }
                }
            }
        }

        internal void NotifyIsSelectedChanged(FrameworkElement container, bool selected, RoutedEventArgs e)
        {
            if (this.SelectionChange.IsActive || ReferenceEquals(container, this._clearingContainer))
            {
                e.Handled = true;
            }
            else if (container != null)
            {
                object itemOrContainerFromContainer = base.GetItemOrContainerFromContainer(container);
                if (itemOrContainerFromContainer != DependencyProperty.UnsetValue)
                {
                    this.SetSelectedHelper(itemOrContainerFromContainer, container, selected);
                    e.Handled = true;
                }
            }
        }

        private void OnCurrentChanged(object sender, EventArgs e)
        {
            if (this.IsSynchronizedWithCurrentItemPrivate)
            {
                this.SetSelectedToCurrent();
            }
        }

        private void OnFocusEnterMainFocusScope(object sender, EventArgs e)
        {
            if (!base.IsKeyboardFocusWithin)
            {
                base.ClearValue(IsSelectionActivePropertyKey);
            }
        }

        private void OnGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                this.AdjustNewContainers();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.SetSynchronizationWithCurrentItem();
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            bool flag = false;
            if ((bool)e.NewValue)
            {
                flag = true;
            }
            else
            {
                DependencyObject focusedElement = Keyboard.FocusedElement as DependencyObject;
                if (focusedElement != null)
                {
                    UIElement visualRoot = KeyboardNavigation.GetVisualRoot(this) as UIElement;
                    if (((visualRoot != null) && visualRoot.IsKeyboardFocusWithin) && !ReferenceEquals(FocusManager.GetFocusScope(focusedElement), FocusManager.GetFocusScope(this)))
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                base.SetValue(IsSelectionActivePropertyKey, BooleanBoxes.TrueBox);
            }
            else
            {
                base.SetValue(IsSelectionActivePropertyKey, BooleanBoxes.FalseBox);
            }
        }

        private static void OnIsSynchronizedWithCurrentItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MySelector)d).SetSynchronizationWithCurrentItem();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if ((e.Action == NotifyCollectionChangedAction.Reset) || ((e.Action == NotifyCollectionChangedAction.Add) && (e.NewStartingIndex == 0)))
            {
                this.ResetSelectedItemsAlgorithm();
            }
            base.OnItemsChanged(e);
            EffectiveValueEntry entry = base.GetValueEntry(base.LookupEntry(SelectedIndexProperty.GlobalIndex), SelectedIndexProperty, null, RequestFlags.DeferredReferences);
            if (!entry.IsDeferredReference || !(entry.Value is DeferredSelectedIndexReference))
            {
                base.CoerceValue(SelectedIndexProperty);
            }
            base.CoerceValue(SelectedItemProperty);
            if (this._cacheValid[0x20] && !Equals(this.SelectedValue, this.InternalSelectedValue))
            {
                this.SelectItemWithValue(this.SelectedValue, true);
            }
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.SelectionChange.Begin();
                    try
                    {
                        ItemsControl.ItemInfo info = base.NewItemInfo(e.NewItems[0], null, e.NewStartingIndex);
                        if (this.InfoGetIsSelected(info))
                        {
                            this.SelectionChange.Select(info, true);
                        }
                        break;
                    }
                    finally
                    {
                        this.SelectionChange.End();
                    }
                    goto TR_0001;

                case NotifyCollectionChangedAction.Remove:
                    this.RemoveFromSelection(e);
                    return;

                case NotifyCollectionChangedAction.Replace:
                    goto TR_0001;

                case NotifyCollectionChangedAction.Move:
                    this.AdjustNewContainers();
                    this.SelectionChange.Validate();
                    return;

                case NotifyCollectionChangedAction.Reset:
                    if (base.Items.IsEmpty)
                    {
                        this.SelectionChange.CleanupDeferSelection();
                    }
                    if ((base.Items.CurrentItem != null) && this.IsSynchronizedWithCurrentItemPrivate)
                    {
                        this.SetSelectedToCurrent();
                        return;
                    }
                    this.SelectionChange.Begin();
                    try
                    {
                        this.LocateSelectedItems(null, true);
                        if (base.ItemsSource == null)
                        {
                            for (int i = 0; i < base.Items.Count; i++)
                            {
                                ItemsControl.ItemInfo info2 = base.ItemInfoFromIndex(i);
                                if (this.InfoGetIsSelected(info2) && !this._selectedItems.Contains(info2))
                                {
                                    this.SelectionChange.Select(info2, true);
                                }
                            }
                        }
                        break;
                    }
                    finally
                    {
                        this.SelectionChange.End();
                    }
                    goto TR_0000;

                default:
                    goto TR_0000;
            }
            return;
        TR_0000:
            object[] objArray1 = new object[] { e.Action };
            throw new NotSupportedException(SR.Get("UnexpectedCollectionChangeAction", objArray1));
        TR_0001:
            this.ItemSetIsSelected(base.ItemInfoFromIndex(e.NewStartingIndex), false);
            this.RemoveFromSelection(e);
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            this.SetSynchronizationWithCurrentItem();
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            this.AdjustNewContainers();
        }

        private void OnNewContainer()
        {
            if (!this._cacheValid[0x40])
            {
                this._cacheValid[0x40] = true;
                base.LayoutUpdated += new EventHandler(this.OnLayoutUpdated);
            }
        }

        private static void OnSelected(object sender, RoutedEventArgs e)
        {
            ((MySelector)sender).NotifyIsSelectedChanged(e.OriginalSource as FrameworkElement, true, e);
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MySelector selector = (MySelector)d;
            if (!selector.SelectionChange.IsActive)
            {
                selector.SelectionChange.SelectJustThisItem(selector.ItemInfoFromIndex((int)e.NewValue), true);
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MySelector selector = (MySelector)d;
            if (!selector.SelectionChange.IsActive)
            {
                selector.SelectionChange.SelectJustThisItem(selector.NewItemInfo(e.NewValue, null, -1), false);
            }
        }

        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!this.SelectionChange.IsActive)
            {
                if (!this.CanSelectMultiple)
                {
                    throw new InvalidOperationException(SR.Get("ChangingCollectionNotSupported"));
                }
                this.SelectionChange.Begin();
                bool flag = false;
                try
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (e.NewItems.Count != 1)
                            {
                                throw new NotSupportedException(SR.Get("RangeActionsNotSupported"));
                            }
                            this.SelectionChange.Select(base.NewUnresolvedItemInfo(e.NewItems[0]), false);
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            if (e.OldItems.Count != 1)
                            {
                                throw new NotSupportedException(SR.Get("RangeActionsNotSupported"));
                            }
                            this.SelectionChange.Unselect(base.NewUnresolvedItemInfo(e.OldItems[0]));
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            if (e.NewItems.Count != 1)
                            {
                                goto TR_000D;
                            }
                            else if (e.OldItems.Count == 1)
                            {
                                this.SelectionChange.Unselect(base.NewUnresolvedItemInfo(e.OldItems[0]));
                                this.SelectionChange.Select(base.NewUnresolvedItemInfo(e.NewItems[0]), false);
                            }
                            else
                            {
                                goto TR_000D;
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            break;

                        case NotifyCollectionChangedAction.Reset:
                            {
                                this.SelectionChange.CleanupDeferSelection();
                                int num = 0;
                                while (true)
                                {
                                    if (num >= this._selectedItems.Count)
                                    {
                                        ObservableCollection<object> observables = (ObservableCollection<object>)sender;
                                        for (int i = 0; i < observables.Count; i++)
                                        {
                                            this.SelectionChange.Select(base.NewUnresolvedItemInfo(observables[i]), false);
                                        }
                                        break;
                                    }
                                    this.SelectionChange.Unselect(this._selectedItems[num]);
                                    num++;
                                }
                                break;
                            }
                        default:
                            {
                                object[] args = new object[] { e.Action };
                                throw new NotSupportedException(SR.Get("UnexpectedCollectionChangeAction", args));
                            }
                    }
                    this.SelectionChange.End();
                    flag = true;
                    return;
                TR_000D:
                    throw new NotSupportedException(SR.Get("RangeActionsNotSupported"));
                }
                finally
                {
                    if (!flag)
                    {
                        this.SelectionChange.Cancel();
                    }
                }
            }
        }

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!FrameworkAppContextSwitches.SelectionPropertiesCanLagBehindSelectionChangedEvent)
            {
                MySelector instance = (MySelector)d;
                ItemsControl.ItemInfo info = PendingSelectionByValueField.GetValue(instance);
                if (info != null)
                {
                    try
                    {
                        if (!instance.SelectionChange.IsActive)
                        {
                            instance._cacheValid[0x10] = true;
                            instance.SelectionChange.SelectJustThisItem(info, true);
                        }
                    }
                    finally
                    {
                        instance._cacheValid[0x10] = false;
                        PendingSelectionByValueField.ClearValue(instance);
                    }
                }
            }
        }

        private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MySelector instance = (MySelector)d;
            ItemValueBindingExpression.ClearValue(instance);
            if (instance.GetValueEntry(instance.LookupEntry(SelectedValueProperty.GlobalIndex), SelectedValueProperty, null, RequestFlags.RawEntry).IsCoerced || (instance.SelectedValue != null))
            {
                instance.CoerceValue(SelectedValueProperty);
            }
        }

        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.RaiseEvent(e);
        }

        private static void OnUnselected(object sender, RoutedEventArgs e)
        {
            ((MySelector)sender).NotifyIsSelectedChanged(e.OriginalSource as FrameworkElement, false, e);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (item == this.SelectedItem)
            {
                KeyboardNavigation.Current.UpdateActiveElement(this, element);
            }
            this.OnNewContainer();
        }

        private BindingExpression PrepareItemValueBinding(object item)
        {
            if (item == null)
            {
                return null;
            }
            bool flag = SystemXmlHelper.IsXmlNode(item);
            BindingExpression bindingExpr = ItemValueBindingExpression.GetValue(this);
            if (bindingExpr != null)
            {
                bool flag2 = bindingExpr.ParentBinding.XPath != null;
                if ((!flag2 & flag) || (flag2 && !flag))
                {
                    ItemValueBindingExpression.ClearValue(this);
                    bindingExpr = null;
                }
            }
            if (bindingExpr == null)
            {
                Binding binding = new Binding
                {
                    Source = null
                };
                if (!flag)
                {
                    binding.Path = new PropertyPath(this.SelectedValuePath, new object[0]);
                }
                else
                {
                    binding.XPath = this.SelectedValuePath;
                    binding.Path = new PropertyPath("/InnerText", new object[0]);
                }
                bindingExpr = (BindingExpression)BindingExpressionBase.CreateUntargetedBindingExpression(this, binding);
                ItemValueBindingExpression.SetValue(this, bindingExpr);
            }
            return bindingExpr;
        }

        internal void RaiseIsSelectedChangedAutomationEvent(DependencyObject container, bool isSelected)
        {
            SelectorAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as SelectorAutomationPeer;
            if ((peer != null) && (peer.ItemPeers != null))
            {
                object itemOrContainerFromContainer = base.GetItemOrContainerFromContainer(container);
                if (itemOrContainerFromContainer != null)
                {
                    SelectorItemAutomationPeer peer2 = peer.ItemPeers[itemOrContainerFromContainer] as SelectorItemAutomationPeer;
                    if (peer2 != null)
                    {
                        peer2.RaiseAutomationIsSelectedChanged(isSelected);
                    }
                }
            }
        }

        private void RemoveFromSelection(NotifyCollectionChangedEventArgs e)
        {
            this.SelectionChange.Begin();
            try
            {
                ItemsControl.ItemInfo info = base.NewItemInfo(e.OldItems[0], ItemsControl.ItemInfo.SentinelContainer, e.OldStartingIndex);
                info.Container = null;
                if (this._selectedItems.Contains(info))
                {
                    this.SelectionChange.Unselect(info);
                }
            }
            finally
            {
                this.SelectionChange.End();
            }
        }

        public static void RemoveSelectedHandler(DependencyObject element, RoutedEventHandler handler)
        {
            RemoveHandler(element, SelectedEvent, handler);
        }

        public static void RemoveUnselectedHandler(DependencyObject element, RoutedEventHandler handler)
        {
            RemoveHandler(element, UnselectedEvent, handler);
        }

        internal void ResetLastMousePosition()
        {
            this._lastMousePosition = new Point();
        }

        private void ResetSelectedItemsAlgorithm()
        {
            if (!base.Items.IsEmpty)
            {
                this._selectedItems.UsesItemHashCodes = base.Items.CollectionView.HasReliableHashCodes();
            }
        }

        internal virtual void SelectAllImpl()
        {
            this.SelectionChange.Begin();
            this.SelectionChange.CleanupDeferSelection();
            try
            {
                int index = 0;
                foreach (object obj2 in (IEnumerable)base.Items)
                {
                    index++;
                    ItemsControl.ItemInfo info = base.NewItemInfo(obj2, null, index);
                    this.SelectionChange.Select(info, true);
                }
            }
            finally
            {
                this.SelectionChange.End();
            }
        }

        private object SelectItemWithValue(object value, bool selectNow)
        {
            object unsetValue;
            if (!FrameworkAppContextSwitches.SelectionPropertiesCanLagBehindSelectionChangedEvent)
            {
                if (!base.HasItems)
                {
                    unsetValue = DependencyProperty.UnsetValue;
                    this._cacheValid[0x20] = true;
                }
                else
                {
                    int num2;
                    unsetValue = this.FindItemWithValue(value, out num2);
                    ItemsControl.ItemInfo info = base.NewItemInfo(unsetValue, null, num2);
                    if (selectNow)
                    {
                        try
                        {
                            this._cacheValid[0x10] = true;
                            this.SelectionChange.SelectJustThisItem(info, true);
                            return unsetValue;
                        }
                        finally
                        {
                            this._cacheValid[0x10] = false;
                        }
                    }
                    PendingSelectionByValueField.SetValue(this, info);
                }
            }
            else
            {
                this._cacheValid[0x10] = true;
                if (base.HasItems)
                {
                    int num;
                    unsetValue = this.FindItemWithValue(value, out num);
                    this.SelectionChange.SelectJustThisItem(base.NewItemInfo(unsetValue, null, num), true);
                }
                else
                {
                    unsetValue = DependencyProperty.UnsetValue;
                    this._cacheValid[0x20] = true;
                }
                this._cacheValid[0x10] = false;
            }
            return unsetValue;
        }

        private void SetCurrentToSelected()
        {
            if (!this._cacheValid[1])
            {
                this._cacheValid[1] = true;
                try
                {
                    if (this._selectedItems.Count == 0)
                    {
                        base.Items.MoveCurrentToPosition(-1);
                    }
                    else
                    {
                        int index = this._selectedItems[0].Index;
                        if (index >= 0)
                        {
                            base.Items.MoveCurrentToPosition(index);
                        }
                        else
                        {
                            base.Items.MoveCurrentTo(this.InternalSelectedItem);
                        }
                    }
                }
                finally
                {
                    this._cacheValid[1] = false;
                }
            }
        }

        internal void SetInitialMousePosition()
        {
            this._lastMousePosition = Mouse.GetPosition(this);
        }

        public static void SetIsSelected(DependencyObject element, bool isSelected)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(IsSelectedProperty, BooleanBoxes.Box(isSelected));
        }

        private void SetSelectedHelper(object item, FrameworkElement UI, bool selected)
        {
            if (!ItemGetIsSelectable(item) & selected)
            {
                throw new InvalidOperationException(SR.Get("CannotSelectNotSelectableItem"));
            }
            this.SelectionChange.Begin();
            try
            {
                ItemsControl.ItemInfo info = base.NewItemInfo(item, UI, -1);
                if (selected)
                {
                    this.SelectionChange.Select(info, true);
                }
                else
                {
                    this.SelectionChange.Unselect(info);
                }
            }
            finally
            {
                this.SelectionChange.End();
            }
        }

        internal bool SetSelectedItemsImpl(IEnumerable selectedItems)
        {
            bool flag = false;
            if (!this.SelectionChange.IsActive)
            {
                this.SelectionChange.Begin();
                this.SelectionChange.CleanupDeferSelection();
                ObservableCollection<object> observables = (ObservableCollection<object>)base.GetValue(SelectedItemsImplProperty);
                try
                {
                    if (observables != null)
                    {
                        foreach (object obj2 in observables)
                        {
                            this.SelectionChange.Unselect(base.NewUnresolvedItemInfo(obj2));
                        }
                    }
                    if (selectedItems != null)
                    {
                        using (IEnumerator enumerator2 = selectedItems.GetEnumerator())
                        {
                            while (true)
                            {
                                if (!enumerator2.MoveNext())
                                {
                                    break;
                                }
                                object current = enumerator2.Current;
                                if (!this.SelectionChange.Select(base.NewUnresolvedItemInfo(current), false))
                                {
                                    this.SelectionChange.Cancel();
                                    return false;
                                }
                            }
                        }
                    }
                    this.SelectionChange.End();
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.SelectionChange.Cancel();
                    }
                }
            }
            return flag;
        }

        private void SetSelectedToCurrent()
        {
            if (!this._cacheValid[1])
            {
                this._cacheValid[1] = true;
                try
                {
                    object currentItem = base.Items.CurrentItem;
                    if ((currentItem == null) || !ItemGetIsSelectable(currentItem))
                    {
                        this.SelectionChange.SelectJustThisItem(null, false);
                    }
                    else
                    {
                        this.SelectionChange.SelectJustThisItem(base.NewItemInfo(currentItem, null, base.Items.CurrentPosition), true);
                    }
                }
                finally
                {
                    this._cacheValid[1] = false;
                }
            }
        }

        private void SetSynchronizationWithCurrentItem()
        {
            bool flag2;
            bool? isSynchronizedWithCurrentItem = this.IsSynchronizedWithCurrentItem;
            bool isSynchronizedWithCurrentItemPrivate = this.IsSynchronizedWithCurrentItemPrivate;
            if (isSynchronizedWithCurrentItem != null)
            {
                flag2 = isSynchronizedWithCurrentItem.Value;
            }
            else
            {
                if (!base.IsInitialized)
                {
                    return;
                }
                flag2 = (((SelectionMode)base.GetValue(ListBox.SelectionModeProperty)) == SelectionMode.Single) && !CollectionViewSource.IsDefaultView(base.Items.CollectionView);
            }
            this.IsSynchronizedWithCurrentItemPrivate = flag2;
            if (!isSynchronizedWithCurrentItemPrivate & flag2)
            {
                if (this.SelectedItem != null)
                {
                    this.SetCurrentToSelected();
                }
                else
                {
                    this.SetSelectedToCurrent();
                }
            }
        }

        internal static bool UiGetIsSelectable(DependencyObject o)
        {
            if (o != null)
            {
                if (!ItemGetIsSelectable(o))
                {
                    return false;
                }
                ItemsControl control = ItemsControlFromItemContainer(o);
                if (control != null)
                {
                    object item = control.ItemContainerGenerator.ItemFromContainer(o);
                    return ((item == o) || ItemGetIsSelectable(item));
                }
            }
            return false;
        }

        internal virtual void UnselectAllImpl()
        {
            this.SelectionChange.Begin();
            this.SelectionChange.CleanupDeferSelection();
            try
            {
                object internalSelectedItem = this.InternalSelectedItem;
                foreach (ItemsControl.ItemInfo info in (IEnumerable<ItemsControl.ItemInfo>)this._selectedItems)
                {
                    this.SelectionChange.Unselect(info);
                }
            }
            finally
            {
                this.SelectionChange.End();
            }
        }

        internal void UpdatePublicSelectionProperties()
        {
            EffectiveValueEntry entry = base.GetValueEntry(base.LookupEntry(SelectedIndexProperty.GlobalIndex), SelectedIndexProperty, null, RequestFlags.DeferredReferences);
            if (!entry.IsDeferredReference)
            {
                int num = (int)entry.Value;
                if (((num > (base.Items.Count - 1)) || ((num == -1) && (this._selectedItems.Count > 0))) || ((num > -1) && ((this._selectedItems.Count == 0) || (num != this._selectedItems[0].Index))))
                {
                    base.SetCurrentDeferredValue(SelectedIndexProperty, new DeferredSelectedIndexReference(this));
                }
            }
            if (this.SelectedItem != this.InternalSelectedItem)
            {
                try
                {
                    this.SkipCoerceSelectedItemCheck = true;
                    base.SetCurrentValueInternal(SelectedItemProperty, this.InternalSelectedItem);
                }
                finally
                {
                    this.SkipCoerceSelectedItemCheck = false;
                }
            }
            if (this._selectedItems.Count > 0)
            {
                this._cacheValid[0x20] = false;
            }
            if (!this._cacheValid[0x10] && !this._cacheValid[0x20])
            {
                object internalSelectedValue = this.InternalSelectedValue;
                if (internalSelectedValue == DependencyProperty.UnsetValue)
                {
                    internalSelectedValue = null;
                }
                if (!Equals(this.SelectedValue, internalSelectedValue))
                {
                    base.SetCurrentValueInternal(SelectedValueProperty, internalSelectedValue);
                }
            }
            this.UpdateSelectedItems();
        }

        private void UpdateSelectedItems()
        {
            SelectedItemCollection selectedItemsImpl = (SelectedItemCollection)this.SelectedItemsImpl;
            if (selectedItemsImpl != null)
            {
                InternalSelectedItemsStorage toAdd = new InternalSelectedItemsStorage(0, MatchExplicitEqualityComparer);
                InternalSelectedItemsStorage toRemove = new InternalSelectedItemsStorage(selectedItemsImpl.Count, MatchExplicitEqualityComparer);
                toAdd.UsesItemHashCodes = this._selectedItems.UsesItemHashCodes;
                toRemove.UsesItemHashCodes = this._selectedItems.UsesItemHashCodes;
                int num = 0;
                while (true)
                {
                    if (num >= selectedItemsImpl.Count)
                    {
                        using (toRemove.DeferRemove())
                        {
                            ItemsControl.ItemInfo e = new ItemsControl.ItemInfo(null, null, -1);
                            foreach (ItemsControl.ItemInfo info2 in (IEnumerable<ItemsControl.ItemInfo>)this._selectedItems)
                            {
                                e.Reset(info2.Item);
                                if (toRemove.Contains(e))
                                {
                                    toRemove.Remove(e);
                                    continue;
                                }
                                toAdd.Add(info2);
                            }
                        }
                        if ((toAdd.Count > 0) || (toRemove.Count > 0))
                        {
                            if (selectedItemsImpl.IsChanging)
                            {
                                ChangeInfoField.SetValue(this, new ChangeInfo(toAdd, toRemove));
                                return;
                            }
                            this.UpdateSelectedItems(toAdd, toRemove);
                        }
                        break;
                    }
                    toRemove.Add(selectedItemsImpl[num], ItemsControl.ItemInfo.SentinelContainer, ~num);
                    num++;
                }
            }
        }

        private void UpdateSelectedItems(InternalSelectedItemsStorage toAdd, InternalSelectedItemsStorage toRemove)
        {
            IList selectedItemsImpl = this.SelectedItemsImpl;
            ChangeInfoField.ClearValue(this);
            for (int i = 0; i < toAdd.Count; i++)
            {
                selectedItemsImpl.Add(toAdd[i].Item);
            }
            for (int j = toRemove.Count - 1; j >= 0; j--)
            {
                selectedItemsImpl.RemoveAt(~toRemove[j].Index);
            }
        }

        private static bool ValidateSelectedIndex(object o)
        {
            return (((int)o) >= -1);
        }

        private bool VerifyEqual(object knownValue, Type knownType, object itemValue, DynamicValueConverter converter)
        {
            object objA = knownValue;
            if ((knownType != null) && (itemValue != null))
            {
                Type c = itemValue.GetType();
                if (!knownType.IsAssignableFrom(c))
                {
                    objA = converter.Convert(knownValue, c);
                    if (objA == DependencyProperty.UnsetValue)
                    {
                        objA = knownValue;
                    }
                }
            }
            return Equals(objA, itemValue);
        }

        // Properties
        [Bindable(true), Category("Behavior"), TypeConverter("System.Windows.NullableBoolConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null"), Localizability(LocalizationCategory.NeverLocalize)]
        public bool? IsSynchronizedWithCurrentItem
        {
            get
            {
                return (bool?)base.GetValue(IsSynchronizedWithCurrentItemProperty);
            }
            set
            {
                base.SetValue(IsSynchronizedWithCurrentItemProperty, value);
            }
        }

        [Bindable(true), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizability(LocalizationCategory.NeverLocalize)]
        public int SelectedIndex
        {
            get
            {
                return (int)base.GetValue(SelectedIndexProperty);
            }
            set
            {
                base.SetValue(SelectedIndexProperty, value);
            }
        }

        [Bindable(true), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get
            {
                return base.GetValue(SelectedItemProperty);
            }
            set
            {
                base.SetValue(SelectedItemProperty, value);
            }
        }

        [Bindable(true), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizability(LocalizationCategory.NeverLocalize)]
        public object SelectedValue
        {
            get
            {
                return base.GetValue(SelectedValueProperty);
            }
            set
            {
                base.SetValue(SelectedValueProperty, value);
            }
        }

        [Bindable(true), Category("Appearance"), Localizability(LocalizationCategory.NeverLocalize)]
        public string SelectedValuePath
        {
            get
            {
                return (string)base.GetValue(SelectedValuePathProperty);
            }
            set
            {
                base.SetValue(SelectedValuePathProperty, value);
            }
        }

        internal IList SelectedItemsImpl
        {
            get
            {
                return (IList)base.GetValue(SelectedItemsImplProperty);
            }
        }

        internal bool CanSelectMultiple
        {
            get
            {
                return this._cacheValid[2];
            }
            set
            {
                if (this._cacheValid[2] != value)
                {
                    this._cacheValid[2] = value;
                    if (!value && (this._selectedItems.Count > 1))
                    {
                        this.SelectionChange.Validate();
                    }
                }
            }
        }

        private bool IsSynchronizedWithCurrentItemPrivate
        {
            get
            {
                return this._cacheValid[4];
            }
            set
            {
                this._cacheValid[4] = value;
            }
        }

        private bool SkipCoerceSelectedItemCheck
        {
            get
            {
                return this._cacheValid[8];
            }
            set
            {
                this._cacheValid[8] = value;
            }
        }

        internal SelectionChanger SelectionChange
        {
            get
            {
                if (this._selectionChangeInstance == null)
                {
                    this._selectionChangeInstance = new SelectionChanger(this);
                }
                return this._selectionChangeInstance;
            }
        }

        internal object InternalSelectedItem
        {
            get
            {
                return ((this._selectedItems.Count == 0) ? null : this._selectedItems[0].Item);
            }
        }

        internal ItemsControl.ItemInfo InternalSelectedInfo
        {
            get
            {
                return ((this._selectedItems.Count == 0) ? null : this._selectedItems[0]);
            }
        }

        internal int InternalSelectedIndex
        {
            get
            {
                if (this._selectedItems.Count == 0)
                {
                    return -1;
                }
                int index = this._selectedItems[0].Index;
                if (index < 0)
                {
                    index = base.Items.IndexOf(this._selectedItems[0].Item);
                    this._selectedItems[0].Index = index;
                }
                return index;
            }
        }

        private object InternalSelectedValue
        {
            get
            {
                object unsetValue;
                object internalSelectedItem = this.InternalSelectedItem;
                if (internalSelectedItem == null)
                {
                    unsetValue = DependencyProperty.UnsetValue;
                }
                else
                {
                    BindingExpression expression = this.PrepareItemValueBinding(internalSelectedItem);
                    if (string.IsNullOrEmpty(this.SelectedValuePath))
                    {
                        unsetValue = !string.IsNullOrEmpty(expression.ParentBinding.Path.Path) ? SystemXmlHelper.GetInnerText(internalSelectedItem) : internalSelectedItem;
                    }
                    else
                    {
                        expression.Activate(internalSelectedItem);
                        unsetValue = expression.Value;
                        expression.Deactivate();
                    }
                }
                return unsetValue;
            }
        }

        // Nested Types
        [Flags]
        private enum CacheBits
        {
            SyncingSelectionAndCurrency = 1,
            CanSelectMultiple = 2,
            IsSynchronizedWithCurrentItem = 4,
            SkipCoerceSelectedItemCheck = 8,
            SelectedValueDrivesSelection = 0x10,
            SelectedValueWaitsForItems = 0x20,
            NewContainersArePending = 0x40
        }

        private class ChangeInfo
        {
            // Methods
            public ChangeInfo(MySelector.InternalSelectedItemsStorage toAdd, MySelector.InternalSelectedItemsStorage toRemove)
            {
                this.ToAdd = toAdd;
                this.ToRemove = toRemove;
            }

            // Properties
            public MySelector.InternalSelectedItemsStorage ToAdd { get; private set; }

            public MySelector.InternalSelectedItemsStorage ToRemove { get; private set; }
        }

        internal class InternalSelectedItemsStorage : IEnumerable<ItemsControl.ItemInfo>, IEnumerable
        {
            // Fields
            private List<ItemsControl.ItemInfo> _list;
            private Dictionary<ItemsControl.ItemInfo, ItemsControl.ItemInfo> _set;
            private IEqualityComparer<ItemsControl.ItemInfo> _equalityComparer;
            private int _resolvedCount;
            private int _unresolvedCount;
            private BatchRemoveHelper _batchRemove;

            // Methods
            internal InternalSelectedItemsStorage(int capacity, IEqualityComparer<ItemsControl.ItemInfo> equalityComparer)
            {
                this._equalityComparer = equalityComparer;
                this._list = new List<ItemsControl.ItemInfo>(capacity);
                this._set = new Dictionary<ItemsControl.ItemInfo, ItemsControl.ItemInfo>(capacity, equalityComparer);
            }

            internal InternalSelectedItemsStorage(MySelector.InternalSelectedItemsStorage collection, IEqualityComparer<ItemsControl.ItemInfo> equalityComparer = null)
            {
                this._equalityComparer = equalityComparer ?? collection._equalityComparer;
                this._list = new List<ItemsControl.ItemInfo>(collection._list);
                if (collection.UsesItemHashCodes)
                {
                    this._set = new Dictionary<ItemsControl.ItemInfo, ItemsControl.ItemInfo>(collection._set, this._equalityComparer);
                }
                this._resolvedCount = collection._resolvedCount;
                this._unresolvedCount = collection._unresolvedCount;
            }

            public void Add(ItemsControl.ItemInfo info)
            {
                if (this._set != null)
                {
                    this._set.Add(info, info);
                }
                this._list.Add(info);
                if (info.IsResolved)
                {
                    this._resolvedCount++;
                }
                else
                {
                    this._unresolvedCount++;
                }
            }

            public void Add(object item, DependencyObject container, int index)
            {
                this.Add(new ItemsControl.ItemInfo(item, container, index));
            }

            public void Clear()
            {
                this._list.Clear();
                if (this._set != null)
                {
                    this._set.Clear();
                }
                this._resolvedCount = this._unresolvedCount = 0;
            }

            public bool Contains(ItemsControl.ItemInfo e)
            {
                return ((this._set == null) ? (this.IndexInList(e) >= 0) : this._set.ContainsKey(e));
            }

            public IDisposable DeferRemove()
            {
                if (this._batchRemove == null)
                {
                    this._batchRemove = new BatchRemoveHelper(this);
                }
                this._batchRemove.Enter();
                return this._batchRemove;
            }

            private void DoBatchRemove()
            {
                int index = 0;
                int count = this._list.Count;
                for (int i = 0; i < count; i++)
                {
                    ItemsControl.ItemInfo info = this._list[i];
                    if (!info.IsRemoved)
                    {
                        if (index < i)
                        {
                            this._list[index] = this._list[i];
                        }
                        index++;
                    }
                }
                this._list.RemoveRange(index, count - index);
            }

            public ItemsControl.ItemInfo FindMatch(ItemsControl.ItemInfo info)
            {
                ItemsControl.ItemInfo info2;
                if (this._set == null)
                {
                    int num = this.IndexInList(info);
                    info2 = (num < 0) ? null : this._list[num];
                }
                else if (!this._set.TryGetValue(info, out info2))
                {
                    info2 = null;
                }
                return info2;
            }

            private int IndexInList(ItemsControl.ItemInfo info)
            {
                return this._list.FindIndex(x => this._equalityComparer.Equals(info, x));
            }

            private int LastIndexInList(ItemsControl.ItemInfo info)
            {
                return this._list.FindLastIndex(x => this._equalityComparer.Equals(info, x));
            }

            public bool Remove(ItemsControl.ItemInfo e)
            {
                bool flag = false;
                bool isResolved = false;
                if (this._set == null)
                {
                    flag = this.RemoveFromList(e);
                }
                else
                {
                    ItemsControl.ItemInfo info;
                    if (this._set.TryGetValue(e, out info))
                    {
                        flag = true;
                        isResolved = info.IsResolved;
                        this._set.Remove(e);
                        if (!this.RemoveIsDeferred)
                        {
                            this.RemoveFromList(e);
                        }
                        else
                        {
                            info.Container = ItemsControl.ItemInfo.RemovedContainer;
                            this._batchRemove.RemovedCount++;
                        }
                    }
                }
                if (flag)
                {
                    if (isResolved)
                    {
                        this._resolvedCount--;
                    }
                    else
                    {
                        this._unresolvedCount--;
                    }
                }
                return flag;
            }

            private bool RemoveFromList(ItemsControl.ItemInfo e)
            {
                bool flag = false;
                int index = this.LastIndexInList(e);
                if (index >= 0)
                {
                    this._list.RemoveAt(index);
                    flag = true;
                }
                return flag;
            }

            IEnumerator<ItemsControl.ItemInfo> IEnumerable<ItemsControl.ItemInfo>.GetEnumerator()
            {
                return this._list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._list.GetEnumerator();
            }

            // Properties
            public ItemsControl.ItemInfo this[int index]
            {
                get
                {
                    return this._list[index];
                }
            }

            public int Count
            {
                get
                {
                    return this._list.Count;
                }
            }

            public bool RemoveIsDeferred
            {
                get
                {
                    return ((this._batchRemove != null) && this._batchRemove.IsActive);
                }
            }

            public int ResolvedCount
            {
                get
                {
                    return this._resolvedCount;
                }
            }

            public int UnresolvedCount
            {
                get
                {
                    return this._unresolvedCount;
                }
            }

            public bool UsesItemHashCodes
            {
                get
                {
                    return (this._set != null);
                }
                set
                {
                    if (!value || (this._set != null))
                    {
                        if (!value)
                        {
                            this._set = null;
                        }
                    }
                    else
                    {
                        this._set = new Dictionary<ItemsControl.ItemInfo, ItemsControl.ItemInfo>(this._list.Count);
                        for (int i = 0; i < this._list.Count; i++)
                        {
                            this._set.Add(this._list[i], this._list[i]);
                        }
                    }
                }
            }

            // Nested Types
            private class BatchRemoveHelper : IDisposable
            {
                // Fields
                private MySelector.InternalSelectedItemsStorage _owner;
                private int _level;

                // Methods
                public BatchRemoveHelper(MySelector.InternalSelectedItemsStorage owner)
                {
                    this._owner = owner;
                }

                public void Dispose()
                {
                    this.Leave();
                }

                public void Enter()
                {
                    this._level++;
                }

                public void Leave()
                {
                    if (this._level > 0)
                    {
                        int num = this._level - 1;
                        this._level = num;
                        if ((num == 0) && (this.RemovedCount > 0))
                        {
                            this._owner.DoBatchRemove();
                            this.RemovedCount = 0;
                        }
                    }
                }

                // Properties
                public bool IsActive
                {
                    get
                    {
                        return (this._level > 0);
                    }
                }

                public int RemovedCount { get; set; }
            }
        }

        private class ItemInfoEqualityComparer : IEqualityComparer<ItemsControl.ItemInfo>
        {
            // Fields
            private bool _matchUnresolved;

            // Methods
            public ItemInfoEqualityComparer(bool matchUnresolved)
            {
                this._matchUnresolved = matchUnresolved;
            }

            bool IEqualityComparer<ItemsControl.ItemInfo>.Equals(ItemsControl.ItemInfo x, ItemsControl.ItemInfo y)
            {
                return (!ReferenceEquals(x, y) ? ((x == null) ? (y == null) : x.Equals(y, this._matchUnresolved)) : true);
            }

            int IEqualityComparer<ItemsControl.ItemInfo>.GetHashCode(ItemsControl.ItemInfo x)
            {
                return x.GetHashCode();
            }
        }

        internal class SelectionChanger
        {
            // Fields
            private MySelector _owner;
            private MySelector.InternalSelectedItemsStorage _toSelect;
            private MySelector.InternalSelectedItemsStorage _toUnselect;
            private MySelector.InternalSelectedItemsStorage _toDeferSelect;
            private bool _active;

            // Methods
            internal SelectionChanger(MySelector s)
            {
                this._owner = s;
                this._active = false;
                this._toSelect = new MySelector.InternalSelectedItemsStorage(1, MySelector.MatchUnresolvedEqualityComparer);
                this._toUnselect = new MySelector.InternalSelectedItemsStorage(1, MySelector.MatchUnresolvedEqualityComparer);
                this._toDeferSelect = new MySelector.InternalSelectedItemsStorage(1, MySelector.MatchUnresolvedEqualityComparer);
            }

            private void ApplyCanSelectMultiple()
            {
                if (!this._owner.CanSelectMultiple)
                {
                    if (this._toSelect.Count == 1)
                    {
                        this._toUnselect = new MySelector.InternalSelectedItemsStorage(this._owner._selectedItems, null);
                    }
                    else if ((this._owner._selectedItems.Count > 1) && (this._owner._selectedItems.Count != (this._toUnselect.Count + 1)))
                    {
                        ItemsControl.ItemInfo info = this._owner._selectedItems[0];
                        this._toUnselect.Clear();
                        foreach (ItemsControl.ItemInfo info2 in (IEnumerable<ItemsControl.ItemInfo>)this._owner._selectedItems)
                        {
                            if (info2 != info)
                            {
                                this._toUnselect.Add(info2);
                            }
                        }
                    }
                }
            }

            internal void Begin()
            {
                this._active = true;
                this._toSelect.Clear();
                this._toUnselect.Clear();
            }

            internal void Cancel()
            {
                this.Cleanup();
            }

            internal void Cleanup()
            {
                this._active = false;
                if (this._toSelect.Count > 0)
                {
                    this._toSelect.Clear();
                }
                if (this._toUnselect.Count > 0)
                {
                    this._toUnselect.Clear();
                }
            }

            internal void CleanupDeferSelection()
            {
                if (this._toDeferSelect.Count > 0)
                {
                    this._toDeferSelect.Clear();
                }
            }

            private void CreateDeltaSelectionChange(List<ItemsControl.ItemInfo> unselectedItems, List<ItemsControl.ItemInfo> selectedItems)
            {
                int num = 0;
                while (true)
                {
                    if (num < this._toDeferSelect.Count)
                    {
                        ItemsControl.ItemInfo info = this._toDeferSelect[num];
                        if (this._owner.Items.Contains(info.Item))
                        {
                            this._toSelect.Add(info);
                            this._toDeferSelect.Remove(info);
                            num--;
                        }
                        num++;
                        continue;
                    }
                    if ((this._toUnselect.Count > 0) || (this._toSelect.Count > 0))
                    {
                        using (this._owner._selectedItems.DeferRemove())
                        {
                            if (this._toUnselect.ResolvedCount > 0)
                            {
                                foreach (ItemsControl.ItemInfo info2 in (IEnumerable<ItemsControl.ItemInfo>)this._toUnselect)
                                {
                                    if (!info2.IsResolved)
                                    {
                                        continue;
                                    }
                                    this._owner.ItemSetIsSelected(info2, false);
                                    if (this._owner._selectedItems.Remove(info2))
                                    {
                                        unselectedItems.Add(info2);
                                    }
                                }
                            }
                            if (this._toUnselect.UnresolvedCount > 0)
                            {
                                foreach (ItemsControl.ItemInfo info3 in (IEnumerable<ItemsControl.ItemInfo>)this._toUnselect)
                                {
                                    if (info3.IsResolved)
                                    {
                                        continue;
                                    }
                                    ItemsControl.ItemInfo info = this._owner._selectedItems.FindMatch(ItemsControl.ItemInfo.Key(info3));
                                    if (info != null)
                                    {
                                        this._owner.ItemSetIsSelected(info, false);
                                        this._owner._selectedItems.Remove(info);
                                        unselectedItems.Add(info);
                                    }
                                }
                            }
                        }
                        break;
                    }
                    return;
                }
                using (this._toSelect.DeferRemove())
                {
                    if (this._toSelect.ResolvedCount > 0)
                    {
                        List<ItemsControl.ItemInfo> list = (this._toSelect.UnresolvedCount > 0) ? new List<ItemsControl.ItemInfo>() : null;
                        foreach (ItemsControl.ItemInfo info5 in (IEnumerable<ItemsControl.ItemInfo>)this._toSelect)
                        {
                            if (info5.IsResolved)
                            {
                                this._owner.ItemSetIsSelected(info5, true);
                                if (!this._owner._selectedItems.Contains(info5))
                                {
                                    this._owner._selectedItems.Add(info5);
                                    selectedItems.Add(info5);
                                }
                                if (list != null)
                                {
                                    list.Add(info5);
                                }
                            }
                        }
                        if (list != null)
                        {
                            foreach (ItemsControl.ItemInfo info6 in list)
                            {
                                this._toSelect.Remove(info6);
                            }
                        }
                    }
                    for (int i = 0; (this._toSelect.UnresolvedCount > 0) && (i < this._owner.Items.Count); i++)
                    {
                        ItemsControl.ItemInfo e = this._owner.NewItemInfo(this._owner.Items[i], null, i);
                        ItemsControl.ItemInfo info8 = new ItemsControl.ItemInfo(e.Item, ItemsControl.ItemInfo.KeyContainer, -1);
                        if (this._toSelect.Contains(info8) && !this._owner._selectedItems.Contains(e))
                        {
                            this._owner.ItemSetIsSelected(e, true);
                            this._owner._selectedItems.Add(e);
                            selectedItems.Add(e);
                            this._toSelect.Remove(info8);
                        }
                    }
                }
            }

            internal void End()
            {
                List<ItemsControl.ItemInfo> unselectedItems = new List<ItemsControl.ItemInfo>();
                List<ItemsControl.ItemInfo> selectedItems = new List<ItemsControl.ItemInfo>();
                try
                {
                    this.ApplyCanSelectMultiple();
                    this.CreateDeltaSelectionChange(unselectedItems, selectedItems);
                    this._owner.UpdatePublicSelectionProperties();
                }
                finally
                {
                    this.Cleanup();
                }
                if ((unselectedItems.Count > 0) || (selectedItems.Count > 0))
                {
                    if (this._owner.IsSynchronizedWithCurrentItemPrivate)
                    {
                        this._owner.SetCurrentToSelected();
                    }
                    this._owner.InvokeSelectionChanged(unselectedItems, selectedItems);
                }
            }

            internal bool Select(ItemsControl.ItemInfo info, bool assumeInItemsCollection)
            {
                if (!MySelector.ItemGetIsSelectable(info))
                {
                    return false;
                }
                if (!assumeInItemsCollection && !this._owner.Items.Contains(info.Item))
                {
                    if (!this._toDeferSelect.Contains(info))
                    {
                        this._toDeferSelect.Add(info);
                    }
                    return false;
                }
                ItemsControl.ItemInfo e = ItemsControl.ItemInfo.Key(info);
                if (!this._toUnselect.Remove(e))
                {
                    if (this._owner._selectedItems.Contains(info))
                    {
                        return false;
                    }
                    if (!e.IsKey && this._toSelect.Contains(e))
                    {
                        return false;
                    }
                    if (!this._owner.CanSelectMultiple && (this._toSelect.Count > 0))
                    {
                        foreach (ItemsControl.ItemInfo info3 in (IEnumerable<ItemsControl.ItemInfo>)this._toSelect)
                        {
                            this._owner.ItemSetIsSelected(info3, false);
                        }
                        this._toSelect.Clear();
                    }
                    this._toSelect.Add(info);
                }
                return true;
            }

            internal void SelectJustThisItem(ItemsControl.ItemInfo info, bool assumeInItemsCollection)
            {
                this.Begin();
                this.CleanupDeferSelection();
                try
                {
                    bool flag = false;
                    int num = this._owner._selectedItems.Count - 1;
                    while (true)
                    {
                        if (num < 0)
                        {
                            if ((!flag && (info != null)) && (info.Item != DependencyProperty.UnsetValue))
                            {
                                this.Select(info, assumeInItemsCollection);
                            }
                            break;
                        }
                        if (info != this._owner._selectedItems[num])
                        {
                            this.Unselect(this._owner._selectedItems[num]);
                        }
                        else
                        {
                            flag = true;
                        }
                        num--;
                    }
                }
                finally
                {
                    this.End();
                }
            }

            internal bool Unselect(ItemsControl.ItemInfo info)
            {
                ItemsControl.ItemInfo e = ItemsControl.ItemInfo.Key(info);
                this._toDeferSelect.Remove(info);
                if (!this._toSelect.Remove(e))
                {
                    if (!this._owner._selectedItems.Contains(e))
                    {
                        return false;
                    }
                    if (this._toUnselect.Contains(info))
                    {
                        return false;
                    }
                    this._toUnselect.Add(info);
                }
                return true;
            }

            internal void Validate()
            {
                this.Begin();
                this.End();
            }

            // Properties
            internal bool IsActive
            {
                get
                {
                    return this._active;
                }
            }
        }
    }




}
