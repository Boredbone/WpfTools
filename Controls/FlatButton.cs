using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfTools.Controls
{
    public class FlatButton : Button
    {
        #region PointerHoverBackground

        public Brush PointerHoverBackground
        {
            get { return (Brush)GetValue(PointerHoverBackgroundProperty); }
            set { SetValue(PointerHoverBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PointerHoverBackgroundProperty =
            DependencyProperty.Register(nameof(PointerHoverBackground), typeof(Brush),
                typeof(FlatButton), new PropertyMetadata(null));

        #endregion

        #region PointerHoverBorderBrush

        public Brush PointerHoverBorderBrush
        {
            get { return (Brush)GetValue(PointerHoverBorderBrushProperty); }
            set { SetValue(PointerHoverBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty PointerHoverBorderBrushProperty =
            DependencyProperty.Register(nameof(PointerHoverBorderBrush), typeof(Brush),
                typeof(FlatButton), new PropertyMetadata(null));

        #endregion

        #region PressedBackground

        public Brush PressedBackground
        {
            get { return (Brush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register(nameof(PressedBackground), typeof(Brush),
                typeof(FlatButton), new PropertyMetadata(null));

        #endregion

        #region PressedBorderBrush

        public Brush PressedBorderBrush
        {
            get { return (Brush)GetValue(PressedBorderBrushProperty); }
            set { SetValue(PressedBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty PressedBorderBrushProperty =
            DependencyProperty.Register(nameof(PressedBorderBrush), typeof(Brush),
                typeof(FlatButton), new PropertyMetadata(null));

        #endregion

        #region PressedBorderThickness

        public Thickness PressedBorderThickness
        {
            get { return (Thickness)GetValue(PressedBorderThicknessProperty); }
            set { SetValue(PressedBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty PressedBorderThicknessProperty =
            DependencyProperty.Register(nameof(PressedBorderThickness), typeof(Thickness),
                typeof(FlatButton), new PropertyMetadata(new Thickness(1)));

        #endregion

        #region DisabledBackground

        public Brush DisabledBackground
        {
            get { return (Brush)GetValue(DisabledBackgroundProperty); }
            set { SetValue(DisabledBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DisabledBackgroundProperty =
            DependencyProperty.Register(nameof(DisabledBackground), typeof(Brush),
                typeof(FlatButton), new PropertyMetadata(null));

        #endregion

        #region DisabledBorderBrush

        public Brush DisabledBorderBrush
        {
            get { return (Brush)GetValue(DisabledBorderBrushProperty); }
            set { SetValue(DisabledBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty DisabledBorderBrushProperty =
            DependencyProperty.Register(nameof(DisabledBorderBrush), typeof(Brush),
                typeof(FlatButton), new PropertyMetadata(null));

        #endregion

        #region DisabledForeground

        public Brush DisabledForeground
        {
            get { return (Brush)GetValue(DisabledForegroundProperty); }
            set { SetValue(DisabledForegroundProperty, value); }
        }

        public static readonly DependencyProperty DisabledForegroundProperty =
            DependencyProperty.Register(nameof(DisabledForeground), typeof(Brush),
                typeof(FlatButton), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        #endregion

        #region NormalForeground

        public Brush NormalForeground
        {
            get { return (Brush)GetValue(NormalForegroundProperty); }
            set { SetValue(NormalForegroundProperty, value); }
        }

        public static readonly DependencyProperty NormalForegroundProperty =
            DependencyProperty.Register(nameof(NormalForeground), typeof(Brush),
                typeof(FlatButton), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        #endregion
        //private Brush foreground;

        static FlatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatButton),
                new FrameworkPropertyMetadata(typeof(FlatButton)));
        }

        public FlatButton()
        {
            //this.Visibility = Visibility.Collapsed;
            //if (this.IsEnabled)
            //{
            //    this.Foreground = this.NormalForeground;
            //}
            //else
            //{
            //    this.Foreground = this.DisabledForeground;
            //}

            //this.foreground = this.Foreground;
            //this.IsEnabledChanged += (o, e) =>
            //{
            //    if (this.IsEnabled)
            //    {
            //        //this.Foreground = this.foreground;
            //    }
            //    else
            //    {
            //        //this.Foreground = this.DisabledForeground;
            //    }
            //};
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //this.Visibility = Visibility.Visible;
            //if (this.IsEnabled)
            //{
            //    this.Foreground = this.NormalForeground;
            //}
            //else
            //{
            //    this.Foreground = this.DisabledForeground;
            //}
            //this.foreground = this.Foreground;
        }

    }
}
