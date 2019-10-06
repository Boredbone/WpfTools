using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfTools.Extensions
{
    public static class VisualExtensions
    {
        private static double xRate = 1.0;
        private static double yRate = 1.0;
        private static bool rateInitialized = false;

        public static Vector GetScreenPosition(this Visual visual)
        {
            var pt = visual.PointToScreen(new Point(0.0, 0.0));

            if (!rateInitialized)
            {
                var source = PresentationSource.FromVisual(visual);
                if (source != null && source.CompositionTarget != null)
                {
                    xRate = 1.0 / source.CompositionTarget.TransformToDevice.M11;
                    yRate = 1.0 / source.CompositionTarget.TransformToDevice.M22;
                    rateInitialized = true;
                }

            }
            return new Vector(pt.X * xRate, pt.Y * yRate);
        }
    }
}
