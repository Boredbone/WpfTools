using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace WpfTools.Behaviors
{
    public class HyperlinkBehavior
    {
        #region NavigateByClick

        public static bool GetNavigateByClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(NavigateByClickProperty);
        }

        public static void SetNavigateByClick(DependencyObject obj, bool value)
        {
            obj.SetValue(NavigateByClickProperty, value);
        }

        public static readonly DependencyProperty NavigateByClickProperty =
            DependencyProperty.RegisterAttached("NavigateByClick",
                typeof(bool), typeof(HyperlinkBehavior),
                new PropertyMetadata(false, new PropertyChangedCallback(OnNavigateByClickChanged)));

        private static void OnNavigateByClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as Hyperlink;
            var value = e.NewValue as bool?;

            if (element != null && value != null && value.Value)
            {
                element.RequestNavigate += (_, __) =>
                {
                    System.Diagnostics.Process.Start(element.NavigateUri.ToString());
                };
            }
        }

        #endregion



    }
}
