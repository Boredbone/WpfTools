using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

namespace WpfTools.Controls
{
    public class PositionBar : Control
    {

        #region Maximum

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(PositionBar),
            new PropertyMetadata(100.0, new PropertyChangedCallback(OnMaximumChanged)));

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as PositionBar;
            var value = (double)e.NewValue;

            if (thisInstance != null)
            {
                thisInstance.maximum = value;
                thisInstance.CheckRange();
                thisInstance.UpdateThumbPosition();
            }

        }

        private double maximum = 100.0;

        #endregion

        #region Minimum

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(PositionBar),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnMinimumChanged)));

        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as PositionBar;
            var value = (double)e.NewValue;

            if (thisInstance != null)
            {
                thisInstance.minimum = value;
                thisInstance.CheckRange();
                thisInstance.UpdateThumbPosition();
            }

        }

        private double minimum = 0.0;

        #endregion

        #region SelectedPosition

        public double SelectedPosition
        {
            get { return (double)GetValue(SelectedPositionProperty); }
            set { SetValue(SelectedPositionProperty, value); }
        }

        public static readonly DependencyProperty SelectedPositionProperty =
            DependencyProperty.Register(nameof(SelectedPosition), typeof(double),
                typeof(PositionBar), new PropertyMetadata(-1.0));


        #endregion

        #region DisplayPosition

        public double DisplayPosition
        {
            get { return (double)GetValue(DisplayPositionProperty); }
            set { SetValue(DisplayPositionProperty, value); }
        }

        public static readonly DependencyProperty DisplayPositionProperty =
            DependencyProperty.Register(nameof(DisplayPosition), typeof(double), typeof(PositionBar),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnDisplayPositionChanged)));

        private static void OnDisplayPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as PositionBar;
            var value = (double)e.NewValue;

            if (thisInstance != null)
            {
                thisInstance.displayPosition = value;
                thisInstance.UpdateThumbPosition();
            }

        }

        private double displayPosition = 0.0;

        #endregion

        #region MinimumThumbHeight

        public double MinimumThumbHeight
        {
            get { return (double)GetValue(MinimumThumbHeightProperty); }
            set { SetValue(MinimumThumbHeightProperty, value); }
        }

        public static readonly DependencyProperty MinimumThumbHeightProperty =
            DependencyProperty.Register(nameof(MinimumThumbHeight), typeof(double), typeof(PositionBar),
            new PropertyMetadata(8.0, new PropertyChangedCallback(OnMinimumThumbHeightChanged)));

        private static void OnMinimumThumbHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as PositionBar;
            var value = (double)e.NewValue;

            if (thisInstance != null)
            {
                thisInstance.minimumThumbHeight = value;
                thisInstance.UpdateThumbPosition();
            }
        }

        private double minimumThumbHeight = 8.0;

        #endregion

        #region IsReversed

        public bool IsReversed
        {
            get { return (bool)GetValue(IsReversedProperty); }
            set { SetValue(IsReversedProperty, value); }
        }

        public static readonly DependencyProperty IsReversedProperty =
            DependencyProperty.Register(nameof(IsReversed), typeof(bool), typeof(PositionBar),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsReversedChanged)));

        private static void OnIsReversedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as PositionBar;
            var value = (bool)e.NewValue;

            if (thisInstance != null)
            {
                thisInstance.prevPosition = -1.0;
                thisInstance.isReversed = value;
                if (thisInstance.thumb != null)
                {
                    thisInstance.thumb.VerticalAlignment
                        = value ? VerticalAlignment.Bottom : VerticalAlignment.Top;
                    thisInstance.UpdateThumbPosition();
                }
            }
        }

        private bool isReversed = false;

        #endregion

        #region MouseOverForeground

        public Brush MouseOverForeground
        {
            get { return (Brush)GetValue(MouseOverForegroundProperty); }
            set { SetValue(MouseOverForegroundProperty, value); }
        }

        public static readonly DependencyProperty MouseOverForegroundProperty =
            DependencyProperty.Register(nameof(MouseOverForeground), typeof(Brush), typeof(PositionBar),
            new PropertyMetadata(Brushes.LightGray, new PropertyChangedCallback(OnMouseOverForegroundChanged)));

        private static void OnMouseOverForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as PositionBar;
            var value = e.NewValue as Brush;

        }

        #endregion

        #region PressedForeground

        public Brush PressedForeground
        {
            get { return (Brush)GetValue(PressedForegroundProperty); }
            set { SetValue(PressedForegroundProperty, value); }
        }

        public static readonly DependencyProperty PressedForegroundProperty =
            DependencyProperty.Register(nameof(PressedForeground), typeof(Brush), typeof(PositionBar),
            new PropertyMetadata(Brushes.Gray, new PropertyChangedCallback(OnPressedForegroundChanged)));

        private static void OnPressedForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as PositionBar;
            var value = e.NewValue as Brush;

        }

        #endregion

        #region IsMouseCaptureing

        public bool IsMouseCaptureing
        {
            get { return (bool)GetValue(IsMouseCaptureingProperty); }
            set { SetValue(IsMouseCaptureingProperty, value); }
        }

        public static readonly DependencyProperty IsMouseCaptureingProperty =
            DependencyProperty.Register(nameof(IsMouseCaptureing), typeof(bool),
                typeof(PositionBar), new PropertyMetadata(false));

        #endregion




        private double prevPosition = -1.0;

        public bool MouseCaptureing
        {
            get { return _fieldMouseCaptureing; }
            set
            {
                if (_fieldMouseCaptureing != value)
                {
                    _fieldMouseCaptureing = value;
                    this.IsMouseCaptureing = value;
                }
            }
        }
        private bool _fieldMouseCaptureing;
        
        private double prevMousePosition = -1.0;

        private bool initialized = false;

        public event Action<object, double> SelectedPositionChanged;

        private Border mainBorder = null;
        private Border thumb = null;


        public PositionBar()
        {
            this.SizeChanged += (o, e) => this.UpdateThumbPosition();
            this.Loaded += (o, e) =>
            {
                this.initialized = true;
                this.UpdateThumbPosition();
            };
        }
        static PositionBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PositionBar),
                new FrameworkPropertyMetadata(typeof(PositionBar)));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.mainBorder = this.GetTemplateChild("PART_RootBorder") as Border;
            this.thumb = this.GetTemplateChild("PART_Thumb") as Border;


            this.mainBorder.MouseLeftButtonDown += this.Grid_MouseLeftButtonDown;
            this.mainBorder.MouseLeftButtonUp += this.Grid_MouseLeftButtonUp;
            this.mainBorder.MouseMove += this.Grid_MouseMove;
            this.mainBorder.MouseLeave += this.Grid_MouseLeave;

            this.thumb.VerticalAlignment = (this.isReversed) ? VerticalAlignment.Bottom : VerticalAlignment.Top;
            this.UpdateThumbPosition();
        }

        private void UpdateThumbPosition()
        {
            if (!this.initialized)
            {
                return;
            }

            if (this.maximum == this.minimum)
            {
                return;
            }

            var totalHeight = this.ActualHeight;

            var unitHeight = totalHeight / (this.maximum - this.minimum + 1);

            if (unitHeight < this.minimumThumbHeight)
            {
                unitHeight = this.minimumThumbHeight;

                var movingHeight = totalHeight - this.minimumThumbHeight;
                unitHeight = movingHeight / (this.maximum - this.minimum);

                this.thumb.Height = unitHeight + this.minimumThumbHeight;
            }
            else
            {
                this.thumb.Height = unitHeight * 2.0;
            }

            var position = Math.Round(this.displayPosition * unitHeight);

            if (this.prevPosition == position)
            {
                return;
            }

            prevPosition = position;

            if (this.isReversed)
            {
                this.thumb.Margin = new Thickness(0.0, 0.0, 0.0, position);
            }
            else
            {
                this.thumb.Margin = new Thickness(0.0, position, 0.0, 0.0);
            }

        }

        private void UpdateSelectedPosition(double y)
        {
            var distance = this.prevMousePosition - y;
            if (distance < 1 && distance > -1)
            {
                return;
            }

            if (this.maximum == this.minimum)
            {
                return;
            }

            this.prevMousePosition = y;

            var normalizedPosition = y / this.ActualHeight;

            if (this.isReversed)
            {
                normalizedPosition = 1.0 - normalizedPosition;
            }
            var position = Math.Floor(normalizedPosition * (this.maximum - this.minimum));

            if (position < this.minimum)
            {
                position = this.minimum;
            }
            if (position >= this.maximum)
            {
                position = this.maximum - 1.0;
            }
            this.SelectedPosition = position;
            this.SelectedPositionChanged?.Invoke(this, position);
        }

        private void CheckRange()
        {
            if (this.thumb != null)
            {
                this.IsEnabled = (this.maximum - this.minimum > 1);
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var uie = sender as UIElement;
            if (uie != null)
            {
                uie.CaptureMouse();
                this.MouseCaptureing = true;
            }

            var p = e.GetPosition(uie);
            this.UpdateSelectedPosition(p.Y);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var uie = sender as UIElement;
            if (this.MouseCaptureing && uie != null)
            {
                this.MouseCaptureing = false;
                uie.ReleaseMouseCapture();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.MouseCaptureing)
            {
                var p = e.GetPosition((UIElement)sender);
                this.UpdateSelectedPosition(p.Y);
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }
}
