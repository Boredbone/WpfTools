using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Boredbone.XamlTools
{

    public class PointerTapEventArgs : EventArgs
    {
        public TimeSpan Span { get; set; }
        public TimeSpan Interval { get; set; }
        public Vector StartPosition { get; set; }
        public Vector EndPosition { get; set; }
        private MouseEventArgs InnerArgs { get; set; }
        private TouchEventArgs InnerTouchArgs { get; set; }
        public double SenderWidth { get; set; }
        public double SenderHeight { get; set; }
        public bool IsDoubleTap { get; set; }

        public bool IsTouch { get; set; }
        private Func<IInputElement, Point> PositionGetter;

        public PointerTapEventArgs(MouseEventArgs args)
        {
            this.IsTouch = false;
            this.InnerArgs = args;
        }

        public PointerTapEventArgs(TouchEventArgs args)
        {
            this.IsTouch = true;
            this.InnerTouchArgs = args;
        }


        public PointerTapEventArgs(Func<IInputElement, Point> positionGetter)
        {
            this.IsTouch = true;
            this.PositionGetter = positionGetter;
        }

        public Point GetPosition(IInputElement element)
        {
            return (this.PositionGetter != null) ? this.PositionGetter(element)
                : this.IsTouch ? this.InnerTouchArgs.GetTouchPoint(element).Position
                : this.InnerArgs.GetPosition(element);
        }

        public PointerTapEventArgs Clone()
        {
            var clone
                = (this.PositionGetter != null) ? new PointerTapEventArgs(this.PositionGetter)
                : this.IsTouch ? new PointerTapEventArgs(this.InnerTouchArgs)
                : new PointerTapEventArgs(this.InnerArgs);

            clone.Span = this.Span;


            clone.Span = this.Span;
            clone.Interval = this.Interval;
            clone.StartPosition = this.StartPosition;
            clone.EndPosition = this.EndPosition;
            clone.SenderWidth = this.SenderWidth;
            clone.SenderHeight = this.SenderHeight;

            return clone;
        }
    }
}
