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
using System.Globalization;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using WpfTools.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Linq;

namespace WpfTools.Controls
{
    public class AppendableText : Control, IDisposable
    {
        #region Controller

        public AppendableTextController Controller
        {
            get { return (AppendableTextController)GetValue(ControllerProperty); }
            set { SetValue(ControllerProperty, value); }
        }

        public static readonly DependencyProperty ControllerProperty =
            DependencyProperty.Register(nameof(Controller), typeof(AppendableTextController), typeof(AppendableText),
            new PropertyMetadata(null, new PropertyChangedCallback(OnControllerChanged)));

        private static void OnControllerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as AppendableText;
            var value = e.NewValue as AppendableTextController;

            if (thisInstance != null && value != null)
            {
                var oldValue = e.OldValue as AppendableTextController;
                if (oldValue != null)
                {
                    oldValue.View = null;
                }
                value.View = thisInstance;
            }
        }

        #endregion

        #region Count

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register(nameof(Count), typeof(int),
                typeof(AppendableText), new PropertyMetadata(0));

        private void SetCount(int value)
        {
            this.Count = value;
            this.bar.Maximum = value;
        }

        #endregion

        #region CurrentIndex

        public int CurrentIndex
        {
            get { return (int)GetValue(CurrentIndexProperty); }
            set { SetValue(CurrentIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrentIndexProperty =
            DependencyProperty.Register(nameof(CurrentIndex), typeof(int),
                typeof(AppendableText), new PropertyMetadata(0));

        private void SetCurrentIndex(int value)
        {
            this.CurrentIndex = value;
            this.bar.DisplayPosition = value;
        }

        #endregion

        #region LastLineBrush

        public Brush LastLineBrush
        {
            get { return (Brush)GetValue(LastLineBrushProperty); }
            set { SetValue(LastLineBrushProperty, value); }
        }

        public static readonly DependencyProperty LastLineBrushProperty =
            DependencyProperty.Register(nameof(LastLineBrush), typeof(Brush), typeof(AppendableText),
            new PropertyMetadata(Brushes.LightGray, new PropertyChangedCallback(OnLastLineBrushChanged)));

        private static void OnLastLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as AppendableText;
            var value = e.NewValue as Brush;

            if (thisInstance != null)
            {
                thisInstance.lastLineMarker.Background = value;
            }
        }

        #endregion


        #region MaxBlockCount

        public int MaxBlockCount
        {
            get { return (int)GetValue(MaxBlockCountProperty); }
            set { SetValue(MaxBlockCountProperty, value); }
        }

        public static readonly DependencyProperty MaxBlockCountProperty =
            DependencyProperty.Register(nameof(MaxBlockCount), typeof(int), typeof(AppendableText),
            new PropertyMetadata(0, new PropertyChangedCallback(OnMaxBlockCountChanged)));

        private static void OnMaxBlockCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as AppendableText;
            if (thisInstance != null)
            {
                thisInstance.Texts.MaxBlockCount = (int)e.NewValue;
            }
        }

        #endregion

        #region BlockSize

        public int BlockSize
        {
            get { return (int)GetValue(BlockSizeProperty); }
            set { SetValue(BlockSizeProperty, value); }
        }

        public static readonly DependencyProperty BlockSizeProperty =
            DependencyProperty.Register(nameof(BlockSize), typeof(int), typeof(AppendableText),
            new PropertyMetadata(0, new PropertyChangedCallback(OnBlockSizeChanged)));

        private static void OnBlockSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as AppendableText;
            if (thisInstance != null)
            {
                thisInstance.Texts.BlockSize = (int)e.NewValue;
            }
        }

        #endregion

        #region MarginLineCount

        public int MarginLineCount
        {
            get { return (int)GetValue(MarginLineCountProperty); }
            set { SetValue(MarginLineCountProperty, value); }
        }

