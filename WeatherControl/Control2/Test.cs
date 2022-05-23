using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace WeatherControl.Control2
{
    [Localizability(LocalizationCategory.ListBox), StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListBoxItem))]
    public class Test:MySelector
    {
    
            // Fields
            public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(Test), new FrameworkPropertyMetadata(SelectionMode.Single, new PropertyChangedCallback(Test.OnSelectionModeChanged)), new ValidateValueCallback(Test.IsValidSelectionMode));
            public static readonly DependencyProperty SelectedItemsProperty = Selector.SelectedItemsImplProperty;
            private ItemsControl.ItemInfo _anchorItem;
            private WeakReference _lastActionItem;
            private DispatcherTimer _autoScrollTimer;
            private static RoutedUICommand SelectAllCommand = new RoutedUICommand(SR.Get("ListBoxSelectAllText"), "SelectAll", typeof(Test));
            private static DependencyObjectType _dType;

            // Methods
            static Test()
            {
                FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Test), new FrameworkPropertyMetadata(typeof(Test)));
                _dType = DependencyObjectType.FromSystemTypeInternal(typeof(Test));
                Control.IsTabStopProperty.OverrideMetadata(typeof(Test), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
                KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(Test), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
                KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(Test), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
                ItemsControl.IsTextSearchEnabledProperty.OverrideMetadata(typeof(Test), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
                ItemsPanelTemplate defaultValue = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));
                defaultValue.Seal();
                ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(Test), new FrameworkPropertyMetadata(defaultValue));
                EventManager.RegisterClassHandler(typeof(Test), Mouse.MouseUpEvent, new MouseButtonEventHandler(Test.OnMouseButtonUp), true);
                EventManager.RegisterClassHandler(typeof(Test), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(Test.OnGotKeyboardFocus));
                CommandHelpers.RegisterCommandHandler(typeof(Test), SelectAllCommand, new ExecutedRoutedEventHandler(Test.OnSelectAll), new CanExecuteRoutedEventHandler(Test.OnQueryStatusSelectAll), (InputGesture)KeyGesture.CreateFromResourceStrings(SR.Get("ListBoxSelectAllKey"), SR.Get("ListBoxSelectAllKeyDisplayString")));
                ControlsTraceLogger.AddControl(TelemetryControls.Test);
            }

            public Test()
            {
                this.Initialize();
            }

            internal override void AdjustItemInfoOverride(NotifyCollectionChangedEventArgs e)
            {
                base.AdjustItemInfo(e, this._anchorItem);
                if ((this._anchorItem != null) && (this._anchorItem.Index < 0))
                {
                    this._anchorItem = null;
                }
                base.AdjustItemInfoOverride(e);
            }

            internal override void AdjustItemInfosAfterGeneratorChangeOverride()
            {
                base.AdjustItemInfoAfterGeneratorChange(this._anchorItem);
                base.AdjustItemInfosAfterGeneratorChangeOverride();
            }

            private ListBoxItem ElementAt(int index)
            {
                return (base.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem);
            }

            private int ElementIndex(ListBoxItem listItem)
            {
                return base.ItemContainerGenerator.IndexFromContainer(listItem);
            }

            internal override bool FocusItem(ItemsControl.ItemInfo info, ItemsControl.ItemNavigateArgs itemNavigateArgs)
            {
                bool flag = base.FocusItem(info, itemNavigateArgs);
                ListBoxItem container = info.Container as ListBoxItem;
                if (container != null)
                {
                    this.LastActionItem = container;
                    this.MakeKeyboardSelection(container);
                }
                return flag;
            }

            protected override DependencyObject GetContainerForItemOverride()
            {
                return new ListBoxItem();
            }

            private object GetWeakReferenceTarget(ref WeakReference weakReference)
            {
                return ((weakReference == null) ? null : weakReference.Target);
            }

            private void Initialize()
            {
                SelectionMode defaultValue = (SelectionMode)SelectionModeProperty.GetDefaultValue(base.DependencyObjectType);
                this.ValidateSelectionMode(defaultValue);
            }

            protected override bool IsItemItsOwnContainerOverride(object item)
            {
                return (item is ListBoxItem);
            }

            private static bool IsValidSelectionMode(object o)
            {
                SelectionMode mode = (SelectionMode)o;
                return ((mode == SelectionMode.Single) || ((mode == SelectionMode.Multiple) || (mode == SelectionMode.Extended)));
            }

            private void MakeAnchorSelection(ListBoxItem actionItem, bool clearCurrent)
            {
                ItemsControl.ItemInfo anchorItemInternal;
                if (this.AnchorItemInternal == null)
                {
                    this.AnchorItemInternal = (base._selectedItems.Count <= 0) ? base.NewItemInfo(base.Items[0], null, 0) : base._selectedItems[base._selectedItems.Count - 1];
                    anchorItemInternal = this.AnchorItemInternal;
                    if (anchorItemInternal == null)
                    {
                        return;
                    }
                }
                int num = this.ElementIndex(actionItem);
                int index = this.AnchorItemInternal.Index;
                if (num > index)
                {
                    num = index;
                    index = num;
                }
                bool flag = false;
                if (!base.SelectionChange.IsActive)
                {
                    flag = true;
                    base.SelectionChange.Begin();
                }
                try
                {
                    if (clearCurrent)
                    {
                        for (int i = 0; i < base._selectedItems.Count; i++)
                        {
                            ItemsControl.ItemInfo info2 = base._selectedItems[i];
                            int num5 = info2.Index;
                            if ((num5 < num) || (index < num5))
                            {
                                base.SelectionChange.Unselect(info2);
                            }
                        }
                    }
                    IEnumerator enumerator = ((IEnumerable)base.Items).GetEnumerator();
                    int num6 = 0;
                    while (true)
                    {
                        if (num6 > index)
                        {
                            IDisposable disposable = enumerator as IDisposable;
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                            break;
                        }
                        enumerator.MoveNext();
                        if (num6 >= num)
                        {
                            base.SelectionChange.Select(base.NewItemInfo(enumerator.Current, null, num6), true);
                        }
                        num6++;
                    }
                }
                finally
                {
                    if (flag)
                    {
                        base.SelectionChange.End();
                    }
                }
                this.LastActionItem = actionItem;
                GC.KeepAlive(anchorItemInternal);
            }

            private void MakeKeyboardSelection(ListBoxItem item)
            {
                if (item != null)
                {
                    switch (this.SelectionMode)
                    {
                        case SelectionMode.Single:
                            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
                            {
                                break;
                            }
                            this.MakeSingleSelection(item);
                            return;

                        case SelectionMode.Multiple:
                            this.UpdateAnchorAndActionItem(base.ItemInfoFromContainer(item));
                            return;

                        case SelectionMode.Extended:
                            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            {
                                bool clearCurrent = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.None;
                                this.MakeAnchorSelection(item, clearCurrent);
                                return;
                            }
                            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.None)
                            {
                                this.MakeSingleSelection(item);
                            }
                            break;

                        default:
                            return;
                    }
                }
            }

            private void MakeSingleSelection(ListBoxItem listItem)
            {
                if (ReferenceEquals(ItemsControlFromItemContainer(listItem), this))
                {
                    ItemsControl.ItemInfo info = base.ItemInfoFromContainer(listItem);
                    base.SelectionChange.SelectJustThisItem(info, true);
                    listItem.Focus();
                    this.UpdateAnchorAndActionItem(info);
                }
            }

            private void MakeToggleSelection(ListBoxItem item)
            {
                bool flag = !item.IsSelected;
                item.SetCurrentValueInternal(Selector.IsSelectedProperty, BooleanBoxes.Box(flag));
                this.UpdateAnchorAndActionItem(base.ItemInfoFromContainer(item));
            }

            internal void NotifyListItemClicked(ListBoxItem item, MouseButton mouseButton)
            {
                if ((mouseButton == MouseButton.Left) && !ReferenceEquals(Mouse.Captured, this))
                {
                    Mouse.Capture(this, CaptureMode.SubTree);
                    base.SetInitialMousePosition();
                }
                switch (this.SelectionMode)
                {
                    case SelectionMode.Single:
                        if (!item.IsSelected)
                        {
                            item.SetCurrentValueInternal(Selector.IsSelectedProperty, BooleanBoxes.TrueBox);
                        }
                        else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            item.SetCurrentValueInternal(Selector.IsSelectedProperty, BooleanBoxes.FalseBox);
                        }
                        this.UpdateAnchorAndActionItem(base.ItemInfoFromContainer(item));
                        return;

                    case SelectionMode.Multiple:
                        this.MakeToggleSelection(item);
                        return;

                    case SelectionMode.Extended:
                        if (mouseButton != MouseButton.Left)
                        {
                            if ((mouseButton == MouseButton.Right) && ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.None))
                            {
                                if (item.IsSelected)
                                {
                                    this.UpdateAnchorAndActionItem(base.ItemInfoFromContainer(item));
                                    return;
                                }
                                this.MakeSingleSelection(item);
                            }
                            return;
                        }
                        if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == (ModifierKeys.Shift | ModifierKeys.Control))
                        {
                            this.MakeAnchorSelection(item, false);
                            return;
                        }
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            this.MakeToggleSelection(item);
                            return;
                        }
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                        {
                            this.MakeAnchorSelection(item, true);
                            return;
                        }
                        this.MakeSingleSelection(item);
                        return;
                }
            }

            internal void NotifyListItemMouseDragged(ListBoxItem listItem)
            {
                if (ReferenceEquals(Mouse.Captured, this) && base.DidMouseMove())
                {
                    base.NavigateToItem(base.ItemInfoFromContainer(listItem), new ItemsControl.ItemNavigateArgs(Mouse.PrimaryDevice, Keyboard.Modifiers), false);
                }
            }

            private void OnAutoScrollTimeout(object sender, EventArgs e)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    base.DoAutoScroll();
                }
            }

            protected override AutomationPeer OnCreateAutomationPeer()
            {
                return new ListBoxAutomationPeer(this);
            }

            private static object OnGetSelectionMode(DependencyObject d)
            {
                return ((Test)d).SelectionMode;
            }

            private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            {
                Test objB = (Test)sender;
                if (KeyboardNavigation.IsKeyboardMostRecentInputDevice())
                {
                    ListBoxItem newFocus = e.NewFocus as ListBoxItem;
                    if ((newFocus != null) && ReferenceEquals(ItemsControlFromItemContainer(newFocus), objB))
                    {
                        DependencyObject oldFocus = e.OldFocus as DependencyObject;
                        Visual descendant = oldFocus as Visual;
                        if (descendant == null)
                        {
                            ContentElement ce = oldFocus as ContentElement;
                            if (ce != null)
                            {
                                descendant = KeyboardNavigation.GetParentUIElementFromContentElement(ce);
                            }
                        }
                        if (((descendant != null) && objB.IsAncestorOf(descendant)) || ReferenceEquals(oldFocus, objB))
                        {
                            objB.LastActionItem = newFocus;
                            objB.MakeKeyboardSelection(newFocus);
                        }
                    }
                }
            }

            protected override void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e)
            {
                if (!base.IsMouseCaptured)
                {
                    if (this._autoScrollTimer != null)
                    {
                        this._autoScrollTimer.Stop();
                        this._autoScrollTimer = null;
                    }
                }
                else if (this._autoScrollTimer == null)
                {
                    this._autoScrollTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                    this._autoScrollTimer.Interval = ItemsControl.AutoScrollTimeout;
                    this._autoScrollTimer.Tick += new EventHandler(this.OnAutoScrollTimeout);
                    this._autoScrollTimer.Start();
                }
                base.OnIsMouseCapturedChanged(e);
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                bool flag = true;
                Key key = e.Key;
                if (key > Key.Down)
                {
                    if ((key != Key.Divide) && (key != Key.Oem2))
                    {
                        if (key == Key.Oem5)
                        {
                            if (((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) || (this.SelectionMode != SelectionMode.Extended))
                            {
                                flag = false;
                            }
                            else
                            {
                                ListBoxItem listItem = (base.FocusedInfo != null) ? (base.FocusedInfo.Container as ListBoxItem) : null;
                                if (listItem != null)
                                {
                                    this.MakeSingleSelection(listItem);
                                }
                            }
                            goto TR_0002;
                        }
                    }
                    else
                    {
                        if (((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) || (this.SelectionMode != SelectionMode.Extended))
                        {
                            flag = false;
                        }
                        else
                        {
                            this.SelectAll();
                        }
                        goto TR_0002;
                    }
                }
                else
                {
                    if (key != Key.Return)
                    {
                        switch (key)
                        {
                            case Key.Space:
                                break;

                            case Key.Prior:
                                base.NavigateByPage(FocusNavigationDirection.Up, new ItemsControl.ItemNavigateArgs(e.Device, Keyboard.Modifiers));
                                goto TR_0002;

                            case Key.Next:
                                base.NavigateByPage(FocusNavigationDirection.Down, new ItemsControl.ItemNavigateArgs(e.Device, Keyboard.Modifiers));
                                goto TR_0002;

                            case Key.End:
                                base.NavigateToEnd(new ItemsControl.ItemNavigateArgs(e.Device, Keyboard.Modifiers));
                                goto TR_0002;

                            case Key.Home:
                                base.NavigateToStart(new ItemsControl.ItemNavigateArgs(e.Device, Keyboard.Modifiers));
                                goto TR_0002;

                            case Key.Left:
                            case Key.Up:
                            case Key.Right:
                            case Key.Down:
                                {
                                    KeyboardNavigation.ShowFocusVisual();
                                    bool flag2 = base.ScrollHost != null;
                                    if (flag2)
                                    {
                                        int num1;
                                        int num2;
                                        if (((((key == Key.Down) && base.IsLogicalHorizontal) && DoubleUtil.GreaterThan(base.ScrollHost.ScrollableHeight, base.ScrollHost.VerticalOffset)) || (((key == Key.Up) && base.IsLogicalHorizontal) && DoubleUtil.GreaterThan(base.ScrollHost.VerticalOffset, 0.0))) || (((key == Key.Right) && base.IsLogicalVertical) && DoubleUtil.GreaterThan(base.ScrollHost.ScrollableWidth, base.ScrollHost.HorizontalOffset)))
                                        {
                                            num1 = 1;
                                        }
                                        else if ((key != Key.Left) || !base.IsLogicalVertical)
                                        {
                                            num1 = 0;
                                        }
                                        else
                                        {
                                            num1 = (int)DoubleUtil.GreaterThan(base.ScrollHost.HorizontalOffset, 0.0);
                                        }
                                        flag2 = (bool)num2;
                                    }
                                    if (flag2)
                                    {
                                        base.ScrollHost.ScrollInDirection(e);
                                    }
                                    else if (((base.ItemsHost == null) || !base.ItemsHost.IsKeyboardFocusWithin) && !base.IsKeyboardFocused)
                                    {
                                        flag = false;
                                    }
                                    else if (!base.NavigateByLine(KeyboardNavigation.KeyToTraversalDirection(key), new ItemsControl.ItemNavigateArgs(e.Device, Keyboard.Modifiers)))
                                    {
                                        flag = false;
                                    }
                                    goto TR_0002;
                                }
                            default:
                                goto TR_0003;
                        }
                    }
                    if ((e.Key == Key.Return) && !((bool)base.GetValue(KeyboardNavigation.AcceptsReturnProperty)))
                    {
                        flag = false;
                    }
                    else
                    {
                        ListBoxItem originalSource = e.OriginalSource as ListBoxItem;
                        if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.Alt)
                        {
                            if (base.IsTextSearchEnabled && (Keyboard.Modifiers == ModifierKeys.None))
                            {
                                TextSearch search = TextSearch.EnsureInstance(this);
                                if ((search != null) && (search.GetCurrentPrefix() != string.Empty))
                                {
                                    flag = false;
                                    goto TR_0002;
                                }
                            }
                            if ((originalSource == null) || !ReferenceEquals(ItemsControlFromItemContainer(originalSource), this))
                            {
                                flag = false;
                            }
                            else
                            {
                                switch (this.SelectionMode)
                                {
                                    case SelectionMode.Single:
                                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                                        {
                                            this.MakeToggleSelection(originalSource);
                                        }
                                        else
                                        {
                                            this.MakeSingleSelection(originalSource);
                                        }
                                        break;

                                    case SelectionMode.Multiple:
                                        this.MakeToggleSelection(originalSource);
                                        break;

                                    case SelectionMode.Extended:
                                        if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.Control)
                                        {
                                            this.MakeToggleSelection(originalSource);
                                        }
                                        else if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.Shift)
                                        {
                                            this.MakeAnchorSelection(originalSource, true);
                                        }
                                        else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.None)
                                        {
                                            this.MakeSingleSelection(originalSource);
                                        }
                                        else
                                        {
                                            flag = false;
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    goto TR_0002;
                }
                goto TR_0003;
            TR_0002:
                if (flag)
                {
                    e.Handled = true;
                    return;
                }
                base.OnKeyDown(e);
                return;
            TR_0003:
                flag = false;
                goto TR_0002;
            }

            private static void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Test box = (Test)sender;
                    box.ReleaseMouseCapture();
                    box.ResetLastMousePosition();
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                if ((e.OriginalSource == this) && ReferenceEquals(Mouse.Captured, this))
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        base.DoAutoScroll();
                    }
                    else
                    {
                        base.ReleaseMouseCapture();
                        base.ResetLastMousePosition();
                    }
                }
                base.OnMouseMove(e);
            }

            private static void OnQueryStatusSelectAll(object target, CanExecuteRoutedEventArgs args)
            {
                if ((target as Test).SelectionMode == SelectionMode.Extended)
                {
                    args.CanExecute = true;
                }
            }

            private static void OnSelectAll(object target, ExecutedRoutedEventArgs args)
            {
                Test box = target as Test;
                if (box.SelectionMode == SelectionMode.Extended)
                {
                    box.SelectAll();
                }
            }

            protected override void OnSelectionChanged(SelectionChangedEventArgs e)
            {
                base.OnSelectionChanged(e);
                if (this.SelectionMode == SelectionMode.Single)
                {
                    ItemsControl.ItemInfo internalSelectedInfo = base.InternalSelectedInfo;
                    if (((internalSelectedInfo != null) ? (internalSelectedInfo.Container as ListBoxItem) : null) != null)
                    {
                        this.UpdateAnchorAndActionItem(internalSelectedInfo);
                    }
                }
                if ((AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated) || (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection))) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
                {
                    ListBoxAutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(this) as ListBoxAutomationPeer;
                    if (peer != null)
                    {
                        peer.RaiseSelectionEvents(e);
                    }
                }
            }

            private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                Test box = (Test)d;
                box.ValidateSelectionMode(box.SelectionMode);
            }

            protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
            {
                base.PrepareContainerForItemOverride(element, item);
                if (item is Separator)
                {
                    Separator.PrepareContainer(element as Control);
                }
            }

            public void ScrollIntoView(object item)
            {
                if (base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    base.OnBringItemIntoView(item);
                }
                else
                {
                    base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(this.OnBringItemIntoView), item);
                }
            }

            public void SelectAll()
            {
                if (!base.CanSelectMultiple)
                {
                    throw new NotSupportedException(SR.Get("ListBoxSelectAllSelectionMode"));
                }
                this.SelectAllImpl();
            }

            protected bool SetSelectedItems(IEnumerable selectedItems)
            {
                return base.SetSelectedItemsImpl(selectedItems);
            }

            public void UnselectAll()
            {
                this.UnselectAllImpl();
            }

            private void UpdateAnchorAndActionItem(ItemsControl.ItemInfo info)
            {
                ListBoxItem container = info.Container as ListBoxItem;
                if (info.Item == DependencyProperty.UnsetValue)
                {
                    this.AnchorItemInternal = null;
                    this.LastActionItem = null;
                }
                else
                {
                    this.AnchorItemInternal = info;
                    this.LastActionItem = container;
                }
                KeyboardNavigation.SetTabOnceActiveElement(this, container);
            }

            private void ValidateSelectionMode(SelectionMode mode)
            {
                base.CanSelectMultiple = mode != SelectionMode.Single;
            }

            // Properties
            public SelectionMode SelectionMode
            {
                get
                {
                    return (SelectionMode)base.GetValue(SelectionModeProperty);
                }
                set
                {
                    base.SetValue(SelectionModeProperty, value);
                }
            }

            [Bindable(true), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public IList SelectedItems
            {
                get
                {
                    return base.SelectedItemsImpl;
                }
            }

            protected internal override bool HandlesScrolling
            {
                get
                {
                    return true;
                }
            }

            protected object AnchorItem
            {
                get
                {
                    return this.AnchorItemInternal;
                }
                set
                {
                    if ((value == null) || (value == DependencyProperty.UnsetValue))
                    {
                        this.AnchorItemInternal = null;
                        this.LastActionItem = null;
                    }
                    else
                    {
                        ItemsControl.ItemInfo info = base.NewItemInfo(value, null, -1);
                        ListBoxItem container = info.Container as ListBoxItem;
                        if (container == null)
                        {
                            object[] args = new object[] { value };
                            throw new InvalidOperationException(SR.Get("ListBoxInvalidAnchorItem", args));
                        }
                        this.AnchorItemInternal = info;
                        this.LastActionItem = container;
                    }
                }
            }

            internal ItemsControl.ItemInfo AnchorItemInternal
            {
                get
                {
                    return this._anchorItem;
                }
                set
                {
                    this._anchorItem = (value != null) ? value.Clone() : null;
                }
            }

            internal ListBoxItem LastActionItem
            {
                get
                {
                    return (this.GetWeakReferenceTarget(ref this._lastActionItem) as ListBoxItem);
                }
                set
                {
                    this._lastActionItem = new WeakReference(value);
                }
            }

            internal override DependencyObjectType DTypeThemeStyleKey
            {
                get
                {
                    return _dType;
                }
            }



    }
}
