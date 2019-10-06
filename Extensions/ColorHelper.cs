using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfTools.Extensions
{
    public static class ColorHelper
    {
        public static Color FromCode(uint code)
        {
            return Color.FromArgb(
                (byte)((code >> 24) & 0xFF),
                (byte)((code >> 16) & 0xFF),
                (byte)((code >> 8) & 0xFF),
                (byte)(code & 0xFF));
        }
        public static uint ToCode(this Color color)
        {
            return ((uint)color.A) << 24
                | ((uint)color.R) << 16
                | ((uint)color.G) << 8
                | (uint)color.B;
        }
    }
}
