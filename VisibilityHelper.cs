using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfTools
{
    public class VisibilityHelper
    {
        public static Visibility Set(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
