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
    public class LabeledIcon : Control
    {

        #region IconText

        public string IconText
        {
            get { return (string)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value); }
        }

        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register(nameof(IconText), typeof(string), typeof(LabeledIcon),
            new PropertyMetadata(null, new PropertyChangedCallback(OnIconTextChanged)));

        private static void OnIconTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as LabeledIcon;
            var value = e.NewValue as string;

            if (thisInstance != null && thisInstance.Icon!=null)
            {
                thisInstance.Icon.Text = value;
            }
        }

        #endregion

        #region IconWidth

        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register(nameof(IconWidth), typeof(double), typeof(LabeledIcon),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnIconWidthChanged)));

        private static void OnIconWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as LabeledIcon;
            var value = e.NewValue as double?;

            if (thisInstance != null && value.HasValue && thisInstance.Icon != null)
            {
                thisInstance.Icon.Width = value.Value;
            }
        }

        #endregion


        #region IconFontFamily

        public FontFamily IconFontFamily
        {
            get { return (FontFamily)GetValue(IconFontFamilyProperty); }
            set { SetValue(IconFontFamilyProperty, value); }
        }

        public static readonly DependencyProperty IconFontFamilyProperty =
            DependencyProperty.Register(nameof(IconFontFamily), typeof(FontFamily), typeof(LabeledIcon),
            new PropertyMetadata(null, new PropertyChangedCallback(OnIconFontFamilyChanged)));

        private static void OnIconFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as LabeledIcon;
            var value = e.NewValue as FontFamily;

            if (thisInstance != null && thisInstance.Icon != null)
            {
                thisInstance.Icon.FontFamily = value;
            }
        }

        #endregion

        #region IconFontSize

        public double IconFontSize
        {
            get { return (double)GetValue(IconFontSizeProperty); }
            set { SetValue(IconFontSizeProperty, value); }
        }

        public static readonly DependencyProperty IconFontSizeProperty =
            DependencyProperty.Register(nameof(IconFontSize), typeof(double), typeof(LabeledIcon),
            new PropertyMetadata(10.0, new PropertyChangedCallback(OnIconFontSizeChanged)));

        private static void OnIconFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as LabeledIcon;
            var value = e.NewValue as double?;

            if (thisInstance != null && value.HasValue && thisInstance.Icon != null)
            {
                thisInstance.Icon.FontSize = value.Value;
            }
        }

        #endregion

        #region Text

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(LabeledIcon),
            new PropertyMetadata(null, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as LabeledIcon;
            var value = e.NewValue as string;

            if (thisInstance != null && thisInstance.Icon != null)
            {
                thisInstance.Label.Text = value;
                if (thisInstance.ToolTip == null || thisInstance.ToolTip.ToString().Length <= 0)
                {
                    thisInstance.ToolTip = value;
                }
            }
        }

        #endregion
        

        public TextBlock Icon { get; private set; }
        public TextBlock Label { get; private set; }


        static LabeledIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabeledIcon),
                new FrameworkPropertyMetadata(typeof(LabeledIcon)));
        }

        public LabeledIcon()
        {

        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            this.Icon = this.GetTemplateChild("PART_Icon") as TextBlock;
            this.Label = this.GetTemplateChild("PART_Label") as TextBlock;

            this.Icon.Text = this.IconText;
            this.Icon.Width = this.IconWidth;
            if (this.IconFontFamily != null)
            {
                this.Icon.FontFamily = this.IconFontFamily;
            }
            this.Icon.FontSize = this.IconFontSize;
            
            //this.ToolTip = this.Text;
            if (this.ToolTip == null || this.ToolTip.ToString().Length <= 0)
            {
                this.ToolTip = this.Text;
            }

            this.Label.Text = this.Text;
        }
    }
}
