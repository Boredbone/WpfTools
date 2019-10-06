using System;
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
using Boredbone.Utility.Extensions;
using Reactive.Bindings.Extensions;
using Boredbone.Utility.Tools;
using System.Diagnostics;

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

        #region IsRefreshEnabled

        public bool IsRefreshEnabled
        {
            get { return (bool)GetValue(IsRefreshEnabledProperty); }
            set { SetValue(IsRefreshEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsRefreshEnabledProperty =
            DependencyProperty.Register(nameof(IsRefreshEnabled), typeof(bool),
                typeof(VirtualizedGridView), new PropertyMetadata(true));

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
            thisInstance.ScrollRequested = 1;
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


        #region IsRenderingEnabled

        public bool IsRenderingEnabled
        {
            get { return (bool)GetValue(IsRenderingEnabledProperty); }
            set { SetValue(IsRenderingEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsRenderingEnabledProperty =
            DependencyProperty.Register(nameof(IsRenderingEnabled), typeof(bool), typeof(VirtualizedGridView),
            new PropertyMetadata(true, new PropertyChangedCallback(OnIsRenderingEnabledChanged)));

        private static void OnIsRenderingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as VirtualizedGridView;
            var value = e.NewValue as bool?;

            if (thisInstance != null && value.HasValue && value.Value)
            {
                thisInstance.ScrollRequested = 1;
                thisInstance.RenderItems(true);
            }
        }

        #endregion


        #region ScrollToIndexAction

        public Action<int> ScrollToIndexAction
        {
            get { return (Action<int>)GetValue(ScrollToIndexActionProperty); }
            set { SetValue(ScrollToIndexActionProperty, value); }
        }

        public static readonly DependencyProperty ScrollToIndexActionProperty =
            DependencyProperty.Register(nameof(ScrollToIndexAction), typeof(Action<int>),
                typeof(VirtualizedGridView), new PropertyMetadata(null));

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

        public int ScrollRequested
        {
            get { return _fieldScrollRequested; }
            set
            {
                if (_fieldScrollRequested != value)
                {
                    _fieldScrollRequested = value;
                }
            }
        }
        private int _fieldScrollRequested = 0;


        private int requestedScrollIndex = 0;

        private Dictionary<int, ItemCacheContainer> activeItems = new Dictionary<int, ItemCacheContainer>();
        private HashSet<ItemCacheContainer> pool = new HashSet<ItemCacheContainer>();
        private CompositeDisposable disposables = new CompositeDisposable();


        private FrameworkElement contentReference = null;
        private IDisposable sizeChangedSubscription = null;

        private ScrollViewer scrollViewer = null;
        private FastCanvas scrollableContent = null;

        private double desiredVerticalOffset = 0.0;

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

            this.ScrollToIndexAction = this.ScrollToIndex;

            this.RenderItems();
        }


        private void virtualizedGridViewRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.ColumnLengthInner.HasValue || this.ColumnLengthInner.Value <= 0
                || !this.itemHeight.HasValue || this.itemHeight.Value <= 0)
            {
                this.Clear();
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
            }
            this.RenderItems();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void Clear()
        {
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


            if (!this.itemWidth.HasValue)
            {
                this.itemWidth = firstItem.Width;

            }

            if (this.itemWidth <= 0 || this.scrollableContent.ActualWidth <= 0)
            {
                return false;
            }

            if (!this.ColumnLengthInner.HasValue)
            {
                var frameWidth = this.scrollableContent.ActualWidth;

                if (this.scrollViewer != null && this.scrollViewer.ScrollableHeight <= 0)
                {
                    frameWidth -= 17;
                }

                var count = (int)(frameWidth / this.itemWidth.Value);
                if (count < 1)
                {
                    count = 1;
                }
                this.ColumnLengthInner = count;
            }
            if (!this.itemHeight.HasValue)
            {
                this.itemHeight = firstItem.Height;
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
        private void RenderItems(bool force = false)
        {
            if (this.scrollableContent == null || (!force && !this.IsRenderingEnabled))
            {
                return;
            }

            if (this.ItemsSource == null
                || this.ItemsSource.Count <= 0
                || this.ItemTemplate == null)
            {
                var array = this.activeItems.ToArray();
                array.ForEach(x => this.DisableItem(x.Key));

                if (!this.IsPropertiesInitialised)
                {
                    this.scrollableContent.Height = 0;
                }
                return;
            }

            if (!this.IsPropertiesInitialised)
            {
                if (!this.CheckProperties())
                {
                    return;
                }
            }

            var top = this.desiredVerticalOffset;

            if (this.desiredVerticalOffset > this.scrollViewer.ScrollableHeight)
            {
                top = this.scrollViewer.ScrollableHeight;
            }


            if (this.ScrollRequested > 0)
            {
                var row = this.requestedScrollIndex / this.ColumnLengthInner.Value;

                var currentRow = (int)(top / this.itemHeight.Value);

                if (row != currentRow || row == 0)
                {
                    var offset = row * this.itemHeight.Value;

                    this.ScrollToVerticalOffset(offset);

                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    if (offset > this.scrollViewer.ScrollableHeight)
                    {
                        offset = this.scrollViewer.ScrollableHeight;
                    }
                    top = offset;
                }

                this.ScrollRequested--;
            }

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
                var oldOffset = this.desiredVerticalOffset;
                var oldIndex = this.CurrentIndexInner + this.ColumnLengthInner.Value;
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

                this.ScrollToVerticalOffset(offset);
            }
            else
            {
                this.ColumnLengthInner = null;
                this.itemHeight = null;
            }

            this.RenderItems();

            if (this.itemHeight.HasValue)
            {
                this.scrollableContent.InvalidateArrange();
            }
        }

        /// <summary>
        /// スクロール
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.ScrollRequested <= 0)
            {
                this.desiredVerticalOffset = this.scrollViewer.VerticalOffset;
            }
            if (this.ScrollRequested <= 1)
            {
                this.RenderItems();
            }
        }

        private void ScrollToVerticalOffset(double value)
        {
            this.desiredVerticalOffset = value;
            this.scrollViewer.ScrollToVerticalOffset(value);
        }

        /// <summary>
        /// 指定インデックスの位置までスクロール
        /// </summary>
        /// <param name="index"></param>
        public void ScrollToIndex(int index)
        {
            if (this.CurrentIndex != index && this.CurrentIndexInner != index)
            {
                this.CurrentIndex = index;
                return;
            }

            this.CurrentIndex = index;

            this.requestedScrollIndex = index;
            this.ScrollRequested = 1;
            this.RenderItems(true);
        }

        /// <summary>
        /// 新しいアイテムを生成してプールに追加
        /// </summary>
        /// <returns></returns>
        private ItemCacheContainer GenerateNewItem()
        {
            if (this.contentReference == null)
            {
                this.contentReference = this.ItemTemplate.LoadContent() as FrameworkElement;

                if (this.contentReference == null)
                {
                    return null;
                }

            }

            var margin = this.contentReference.Margin;

            var item = new ContentControl()
            {
                Width = this.contentReference.Width + margin.Left + margin.Right,
                Height = this.contentReference.Height + margin.Top + margin.Bottom,
                ContentTemplate = this.ItemTemplate,
                IsTabStop = false,
            };


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

            item.Enable(index, dataContext, left, top);

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

            private Indexed<object> Data { get; }


            public bool IsEnabled
                => (this.Item == null) ? false : (this.Item.Visibility == Visibility.Visible);

            public ItemCacheContainer(ContentControl item)
            {
                this.Item = item;
                this.Data = new Indexed<object>();
            }

            public void Disable()
            {
                this.Item.Visibility = Visibility.Collapsed;
                this.Item.DataContext = null;
                this.Item.Content = null;
            }

            public void Enable(int index, object dataContext, double left, double top)
            {

                if (this.Left != left || this.Top != top)
                {
                    this.Left = left;
                    this.Top = top;
                    FastCanvas.SetLocation(this.Item, new Point(this.Left, this.Top));
                }

                this.Data.Index = index;
                this.Data.Value = dataContext;

                this.Item.Content = this.Data;
                this.Item.Visibility = Visibility.Visible;
            }
        }

    }
}
