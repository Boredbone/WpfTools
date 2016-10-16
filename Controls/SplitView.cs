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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTools.Controls
{

    public class SplitView : Control
    {
        static SplitView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitView), 
                new FrameworkPropertyMetadata(typeof(SplitView)));
        }

        #region CompactPaneLength

        public double CompactPaneLength
        {
            get { return (double)GetValue(CompactPaneLengthProperty); }
            set { SetValue(CompactPaneLengthProperty, value); }
        }

        public static readonly DependencyProperty CompactPaneLengthProperty =
            DependencyProperty.Register(nameof(CompactPaneLength), typeof(double), typeof(SplitView),
            new PropertyMetadata(48.0, new PropertyChangedCallback(OnCompactPaneLengthChanged)));

        private static void OnCompactPaneLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as SplitView;
            var value = e.NewValue as double?;

            thisInstance.RefreshView();
        }

        #endregion

        #region OpenPaneLength

        public double OpenPaneLength
        {
            get { return (double)GetValue(OpenPaneLengthProperty); }
            set { SetValue(OpenPaneLengthProperty, value); }
        }

        public static readonly DependencyProperty OpenPaneLengthProperty =
            DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(SplitView),
            new PropertyMetadata(320.0, new PropertyChangedCallback(OnOpenPaneLengthChanged)));

        private static void OnOpenPaneLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as SplitView;
            var value = e.NewValue as double?;

            thisInstance.RefreshView();
        }

        #endregion

        #region DisplayMode

        public SplitViewDisplayMode DisplayMode
        {
            get { return (SplitViewDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(nameof(DisplayMode), typeof(SplitViewDisplayMode), typeof(SplitView),
            new PropertyMetadata(SplitViewDisplayMode.CompactOverlay, new PropertyChangedCallback(OnDisplayModeChanged)));

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as SplitView;
            var value = e.NewValue as SplitViewDisplayMode?;

            thisInstance.RefreshView();
        }

        #endregion

        #region IsPaneOpen

        public bool IsPaneOpen
        {
            get { return (bool)GetValue(IsPaneOpenProperty); }
            set { SetValue(IsPaneOpenProperty, value); }
        }

        public static readonly DependencyProperty IsPaneOpenProperty =
            DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(SplitView),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsPaneOpenChanged)));

        private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as SplitView;
            var value = e.NewValue as bool?;

            var oldValue = e.OldValue as bool?;
            if (thisInstance.pane == null)
            {
                return;
            }
            if (value != null && oldValue != null && value.Value && !oldValue.Value)
            {
                thisInstance.pane.Focus();
            }

            thisInstance.RefreshView();
        }

        #endregion

        #region Pane

        public UIElement Pane
        {
            get { return (UIElement)GetValue(PaneProperty); }
            set { SetValue(PaneProperty, value); }
        }

        public static readonly DependencyProperty PaneProperty =
            DependencyProperty.Register(nameof(Pane), typeof(UIElement), typeof(SplitView),
            new PropertyMetadata(null, new PropertyChangedCallback(OnPaneChanged)));

        private static void OnPaneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as SplitView;
            var value = e.NewValue as UIElement;
            if (thisInstance.pane != null)
            {
                thisInstance.pane.Child = value;
            }
        }

        #endregion

        #region MainContent

        public UIElement MainContent
        {
            get { return (UIElement)GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(nameof(MainContent), typeof(UIElement), typeof(SplitView),
            new PropertyMetadata(null, new PropertyChangedCallback(OnMainContentChanged)));

        private static void OnMainContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as SplitView;
            var value = e.NewValue as UIElement;

            if (thisInstance.mainContent != null)
            {
                thisInstance.mainContent.Child = value;
            }
        }

        #endregion

        #region MiddleContent

        public UIElement MiddleContent
        {
            get { return (UIElement)GetValue(MiddleContentProperty); }
            set { SetValue(MiddleContentProperty, value); }
        }

        public static readonly DependencyProperty MiddleContentProperty =
            DependencyProperty.Register(nameof(MiddleContent), typeof(UIElement), typeof(SplitView),
            new PropertyMetadata(null, new PropertyChangedCallback(OnMiddleContentChanged)));

        private static void OnMiddleContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as SplitView;
            var value = e.NewValue as UIElement;

            if (thisInstance.middleContent != null)
            {
                thisInstance.middleContent.Child = value;
            }
        }

        #endregion



        #region PaneBackground

        public Brush PaneBackground
        {
            get { return (Brush)GetValue(PaneBackgroundProperty); }
            set { SetValue(PaneBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PaneBackgroundProperty =
            DependencyProperty.Register(nameof(PaneBackground), typeof(Brush), typeof(SplitView),
            new PropertyMetadata(new SolidColorBrush(Colors.Transparent),
                new PropertyChangedCallback(OnPaneBackgroundChanged)));

        private static void OnPaneBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as SplitView;
            var value = e.NewValue as Brush;

            if (thisInstance.pane == null)
            {
                return;
            }
            thisInstance.pane.Background = value;

        }

        #endregion

        private void RefreshView()
        {
            if (this.pane == null)
            {
                return;
            }
            var oldPaneWidth = this.pane.ActualWidth;
            var newPaneWidth = oldPaneWidth;

            if (this.IsPaneOpen)
            {

                if (this.DisplayMode == SplitViewDisplayMode.Overlay
                    || this.DisplayMode == SplitViewDisplayMode.CompactOverlay)
                {
                    this.cover.Visibility = Visibility.Visible;
                }
                else
                {
                    this.cover.Visibility = Visibility.Collapsed;
                }

                this.panePlace.Width
                    = (this.DisplayMode == SplitViewDisplayMode.Overlay) ? 0.0
                    : (this.DisplayMode == SplitViewDisplayMode.CompactOverlay) ? this.CompactPaneLength
                    : this.OpenPaneLength;

                newPaneWidth = this.OpenPaneLength;
            }
            else
            {
                this.cover.Visibility = Visibility.Collapsed;

                if (this.DisplayMode == SplitViewDisplayMode.CompactInline
                    || this.DisplayMode == SplitViewDisplayMode.CompactOverlay)
                {
                    this.panePlace.Width = this.CompactPaneLength;
                    newPaneWidth = this.CompactPaneLength;
                }
                else
                {
                    this.panePlace.Width = 0.0;
                    newPaneWidth = 0.0;
                }
            }

            if (oldPaneWidth != newPaneWidth
                && !double.IsNaN(this.pane.Width) && !double.IsInfinity(this.pane.Width))
            {
                var storyboard = new Storyboard();

                var a = new DoubleAnimation();

                Storyboard.SetTarget(a, this.pane);
                Storyboard.SetTargetProperty(a, new PropertyPath("(Border.Width)"));
                a.To = newPaneWidth;
                a.Duration = TimeSpan.FromMilliseconds(150);

                a.EasingFunction = new ExponentialEase()
                {
                    EasingMode = (oldPaneWidth < newPaneWidth) ? EasingMode.EaseOut : EasingMode.EaseIn,
                    Exponent = (oldPaneWidth < newPaneWidth) ? 4 : 2,
                };

                storyboard.Children.Add(a);
                
                storyboard.Begin();
            }
            else
            {
                this.pane.Width = newPaneWidth;
            }
        }

        private Border pane;
        private Border mainContent;
        private Border middleContent;
        private Border panePlace;
        private Border cover;


        public SplitView()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.pane = this.GetTemplateChild("PART_pane") as Border;
            this.mainContent = this.GetTemplateChild("PART_mainContent") as Border;
            this.middleContent = this.GetTemplateChild("PART_middleContent") as Border;
            this.panePlace = this.GetTemplateChild("PART_panePlace") as Border;
            this.cover = this.GetTemplateChild("PART_cover") as Border;

            
            this.cover.MouseDown += this.cover_MouseDown;

            this.pane.Child = this.Pane;
            this.mainContent.Child = this.MainContent;
            this.middleContent.Child = this.MiddleContent;
            this.pane.Background = this.PaneBackground;
            
            this.RefreshView();
        }

        private void cover_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsPaneOpen = false;
        }
    }

    public enum SplitViewDisplayMode
    {
        Overlay,
        Inline,
        CompactOverlay,
        CompactInline,
    }
}