        public static readonly DependencyProperty MarginLineCountProperty =
            DependencyProperty.Register(nameof(MarginLineCount), typeof(int), typeof(AppendableText),
            new PropertyMetadata(3, new PropertyChangedCallback(OnMarginLineCountChanged)));

        private static void OnMarginLineCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as AppendableText;
            thisInstance?.RefreshLineSize();
        }

        #endregion





        private PositionBar bar = null;
        private FastCanvas canvas = null;
        private Border rootGrid = null;
        private MenuItem copyMenu = null;
        private RepeatButton upButton = null;
        private RepeatButton downButton = null;

        private Canvas lastLineMarker;

        private readonly CompositeDisposable disposables;

        private ItemsManager<FormattedText> Texts { get; }

        private List<TextCanvas> activeContainers = new List<TextCanvas>();
        private List<TextCanvas> pooledContainers = new List<TextCanvas>();

        private BlockItem<FormattedText> TopItem
        {
            get { return _fieldTopItem; }
            set
            {
                if (_fieldTopItem != value)
                {
                    _fieldTopItem = value;
                    this.UpdateCurrentIndex();
                }
            }
        }
        private BlockItem<FormattedText> _fieldTopItem = null;
        private BlockItem<FormattedText> bottomItem = null;
        private double topOffset = 0.0;
        private double bottomOffset = 0.0;

        private BlockItem<FormattedText> LastItem
        {
            get { return _fieldLastItem; }
            set
            {
                if (_fieldLastItem != value)
                {
                    _fieldLastItem = value;
                }
            }
        }
        private BlockItem<FormattedText> _fieldLastItem = null;

        private object lastItemLock = new object();

        public string LastText => this.LastItem?.Value?.Text;

        private List<TextBrush> textBrushes = new List<TextBrush>();

        private Typeface textFontFamily = new Typeface("Consolas");
        private double textFontSize = 10.0;

        //private int marginLineCount = 3;

        private double remainingLineHeight = 0.0;
        private double unitLineHeight = 0.0;
        private double scrollLength = 0.0;
        private double marginLineHeight = 0.0;
        //private double marginLineThreshold = 0.0;


        private bool refreshFlag = false;

        private List<BlockItem<FormattedText>> selectedItems = new List<BlockItem<FormattedText>>();

        private bool mouseCaptureing = false;
        private Point prevMousePosition = default(Point);


        private double pixelsPerDip = -1.0;
        private double PixelsPerDip
        {
            get
            {
                if (this.pixelsPerDip > 0)
                {
                    return this.pixelsPerDip;
                }
                var source = PresentationSource.FromVisual(this);
                if (source != null && source.CompositionTarget != null)
                {
                    this.pixelsPerDip = source.CompositionTarget.TransformToDevice.M11;
                    return this.pixelsPerDip;
                }
                return 1.0;
            }
        }

        private readonly Subject<Unit> updateRequestSubject;

        private object textGeneratorGate = new object();

        public AppendableText()
        {

            this.disposables = new CompositeDisposable();

            this.lastLineMarker = new Canvas();

            this.updateRequestSubject = new Subject<Unit>().AddTo(this.disposables);

            this.Texts = new ItemsManager<FormattedText>().AddTo(this.disposables);

            this.Texts.ItemsAdded
                .Select(_ => Unit.Default)
                .Merge(this.updateRequestSubject)
                .ObserveOnUIDispatcher()
                .Subscribe(_ =>
                {
                    if (this.TopItem == null)
                    {
                        this.TopItem = this.Texts.CurrentBlock.Last;
                    }
                    this.RefreshTexts(false);

                    if (this.bar != null)
                    {
                        this.SetCount(this.Texts.Count);
                        this.UpdateCurrentIndex();
                    }
                })
                .AddTo(this.disposables);

            //this.Texts.CountChanged.ObserveOnUIDispatcher().Subscribe(x =>
            //{
            //    if (this.bar != null)
            //    {
            //        this.SetCount(x);
            //        this.UpdateCurrentIndex();
            //    }
            //})
            //.AddTo(this.disposables);

            this.Texts.ItemsRemoved.ObserveOnUIDispatcher().Subscribe(x =>
            {
                if (this.TopItem != null && this.TopItem.Parent.Id == x.RemovedBlockId)
                {
                    this.MoveToHome();
                }
            })
            .AddTo(this.disposables);
        }


        static AppendableText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppendableText),
                new FrameworkPropertyMetadata(typeof(AppendableText)));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            this.bar = this.GetTemplateChild("PART_Bar") as PositionBar;
            this.canvas = this.GetTemplateChild("PART_Canvas") as FastCanvas;
            this.rootGrid = this.GetTemplateChild("PART_RootBorder") as Border;
            this.copyMenu = this.GetTemplateChild("PART_CopyMenu") as MenuItem;
            this.upButton = this.GetTemplateChild("PART_UpButton") as RepeatButton;
            this.downButton = this.GetTemplateChild("PART_DownButton") as RepeatButton;


            this.rootGrid.MouseWheel += canvas_MouseWheel;
            this.rootGrid.PreviewKeyDown += rootGrid_PreviewKeyDown;
            this.rootGrid.KeyDown += Grid_KeyDown;

            this.canvas.SizeChanged += canvas_SizeChanged;
            this.canvas.MouseLeftButtonDown += canvas_MouseLeftButtonDown;
            this.canvas.MouseMove += canvas_MouseMove;
            this.canvas.MouseLeftButtonUp += canvas_MouseLeftButtonUp;

            this.copyMenu.Click += MenuItem_Click;

            this.upButton.Click += Button_Click_1;
            this.downButton.Click += Button_Click_2;

            this.bar.SelectedPositionChanged += bar_SelectedPositionChanged;

            this.Loaded += this.UserControl_Loaded;

            this.lastLineMarker.Background = this.LastLineBrush;
            this.canvas.Children.Add(this.lastLineMarker);

            this.SetFontFamily(this.FontFamily);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.textFontSize = this.FontSize;
            this.RefreshLineSize();
            lock (this.lastItemLock)
            {
                this.LastItem = this.Texts.Add(this.GenerateFormattedText(""));
            }
        }

        public void SetFontFamily(FontFamily font)
        {
            this.textFontFamily = new Typeface(font.Source);
        }
        public void SetFontSize(double fontSize)
        {
            this.FontSize = fontSize;
            this.textFontSize = this.FontSize;
        }


        private FormattedText GenerateFormattedText(string text)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                this.textFontFamily,
                this.textFontSize,
                Brushes.Black
