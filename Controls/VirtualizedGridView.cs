﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls.Primitives;
using Boredbone.Utility.Extensions;
using Boredbone.XamlTools;
using Boredbone.XamlTools.Extensions;
using Reactive.Bindings.Extensions;
using System.Windows.Threading;

namespace WpfTools.Controls
{
    public class VirtualizedGridView : Control, IDisposable
    {
        #region ItemsSource

        public IReadOnlyList<object> ItemsSource
        {
            get { return (IReadOnlyList<object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource),
                typeof(IReadOnlyList<object>), typeof(VirtualizedGridView),
                new PropertyMetadata(null, ItemsSourceChanged));

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = (VirtualizedGridView)d;
            thisInstance.OnItemsSourceChanged(e);
        }

        #endregion

        #region ItemTemplate

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate),
                typeof(VirtualizedGridView), new PropertyMetadata(null, ItemTemplateChanged));

        private static void ItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = (VirtualizedGridView)d;
            thisInstance.RefreshSize();
            //thisInstance.contentReference = null;
            //thisInstance.Clear();
            //thisInstance.RenderItems();
        }

        #endregion

        #region ItemSize

        public Size ItemSize
        {
            get { return (Size)GetValue(ItemSizeProperty); }
            set { SetValue(ItemSizeProperty, value); }
        }

        public static readonly DependencyProperty ItemSizeProperty =
            DependencyProperty.Register(nameof(ItemSize), typeof(Size), typeof(VirtualizedGridView),
            new PropertyMetadata(default(Size), new PropertyChangedCallback(OnItemSizeChanged)));

        private static void OnItemSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as VirtualizedGridView;
            //var value = e.NewValue as Size?;
            if (thisInstance.itemSizeChangeCount++ > 0)
            {
                thisInstance?.RefreshSize();
            }
        }

        private int itemSizeChangeCount = 0;

        #endregion

        



        #region RowLength

        public int RowLength
        {
            get { return (int)GetValue(RowLengthProperty); }
            set { SetValue(RowLengthProperty, value); }
        }

        public static readonly DependencyProperty RowLengthProperty =
            DependencyProperty.Register(nameof(RowLength), typeof(int),
                typeof(VirtualizedGridView), new PropertyMetadata(0));

        #endregion

        #region ColumnLength

        public int ColumnLength
        {
            get { return (int)GetValue(ColumnLengthProperty); }
            set { SetValue(ColumnLengthProperty, value); }
        }

        public static readonly DependencyProperty ColumnLengthProperty =
            DependencyProperty.Register(nameof(ColumnLength), typeof(int),
                typeof(VirtualizedGridView), new PropertyMetadata(0));

        #endregion

        #region CurrentIndex

        public int CurrentIndex
        {
            get { return (int)GetValue(CurrentIndexProperty); }
            set { SetValue(CurrentIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrentIndexProperty =
            DependencyProperty.Register(nameof(CurrentIndex), typeof(int), typeof(VirtualizedGridView),
            new PropertyMetadata(0, new PropertyChangedCallback(OnCurrentIndexChanged)));

        private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as VirtualizedGridView;
            var value = e.NewValue as int?;

            if (thisInstance == null || value == null
                || thisInstance.CurrentIndexInner == value)
            {
                return;
            }

            thisInstance.requestedScrollIndex = value.Value;
            thisInstance.scrollRequested = true;
            thisInstance.RenderItems();
        }

        #endregion

        #region PoolLength

        public int PoolLength
        {
            get { return (int)GetValue(PoolLengthProperty); }
            set { SetValue(PoolLengthProperty, value); }
        }
        public static readonly DependencyProperty PoolLengthProperty =
            DependencyProperty.Register(nameof(PoolLength), typeof(int),
                typeof(VirtualizedGridView), new PropertyMetadata(0));

        #endregion

        #region ActiveLength

        public int ActiveLength
        {
            get { return (int)GetValue(ActiveLengthProperty); }
            set { SetValue(ActiveLengthProperty, value); }
        }
        public static readonly DependencyProperty ActiveLengthProperty =
            DependencyProperty.Register(nameof(ActiveLength), typeof(int),
                typeof(VirtualizedGridView), new PropertyMetadata(0));

        #endregion

        public int? ColumnLengthInner
        {
            get { return _fieldColumnLengthInner; }
            set
            {
                if (_fieldColumnLengthInner != value)
                {
                    _fieldColumnLengthInner = value;
                    if (value.HasValue)
                    {
                        this.ColumnLength = value.Value;
                    }
                }
            }
        }
        private int? _fieldColumnLengthInner;

        public int CurrentIndexInner
        {
            get { return _fieldCurrentIndexInner; }
            set
            {
                if (_fieldCurrentIndexInner != value)
                {
                    _fieldCurrentIndexInner = value;
                    this.CurrentIndex = value;
                }
            }
        }
        private int _fieldCurrentIndexInner;



        private bool IsPropertiesInitialised
            => this.itemWidth.HasValue
            && this.itemHeight.HasValue
            && this.ColumnLengthInner.HasValue;

        private double? itemWidth = null;
        private double? itemHeight = null;

        private bool scrollRequested = false;
        private int requestedScrollIndex = 0;

        private Dictionary<int, ItemCacheContainer> activeItems = new Dictionary<int, ItemCacheContainer>();
        private HashSet<ItemCacheContainer> pool = new HashSet<ItemCacheContainer>();
        private CompositeDisposable disposables = new CompositeDisposable();


        private FrameworkElement contentReference = null;
        private IDisposable sizeChangedSubscription = null;

        private ScrollViewer scrollViewer = null;
        private FastCanvas scrollableContent = null;

        static VirtualizedGridView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualizedGridView), new FrameworkPropertyMetadata(typeof(VirtualizedGridView)));
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.scrollViewer = this.GetTemplateChild("PART_scrollViewer") as ScrollViewer;
            this.scrollableContent = this.GetTemplateChild("PART_scrollableContent") as FastCanvas;

            var root = this.GetTemplateChild("PART_virtualizedGridViewRoot") as FrameworkElement;

            if (root != null)
            {
                root.Loaded += this.virtualizedGridViewRoot_Loaded;
            }
            if (this.scrollViewer != null)
            {
                this.scrollViewer.SizeChanged += this.scrollViewer_SizeChanged;
                this.scrollViewer.ScrollChanged += this.scrollViewer_ScrollChanged;
            }

            this.RenderItems();
        }


        private void virtualizedGridViewRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.ColumnLengthInner.HasValue || this.ColumnLengthInner.Value <= 0
                || !this.itemHeight.HasValue || this.itemHeight.Value <= 0)
            {
                this.Clear();
                //this.CheckProperties();
                this.RenderItems();
            }
        }

        /// <summary>
        /// ソース変更
        /// </summary>
        /// <param name="e"></param>
        private void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            var list = e.NewValue as INotifyCollectionChanged;

            if (list != null)
            {
                this.disposables.Clear();

                list.CollectionChangedAsObservable()
                    .ObserveOnUIDispatcher()
                    .Subscribe(this.OnCollectionChanged)
                    .AddTo(this.disposables);
            }

            this.itemHeight = null;

            this.RenderItems();

        }

        /// <summary>
        /// コレクションの内容が変化
        /// </summary>
        /// <param name="e"></param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Move
                && e.Action != NotifyCollectionChangedAction.Replace)
            {
                this.itemHeight = null;
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.activeItems.ForEach(x => x.Value.Item.DataContext = null);
                this.pool.ForEach(x => x.Item.DataContext = null);
                //await Task.Delay(2000);
            }
            //if (e.Action != NotifyCollectionChangedAction.Replace)
            //{
            this.RenderItems();
            //}
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void Clear()
        {
            //this.activeItems.ForEach(x => x.Value.Disable());
            this.activeItems.Clear();
            this.pool.Clear();
            this.ActiveLength = 0;
            this.PoolLength = 0;
            this.itemWidth = null;
            this.itemHeight = null;
            this.ColumnLengthInner = null;
            this.scrollableContent?.ClearChildren();
        }

        /// <summary>
        /// アイテムのサイズを再取得
        /// </summary>
        private void RefreshSize()
        {
            this.contentReference = null;
            this.sizeChangedSubscription?.Dispose();
            this.sizeChangedSubscription = null;
            this.Clear();
            this.RenderItems();
        }

        /// <summary>
        /// 配置計算に必要な値を計算
        /// </summary>
        private bool CheckProperties()
        {

            var firstItem = (this.activeItems.Select(x => x.Value).FirstOrDefault()
                ?? this.pool.FirstOrDefault()
                ?? this.GenerateNewItem())?.Item;

            if (firstItem == null)
            {
                return false;
            }


            //var margin = default(Thickness);// firstItem.Margin;
            if (!this.itemWidth.HasValue)
            {
                this.itemWidth = firstItem.Width;// + margin.Left + margin.Right;

            }

            if (this.itemWidth <= 0 || this.scrollableContent.ActualWidth <= 0)
            {
                return false;
            }

            if (!this.ColumnLengthInner.HasValue)
            {
                var count = (int)(this.scrollableContent.ActualWidth / this.itemWidth.Value);
                if (count < 1)
                {
                    count = 1;
                }
                this.ColumnLengthInner = count;
            }
            if (!this.itemHeight.HasValue)
            {
                this.itemHeight = firstItem.Height;// + margin.Top + margin.Bottom;
                this.scrollableContent.Height
                    = Math.Ceiling((double)this.ItemsSource.Count / this.ColumnLengthInner.Value)
                        * this.itemHeight.Value;

                var count = (int)Math.Ceiling(this.scrollViewer.ActualHeight / this.itemHeight.Value);
                if (count < 1)
                {
                    count = 1;
                }
                this.RowLength = count + 1;
            }

            return true;

        }

        /// <summary>
        /// 画面内に配置されるアイテムを描画
        /// </summary>
        private void RenderItems()
        {
            if (this.scrollableContent == null)
            {
                return;
            }

            if (this.ItemsSource == null
                || this.ItemsSource.Count <= 0
                || this.ItemTemplate == null)
            {
                this.activeItems.ToArray().ForEach(x => this.DisableItem(x.Key));
                //this.scrollableContent.Children.Clear();
                return;
            }

            if (!this.IsPropertiesInitialised)
            {
                if (!this.CheckProperties())
                {
                    return;
                }
            }

            if (this.scrollRequested)
            {
                var row = this.requestedScrollIndex / this.ColumnLengthInner.Value;

                this.scrollViewer.ScrollToVerticalOffset(row * this.itemHeight.Value);

                this.scrollRequested = false;
            }

            var top = this.scrollViewer.VerticalOffset;
            var bottom = top + this.scrollViewer.ActualHeight;

            var topRow = (int)(top / this.itemHeight.Value);
            var bottomRow = (int)Math.Ceiling(bottom / this.itemHeight.Value);

            var firstIndex = topRow * this.ColumnLengthInner.Value;
            var lastIndex = (bottomRow + 1) * this.ColumnLengthInner.Value - 1;

            if (firstIndex < 0)
            {
                firstIndex = 0;
            }

            if (lastIndex > this.ItemsSource.Count - 1)
            {
                lastIndex = this.ItemsSource.Count - 1;
            }

            this.CurrentIndexInner = firstIndex;

            this.activeItems
                .Where(x => x.Key < firstIndex || x.Key > lastIndex)
                .ToArray()
                .ForEach(x => this.DisableItem(x.Key));

            for (var i = firstIndex; i <= lastIndex; i++)
            {
                this.EnableItem(i, this.ItemsSource[i]);
            }

            /*
            var rect = new Rect(
                this.scrollViewer.HorizontalOffset,
                this.scrollViewer.VerticalOffset,
                this.scrollViewer.ViewportWidth,
                this.scrollViewer.ViewportHeight);

            this.scrollableContent.SetViewport(rect);*/

            //foreach (var item in this.ItemsSource)
            //{
            //    var elm = this.ItemTemplate.LoadContent() as FrameworkElement;
            //
            //    if (elm != null)
            //    {
            //        elm.DataContext = item;
            //        this.scrollableContent.Children.Add(elm);
            //    }
            //}
        }

        /// <summary>
        /// 画面サイズ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsPropertiesInitialised)
            {
                var oldOffset = this.scrollViewer.VerticalOffset;
                var oldIndex = this.CurrentIndexInner + this.ColumnLengthInner.Value;
                // (int)(oldOffset / this.itemHeight.Value) * this.ColumnLengthInner.Value;
                var oldItemOffset = oldOffset % this.itemHeight.Value;

                this.ColumnLengthInner = null;
                this.itemHeight = null;

                //配置に必要な数値を再計算
                this.CheckProperties();
                var column = this.ColumnLengthInner;
                if (column == null)
                {
                    column = 1;
                }

                var row = oldIndex / column.Value - 1;
                var offset = row * this.itemHeight.Value + oldItemOffset;

                this.scrollViewer.ScrollToVerticalOffset(offset);

            }
            else
            {
                this.ColumnLengthInner = null;
                this.itemHeight = null;
            }

            this.RenderItems();

            if (this.itemHeight.HasValue
                && this.scrollableContent.ActualHeight < e.NewSize.Height + this.itemHeight.Value)
            {
                this.scrollableContent.InvalidateArrange();
                //this.Padding = (this.Padding.Bottom == 0) ? new Thickness(1) : new Thickness(0);
                //if (this.activeItems.Count > 0)
                //{
                //    this.activeItems.First().Value.Item.Visibility =
                //        (this.activeItems.First().Value.Item.Visibility==Visibility.Visible)?
                //        Visibility.Collapsed:Visibility.Visible;
                //}
            }
        }

        /// <summary>
        /// スクロール
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.RenderItems();
        }

        /// <summary>
        /// 新しいアイテムを生成してプールに追加
        /// </summary>
        /// <returns></returns>
        private ItemCacheContainer GenerateNewItem()
        {
            //var item = this.ItemTemplate.LoadContent() as FrameworkElement;
            //
            //if (item == null)
            //{
            //    return null;
            //    //throw new ArgumentException("Invalid ItemTemplate");
            //}

            if (this.contentReference == null)
            {
                this.contentReference = this.ItemTemplate.LoadContent() as FrameworkElement;

                if (this.contentReference == null)
                {
                    return null;
                    //throw new ArgumentException("Invalid ItemTemplate");
                }

            }

            var margin = this.contentReference.Margin;

            var item = new ContentControl()
            {
                Width = this.contentReference.Width + margin.Left + margin.Right,
                Height = this.contentReference.Height + margin.Top + margin.Bottom,
                //Margin = this.contentReference.Margin,
                ContentTemplate = this.ItemTemplate,
                IsTabStop = false,
            };

            //if (this.sizeChangedSubscription == null)
            //{
            //    this.sizeChangedSubscription = contentReference.SizeChangedAsObservable()
            //        .Skip(1)
            //        .Subscribe(x =>
            //        {
            //            this.RefreshSize();
            //        })
            //        .AddTo(this.disposables);
            //
            //}

            //if (this.scrollableContent.Children.Count <= 0)
            //{
            //    this.scrollableContent.Children.Add(new UIElement()
            //    {
            //        Visibility = Visibility.Collapsed,
            //    });
            //}

            var container = new ItemCacheContainer(item);

            container.Disable();

            this.pool.Add(container);
            this.scrollableContent.Children.Add(item);

            this.PoolLength++;

            return container;
        }

        /// <summary>
        /// 画面内のアイテムをプールに移動
        /// </summary>
        /// <param name="key"></param>
        private void DisableItem(int key)
        {
            ItemCacheContainer item;

            if (this.activeItems.TryGetValue(key, out item))
            {
                //var item = this.activeItems[key];
                item.Disable();
                this.activeItems.Remove(key);
                this.pool.Add(item);

                this.ActiveLength--;
                this.PoolLength++;
            }
        }


        /// <summary>
        /// アイテムの描画許可
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dataContext"></param>
        private void EnableItem(int index, object dataContext)
        {
            ItemCacheContainer item;


            //if (this.activeItems.ContainsKey(index))
            //{
            //    item = this.activeItems[index];
            //}
            //else
            if (!this.activeItems.TryGetValue(index, out item))
            {
                item = this.pool.FirstOrDefault();
                if (item == null)
                {
                    item = this.GenerateNewItem();
                }

                this.pool.Remove(item);
                this.activeItems.Add(index, item);

                this.ActiveLength++;
                this.PoolLength--;
            }

            var top = (index / this.ColumnLengthInner.Value) * this.itemHeight.Value;
            var left = (index % this.ColumnLengthInner.Value) * this.itemWidth.Value;

            //item.Top = top;
            //item.Left = left;

            item.Enable(dataContext, left, top);
            
        }

        public void Dispose()
        {
            this.disposables.Dispose();
        }



        /// <summary>
        /// アイテムを保持するクラス
        /// </summary>
        private class ItemCacheContainer
        {
            private double Top { get; set; }
            private double Left { get; set; }
            public ContentControl Item { get; }

            public bool IsEnabled
                => (this.Item == null) ? false : (this.Item.Visibility == Visibility.Visible);

            //private bool isActive = false;

            public ItemCacheContainer(ContentControl item)
            {
                this.Item = item;
            }

            public void Disable()
            {
                this.Item.Visibility = Visibility.Collapsed;
                this.Item.DataContext = null;
                this.Item.Content = null;
                //this.isActive = false;
            }

            public void Enable(object dataContext, double left, double top)
            {
                //this.isActive = true;

                /*
                if (this.Left != left)
                {
                    this.Left = left;
                    Canvas.SetLeft(this.Item, this.Left);
                }
                if (this.Top != top)
                {
                    this.Top = top;
                    Canvas.SetTop(this.Item, this.Top);
                }*/

                if (this.Left != left || this.Top != top)
                {
                    this.Left = left;
                    this.Top = top;
                    FastCanvas.SetLocation(this.Item, new Point(this.Left, this.Top));
                }

                this.Item.DataContext = dataContext;
                this.Item.Content = dataContext;
                this.Item.Visibility = Visibility.Visible;
            }
        }

    }
}