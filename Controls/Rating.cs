using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Boredbone.Utility.Extensions;
using Boredbone.XamlTools.Extensions;

namespace WpfTools.Controls
{
    public class Rating : Control
    {
        #region Maximum

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(Rating),
            new PropertyMetadata(6, new PropertyChangedCallback(OnMaximumChanged)));

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as Rating;
            var value = e.NewValue as int?;

            if (thisInstance != null && value.HasValue)
            {
                thisInstance.RefreshLength();
            }
        }

        #endregion

        #region ItemTemplate

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(Rating),
            new PropertyMetadata(null, new PropertyChangedCallback(OnItemTemplateChanged)));

        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as Rating;
            var value = e.NewValue as DataTemplate;

            if (thisInstance != null && value != null && thisInstance.Items != null)
            {
                thisInstance.Items.ItemTemplate = value;
            }
        }

        #endregion

        #region Value

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(Rating),
            new PropertyMetadata(0, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as Rating;

            if (thisInstance != null)
            {
                thisInstance.SetRatingValue();
            }

        }

        #endregion

        #region SymbolBackGround

        public Brush SymbolBackGround
        {
            get { return (Brush)GetValue(SymbolBackGroundProperty); }
            set { SetValue(SymbolBackGroundProperty, value); }
        }

        public static readonly DependencyProperty SymbolBackGroundProperty =
            DependencyProperty.Register(nameof(SymbolBackGround), typeof(Brush),
                typeof(Rating), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        #endregion

        #region SymbolHoverBackGround

        public Brush SymbolHoverBackGround
        {
            get { return (Brush)GetValue(SymbolHoverBackGroundProperty); }
            set { SetValue(SymbolHoverBackGroundProperty, value); }
        }

        public static readonly DependencyProperty SymbolHoverBackGroundProperty =
            DependencyProperty.Register(nameof(SymbolHoverBackGround), typeof(Brush),
                typeof(Rating), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        #endregion


        #region ItemsMargin

        public Thickness ItemsMargin
        {
            get { return (Thickness)GetValue(ItemsMarginProperty); }
            set { SetValue(ItemsMarginProperty, value); }
        }

        public static readonly DependencyProperty ItemsMarginProperty =
            DependencyProperty.Register(nameof(ItemsMargin), typeof(Thickness),
                typeof(Rating), new PropertyMetadata(new Thickness(0)));

        #endregion




        public ObservableCollection<RatingItemDataContext> RatingCollection { get; }
        public Slider Slider { get; private set; }
        public ItemsControl Items { get; private set; }
        public Border ItemsBorder { get; private set; }

        private Thumb thumb = null;


        static Rating()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Rating),
                new FrameworkPropertyMetadata(typeof(Rating)));
        }

        public Rating()
        {
            this.RatingCollection = new ObservableCollection<RatingItemDataContext>();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.Slider = this.GetTemplateChild("PART_slider") as Slider;
            this.Items = this.GetTemplateChild("PART_items") as ItemsControl;
            this.ItemsBorder = this.GetTemplateChild("PART_itemsBorder") as Border;

            this.Slider.ValueChanged += (o, e) => this.Value = (int)Math.Round(e.NewValue);

            this.ItemsBorder.Background = this.SymbolBackGround;
            this.Slider.MouseEnter += (o, e) => this.ItemsBorder.Background = this.SymbolHoverBackGround;
            this.Slider.MouseLeave += (o, e) => this.ItemsBorder.Background = this.SymbolBackGround;
            
            this.RefreshLength();

            this.Items.SizeChanged += (o, e) =>
            {
                if (this.thumb == null)
                {
                    this.thumb = this.Slider.Descendants<Thumb>().FirstOrDefault();
                }
                var thumbWidth = this.thumb?.ActualWidth / 2.0 ?? 0.0;

                var itemWidth = e.NewSize.Width / this.Maximum;
                this.ItemsBorder.Margin = new Thickness(itemWidth / 2.0 + thumbWidth, 0, thumbWidth, 0);
                this.ItemsMargin = this.ItemsBorder.Margin;
            };
            this.Items.ItemTemplate = this.ItemTemplate;
            this.Items.ItemsSource = this.RatingCollection;

            this.Items.Loaded += (o, e) => this.SetRatingValue();
        }


        private void RefreshLength()
        {
            if (this.Slider != null)
            {
                this.Slider.Maximum = this.Maximum + 0.49;
            }

            this.RatingCollection.Clear();
            Enumerable.Range(1, this.Maximum)
                .Select(c => new RatingItemDataContext() { Value = c, IsSelected = false })
                .ForEach(x => this.RatingCollection.Add(x));


        }

        private void SetRatingValue()
        {
            var rating = this.Value;
            this.RatingCollection
                .ForEach((x, c) => x.IsSelected = (c < rating));
        }
    }

    public class RatingItemDataContext : INotifyPropertyChanged
    {
        public int Value
        {
            get { return _fieldValue; }
            set
            {
                if (_fieldValue != value)
                {
                    _fieldValue = value;
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }
        private int _fieldValue;

        public bool IsSelected
        {
            get { return _fieldIsSelected; }
            set
            {
                if (_fieldIsSelected != value)
                {
                    _fieldIsSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                }
            }
        }
        private bool _fieldIsSelected;



        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