#if !NET45
                ,this.PixelsPerDip
#endif
                )
            {
                Trimming = TextTrimming.None,
                MaxTextWidth = this.canvas.ActualWidth,
            };
        }
        private FormattedText GenerateText(string text, IEnumerable<TextBrush> brush)
        {
            lock (this.textGeneratorGate)
            {
                this.textBrushes.Clear();
                return this.GenerateTextMain(text, brush);
            }
        }

        private FormattedText GenerateText(FormattedText target, string text, IEnumerable<TextBrush> brush)
        {
            lock (this.textGeneratorGate)
            {
                var currentLength = target.Text?.Length ?? 0;

                if (currentLength == 0)
                {
                    this.textBrushes.Clear();
                }

                var brushs = (brush == null) ? null
                    : brush.Where(x => x != null).Select(x => x.AddOffset(currentLength));

                return this.GenerateTextMain(target.Text + text, brushs);
            }
        }

        private FormattedText GenerateTextMain(string text, IEnumerable<TextBrush> brush)
        {
            var ft = this.GenerateFormattedText(text);

            if (brush != null)
            {
                this.textBrushes.AddRange(brush.Where(x => x != null));
            }

            foreach (var b in this.textBrushes)
            {
                var length = b.Length;
                if (b.StartIndex + b.Length > text.Length)
                {
                    length = text.Length - b.StartIndex;
                }
                if (length <= 0)
                {
                    continue;
                }
                if (b.Brush != null)
                {
                    ft.SetForegroundBrush(b.Brush, b.StartIndex, length);
                }
                if (b.IsBold)
                {
                    ft.SetFontWeight(FontWeights.Bold, b.StartIndex, length);
                }

            }

            return ft;
        }



        private void RefreshLineSize()
        {
            if (!this.IsLoaded)
            {
                return;
            }
            var line = this.GenerateFormattedText("\n\n\n");
            this.scrollLength = line.Height / 60.0;
            this.remainingLineHeight = line.Height / 3.0;
            this.unitLineHeight = line.Height / 3.0;

            if (this.MarginLineCount <= 0)
            {
                this.marginLineHeight = 0.0;
            }
            else
            {
                var marginLines = this.GenerateFormattedText
                    (new string(Enumerable.Range(0, this.MarginLineCount).Select(_ => '\n').ToArray()));

                this.marginLineHeight = marginLines.Height;
            }
            //var marginLinesUpper = this.GenerateFormattedText
            //    (new string(Enumerable.Range(0, this.marginLineCount + 1).Select(_ => '\n').ToArray()));

            //this.marginLineThreshold = (this.marginLineHeight + marginLinesUpper.Height) / 2.0;

        }

        public void AppendLine(FormattedText text)
        {
            this.Texts.Add(text);
            lock (this.lastItemLock)
            {
                this.LastItem = null;
            }
        }

        public void ReplaceLine(FormattedText text)
        {
            lock (this.lastItemLock)
            {
                if (this.LastItem != null)
                {
                    this.LastItem.Value = text;
                    this.updateRequestSubject.OnNext(Unit.Default);
                }
                else
                {
                    this.LastItem = this.Texts.Add(text);
                }
            }
        }

        public void Clear()
        {
            this.TopItem = null;
            this.bottomItem = null;
            this.LastItem = null;
            this.textBrushes.Clear();
            this.selectedItems.Clear();
            this.topOffset = 0;
            this.bottomOffset = 0;
            this.Texts.Clear();
            lock (this.lastItemLock)
            {
                this.LastItem = this.Texts.Add(this.GenerateFormattedText(""));
            }
            this.RefreshTexts(true);
        }

        public void Write(string text, bool atNewLine, params TextBrush[] brush)
        {
            var lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

            List<FormattedText> items = null;

            var updated = false;
            var refresh = false;

            var brushes = (brush == null) ? new TextBrush[0] : brush;

            lock (this.lastItemLock)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (i == 0 && this.LastItem != null
                        && (!atNewLine || this.Texts.Count <= 0
                        || (this.Texts.Count == 1 && this.Texts.CurrentBlock.First.Value.Text.Length <= 0)))
                    {
                        //if (line.Length == 0)
                        //{
                        //    this.LastItem = null;
                        //}
                        //else
                        //{
                        if (this.LastItem.Value != null)
                        {
                            this.LastItem.Value = this.GenerateText(this.LastItem.Value, line, brushes);
                            updated = true;
                        }
                        else
                        {
                            this.LastItem.Value = this.GenerateText(line, brushes);
                            updated = true;
                        }
                        if (lines.Length == 1)
                        {
                            refresh = true;
                            //this.RefreshTexts();
                        }
                        //}
                    }
                    else
                    {
                        if (items == null)
                        {
                            items = new List<FormattedText>();
                        }
                        var item = this.GenerateText(line, brushes);
                        items.Add(item);
                        //this.LastItem = (line.Length == 0) ? null : this.Texts.Add(item);
                    }

                    brushes = brushes
                        .Select(x => x.CutStart(line.Length + 1))
                        .Where(x => x.Length >= 0)
                        .ToArray();
                }

                if (items != null && items.Count > 0)
                {
                    var last = this.Texts.AddRange(items);
                    this.LastItem = last; //(last.Value.Text.Length == 0) ? null : last;
                    return;
                }
            }

            if (refresh)
            {
                this.updateRequestSubject.OnNext(Unit.Default);
                //this.RefreshTexts();
            }
            else if (updated)
            {
                this.updateRequestSubject.OnNext(Unit.Default);
            }
        }


        private double GetItemHeight(FormattedText item)
        {
            if (item == null)
            {
                return this.unitLineHeight;
            }
            var height = item.Height;
            if (height < 1)
            {
                return this.unitLineHeight;
            }
            return height;
        }


        private void RefreshTexts(bool force)
        {
            if (force)
            {
                this.refreshFlag = true;
            }
            if (this.bottomItem == null)
            {
                var offset = this.topOffset;
                var item = this.TopItem;
                var width = this.canvas.ActualWidth;
                var threshold = this.canvas.ActualHeight - this.marginLineHeight;
                var scroll = false;

                while (true)
                {
                    if (item == null)
                    {
                        break;
                    }
                    item.Value.MaxTextWidth = width;
                    offset += this.GetItemHeight(item.Value);
                    if (offset >= threshold)
                    {
                        scroll = true;
                        break;
                    }
                    item = item.NextItem;
                }

                if (scroll)
                {
                    this.ScrollToBottomMain();
                }
                else
                {
                    this.RefreshTexts(this.TopItem, this.topOffset);
                }

            }
            else if (this.bottomItem != null && this.bottomItem.NextItem != null && !force)
            {
                return;
            }
            else
            {
                this.RefreshTexts(this.TopItem, this.topOffset);
            }

        }

        internal void ScrollToBottom()
        {
            if (this.bottomItem != null)
            {
                this.ScrollToBottomMain();
            }
        }
        private void ScrollToBottomMain()
        {
            this.RefreshTexts(null, this.canvas.ActualHeight - this.marginLineHeight);
        }

        private void UpdateCurrentIndex()
        {
            var value = this.Texts.GetReversedIndexOf(this.TopItem);
            this.SetCurrentIndex(value);
        }

        private void RefreshTexts(BlockItem<FormattedText> referenceItem, double startOffset)
        {
            var offset = startOffset;

            var item = referenceItem;

            var displayed = new List<TextCanvas>(this.activeContainers);

            this.activeContainers.Clear();

            var continuingIndices = new HashSet<int>();

            var width = this.canvas.ActualWidth;
            var height = this.canvas.ActualHeight;


            this.bottomItem = null;

            if (referenceItem == null)
            {
                item = this.Texts.CurrentBlock.Last;
                if (item == null)
                {
                    return;
                }
                item.Value.MaxTextWidth = width;
                offset -= this.GetItemHeight(item.Value);


                if (startOffset < this.remainingLineHeight * 0.5)
                {
                    offset = -this.GetItemHeight(item.Value) + this.remainingLineHeight;
                    //offset = 0.0;
                }
            }
            while (offset > 0)
            {
                if (item == null)
                {
                    break;
                }
                if (item.PrevItem == null)
                {
                    offset = 0.0;
                    break;
                }
                item = item.PrevItem;
                item.Value.MaxTextWidth = width;
                offset -= this.GetItemHeight(item.Value);
            }

            BlockItem<FormattedText> lastLine = null;
            double lastLineOffset = 0.0;

            while (true)
            {
                if (item == null)
                {
                    break;
                }

                var text = item.Value;
                var widthChanged = text.MaxTextWidth != width;
                var itemHeight = this.GetItemHeight(text);

                text.MaxTextWidth = width;

                if (offset <= height && offset + itemHeight >= 0.0)
                {
                    TextCanvas container;
                    var index = this.refreshFlag ? -1 : displayed
                        .FindIndex(x => object.ReferenceEquals(x.Source.Value, text));

                    if (index >= 0)
                    {
                        container = displayed[index];
                        continuingIndices.Add(index);

                        if (widthChanged)
                        {
                            container.Update();
                        }
                    }
                    else
                    {
                        if (this.pooledContainers.Count > 0)
                        {
                            container = this.pooledContainers[0];
                            this.pooledContainers.RemoveAt(0);
                            container.Source = item;
                        }
                        else
                        {
                            container = new TextCanvas() { Source = item, Focusable = false };
                        }
                        this.canvas.Children.Add(container);
                    }

                    this.activeContainers.Add(container);

                    FastCanvas.SetLocation(container, new Point(0.0, Math.Round(offset)));

                    if (offset <= 0.0)
                    {
                        this.TopItem = item;
                        this.topOffset = offset;
                    }
                    if (offset + itemHeight > height)
                    {
                        this.bottomItem = item;
                        this.bottomOffset = offset;
                    }
                }

                lastLine = item;
                lastLineOffset = offset;

                offset += itemHeight;

                if (offset > height)
                {
                    break;
                }
                item = item.NextItem;
            }

            if (lastLine != null && lastLine.NextItem == null)
            {
                FastCanvas.SetLocation(this.lastLineMarker, new Point(0.0, Math.Floor(lastLineOffset)));
                this.lastLineMarker.Width = this.canvas.ActualWidth;
                this.lastLineMarker.Height = Math.Ceiling(this.GetItemHeight(lastLine.Value));
                this.lastLineMarker.Visibility = Visibility.Visible;
            }
            else
            {
                this.lastLineMarker.Visibility = Visibility.Collapsed;
            }

            if (this.bottomItem == null)
            {
                this.bottomOffset = offset;
            }

            for (int i = 0; i < displayed.Count; i++)
            {
                if (!continuingIndices.Contains(i))
                {
                    var disabledItem = displayed[i];
                    disabledItem.Source = null;
                    this.canvas.Children.Remove(disabledItem);
                    this.pooledContainers.Add(disabledItem);
                }
            }

            foreach (var c in activeContainers)
            {
                c.Refresh();
            }

            this.refreshFlag = false;

            this.canvas.InvalidateArrange();
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                //^
                this.RefreshTexts(this.TopItem, this.topOffset + e.Delta * this.scrollLength);
            }
            else
            {
                //v
                this.RefreshTexts(this.bottomItem, this.bottomOffset + e.Delta * this.scrollLength);
            }
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshTexts(true);
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //^
            this.MoveUp();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //v
            this.MoveDown();
        }


        private void MoveUp()
        {
            //^
            this.RefreshTexts(this.TopItem, this.topOffset + this.unitLineHeight);
        }

        private void MoveDown()
        {
            //v
            this.RefreshTexts(this.bottomItem, this.bottomOffset - this.unitLineHeight);
        }

        private void MoveToHome()
        {
            var first = this.Texts.GetFirst();
            if (first != null)
            {
                this.RefreshTexts(first, 0);
            }
        }


        private int count = 0;
        private void Test()
        {
            this.Controller.Write
                (@"    textBlock1.Inlines.Add(new Underline(new Run(""lightweight"")));",
                new TextBrush(Brushes.Blue, 27, 3),
                new TextBrush(new SolidColorBrush(Color.FromRgb(43, 145, 175)), 31, 9),
                new TextBrush(Brushes.Blue, 41, 3),
                new TextBrush(new SolidColorBrush(Color.FromRgb(43, 145, 175)), 45, 3),
                new TextBrush(new SolidColorBrush(Color.FromRgb(163, 21, 21)), 49, 13));

            this.Controller.Write(@"    //textBlock1.Inlines.Add(new Italic(new Run(""small"")));",
                Brushes.Green);

            var text =
                "Lorem ipsum dolor sit amet, consecuuutetur adipiscing elit. Sed quanta sit alias,"
                + " nunc tantum possitne esse tanta. Quid ad utilitatem tantae pecuniae" +
                " Sullae consulatumsto mouudo ne improbos quidem, si essent boni viri." +
                " Duo Reges: constructio interrete. ";

            this.Controller
                .Write(string.Join("\r\n", text.Replace("u", "\n").Split('\n')
                .Select((x, c) => string.IsNullOrWhiteSpace(x) ? "" : $"{c}:{x}")));

            this.Controller.Write
                ("    textBlock1.Inlines.Add(new Under\nline(new Run(\"lightwe\night\")));",
                new TextBrush(Brushes.Blue, 27, 3),
                new TextBrush(new SolidColorBrush(Color.FromRgb(43, 145, 175)), 31, 10),
                new TextBrush(Brushes.Blue, 42, 3),
                new TextBrush(new SolidColorBrush(Color.FromRgb(43, 145, 175)), 46, 3),
                new TextBrush(new SolidColorBrush(Color.FromRgb(163, 21, 21)), 50, 14));

            this.Controller.Write("    //textBlock1.Inlines\n.Add(new Italic(new Run(\"small\")));",
                Brushes.Green);

            this.Controller
                .Write($"{count++}:MainWindow.xaml の相互作用ロジック {DateTime.Now}");

        }


        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            var uie = sender as UIElement;
            if (uie != null)
            {
                uie.CaptureMouse();
                this.mouseCaptureing = true;
            }

            var p = e.GetPosition(uie);

            this.prevMousePosition = p;

            var item = this.GetItemFromPosition(p.Y);

            var rangeSelect = Keyboard.Modifiers == ModifierKeys.Shift;

            this.SelectItem(item, rangeSelect);

            this.canvas.Focus();
        }
        private void SelectItem(BlockItem<FormattedText> item, bool rangeSelect)
        {

            var firstSelectedItem = this.selectedItems.FirstOrDefault();
            var prevSelectedCount = this.selectedItems.Count;

            //選択解除
            foreach (var t in this.selectedItems)
            {
                if (t != item)
                {
                    t.IsSelected = false;
                }
            }
            this.selectedItems.Clear();

            if (item == null)
            {
                if (prevSelectedCount > 0)
                {
                    this.RefreshTexts(true);
                }
                return;
            }



            if (rangeSelect && firstSelectedItem != null)
            {
                //範囲選択
                var direction = this.Texts.CompareItems(item, firstSelectedItem);

                var nextItem = firstSelectedItem;

                while (true)
                {
                    nextItem.IsSelected = true;
                    this.selectedItems.Add(nextItem);

                    if (nextItem == item)
                    {
                        break;
                    }

                    if (direction > 0)
                    {
                        //v
                        nextItem = nextItem.NextItem;
                    }
                    else if (direction < 0)
                    {
                        //^
                        nextItem = nextItem.PrevItem;
                    }
                    else
                    {
                        break;
                    }

                    if (nextItem == null)
                    {
                        break;
                    }
                }
            }
            else
            {
                //単独

                if (!item.IsSelected || prevSelectedCount <= 1)
                {
                    item.IsSelected = !item.IsSelected;
                }
                if (item.IsSelected)
                {
                    this.selectedItems.Add(item);
                }
            }

            this.RefreshTexts(true);
        }

        private BlockItem<FormattedText> GetItemFromPosition(double y)
        {
            var item = this.TopItem;
            var offset = this.topOffset;

            while (item != null)
            {
                offset += this.GetItemHeight(item.Value);
                if (offset > y)
                {
                    return item;
                }
                item = item.NextItem;
            }
            return null;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseCaptureing)
            {
                var p = e.GetPosition((UIElement)sender);

                var distance = (this.prevMousePosition - p).LengthSquared;
                if (distance < 1)
                {
                    return;
                }
                this.prevMousePosition = p;

                var item = this.GetItemFromPosition(p.Y);

                if (item != null)
                {
                    this.SelectItem(item, true);
                }
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var uie = sender as UIElement;
            if (this.mouseCaptureing && uie != null)
            {
                this.mouseCaptureing = false;
                uie.ReleaseMouseCapture();
            }
        }

        private void bar_SelectedPositionChanged(object arg1, double arg2)
        {
            this.RefreshTexts(this.Texts.GetItemFromReversedIndex((int)arg2), 0);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.CopySelectedTexts();
        }

        private void CopySelectedTexts()
        {

            var firstItem = this.selectedItems.FirstOrDefault();
            if (firstItem == null)
            {
                return;
            }

            string text;

            if (this.selectedItems.Count == 1)
            {
                text = firstItem.Value.Text;
            }
            else
            {
                var secondItem = this.selectedItems[1];
                var direction = this.Texts.CompareItems(firstItem, secondItem);

                var items = new List<string>(this.selectedItems.Count);


                var item = firstItem;
                while (item != null && item.IsSelected)
                {
                    items.Add(item.Value.Text);
                    if (direction > 0)
                    {
                        item = item.PrevItem;
                    }
                    else
                    {
                        item = item.NextItem;
                    }
                }

                if (direction > 0)
                {
                    items.Reverse();
                }

                text = string.Join(System.Environment.NewLine, items);
            }

            Clipboard.SetDataObject(text, true);
        }




        private void rootGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                this.MoveUp();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                this.MoveDown();
                e.Handled = true;
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageDown)
            {
                this.RefreshTexts(this.bottomItem, 0);
            }
            else if (e.Key == Key.PageUp)
            {
                this.RefreshTexts(this.TopItem, this.canvas.ActualHeight);
            }
            else if (e.Key == Key.Home)
            {
                this.MoveToHome();
            }
            else if (e.Key == Key.End)
            {
                this.ScrollToBottom();
            }
            else if (e.Key == Key.NumPad5)
            {
                this.Test();
            }
            //else if (e.Key == Key.Up)
            //{
            //    this.MoveUp();
            //}
            //else if (e.Key == Key.Down)
            //{
            //    this.MoveDown();
            //}
            else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.CopySelectedTexts();
            }
        }

        public void Dispose()
        {
            this.Controller = null;
            this.disposables.Dispose();
        }


        private class TextCanvas : Canvas, IDisposable
        {
            private bool IsTextChanged { get; set; } = false;

            public BlockItem<FormattedText> Source
            {
                get { return _fieldSource; }
                set
                {
                    if (_fieldSource != value)
                    {
                        _fieldSource = value;
                        if (value != null)
                        {

                            this.valueChangedDisposable.Disposable
                                = value.StateChanged.Subscribe(_ => this.Update());
                            this.Update();
                        }
                        else
                        {
                            this.valueChangedDisposable.Disposable = Disposable.Empty;
                        }
                    }
                }
            }
            private BlockItem<FormattedText> _fieldSource;


            private SerialDisposable valueChangedDisposable;
            private SerialDisposable isSelectedChangedDisposable;

            private static readonly Pen border = new Pen(Brushes.Transparent, 0);
            private static readonly Brush selectionBrush = new SolidColorBrush(Color.FromRgb(173, 214, 255));


            public TextCanvas()
            {
                this.valueChangedDisposable = new SerialDisposable();
                this.isSelectedChangedDisposable = new SerialDisposable();
            }

            protected override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);
                if (this.IsTextChanged)
                {
                    var source = this.Source;
                    var text = source?.Value;
                    if (text != null)
                    {
                        if (source.IsSelected)
                        {
                            var rect = new Rect(0, 0, text.Width, Math.Ceiling(text.Height));
                            dc.DrawRectangle(selectionBrush, border, rect);
                        }
                        dc.DrawText(text, default(Point));
                    }
                    this.IsTextChanged = false;
                }
            }

            public void Refresh()
            {
                if (this.IsTextChanged)
                {
                    this.InvalidateVisual();
                }
            }

            public void Update()
            {
                this.IsTextChanged = true;
            }

            public void Dispose()
            {
                this.valueChangedDisposable.Dispose();
                this.isSelectedChangedDisposable.Dispose();
            }
        }
    }
}

