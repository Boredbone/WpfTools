using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Boredbone.XamlTools.Extensions;

namespace WpfTools.Behaviors
{
    public class SliderBehavior
    {

        #region MoveToPointOnDrag

        public static bool GetMoveToPointOnDrag(DependencyObject obj)
        {
            return (bool)obj.GetValue(MoveToPointOnDragProperty);
        }
        public static void SetMoveToPointOnDrag(DependencyObject obj, bool value)
        {
            obj.SetValue(MoveToPointOnDragProperty, value);
        }

        public static readonly DependencyProperty MoveToPointOnDragProperty
            = DependencyProperty.RegisterAttached("MoveToPointOnDrag", typeof(bool),
                typeof(SliderBehavior), new PropertyMetadata(false, OnMoveToPointOnDragChanged));

        private static void OnMoveToPointOnDragChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            var slider = (Slider)sender;
            if ((bool)e.NewValue)
            {
                Thumb thumb = null;// slider.Descendants<Thumb>().FirstOrDefault();
                //ToolTip toolTip = null;
                //TextBlock toolTipText = null;

                slider.MouseMove += (obj2, mouseEvent) =>
                {
                    if (thumb == null)
                    {
                        thumb = slider.Descendants<Thumb>().FirstOrDefault();
                        //toolTipText = new TextBlock()
                        //{
                        //    Text = slider.Value.ToString()
                        //};
                        //
                        //toolTip = new ToolTip()
                        //{
                        //    Content= toolTipText,
                        //    //Content = new Border()
                        //    //{
                        //    //    Child = toolTipText,
                        //    //    BorderBrush = new SolidColorBrush(Colors.Gray),
                        //    //    BorderThickness = new Thickness(1),
                        //    //    Background = new SolidColorBrush(Colors.White),
                        //    //}
                        //};
                        //thumb.ToolTip = toolTip;
                    }
                    if (mouseEvent.LeftButton == MouseButtonState.Pressed)
                    {
                        /*
                        Point position = mouseEvent.GetPosition(slider);
                        double d = 1.0 / slider.ActualWidth * position.X;
                        var p = slider.Maximum * d;
                        slider.Value = p;
                        */
                        
                        thumb?.RaiseEvent(new MouseButtonEventArgs
                            (mouseEvent.MouseDevice, mouseEvent.Timestamp, MouseButton.Left)
                        {
                            RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                            Source = mouseEvent.Source,

                        });

                        //toolTipText.Text = slider.Value.ToString();

                        slider.RaiseEvent(new MouseButtonEventArgs
                            (mouseEvent.MouseDevice, mouseEvent.Timestamp, MouseButton.Left)
                        {
                            RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                            Source = mouseEvent.Source,

                        });
                    }
                };
            }
        }

        #endregion


        #region IsReceiveMouseWheel

        public static bool GetIsReceiveMouseWheel(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsReceiveMouseWheelProperty);
        }

        public static void SetIsReceiveMouseWheel(DependencyObject obj, bool value)
        {
            obj.SetValue(IsReceiveMouseWheelProperty, value);
        }

        public static readonly DependencyProperty IsReceiveMouseWheelProperty =
            DependencyProperty.RegisterAttached("IsReceiveMouseWheel",
                typeof(bool), typeof(SliderBehavior),
                new PropertyMetadata(false, new PropertyChangedCallback(OnIsReceiveMouseWheelChanged)));

        private static void OnIsReceiveMouseWheelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as Slider;
            var value = e.NewValue as bool?;

            if (element != null && value != null && value.Value)
            {
                element.MouseWheel += (o, ea) =>
                {
                    var newValue = element.Value + Math.Sign(ea.Delta) * element.SmallChange;
                    if (newValue < element.Minimum)
                    {
                        newValue = element.Minimum;
                    }
                    if (newValue > element.Maximum)
                    {
                        newValue = element.Maximum;
                    }
                    element.Value = newValue;
                };
            }
        }

        #endregion

        #region IsReceivePreviewMouseWheel

        public static bool GetIsReceivePreviewMouseWheel(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsReceivePreviewMouseWheelProperty);
        }

        public static void SetIsReceivePreviewMouseWheel(DependencyObject obj, bool value)
        {
            obj.SetValue(IsReceivePreviewMouseWheelProperty, value);
        }

        public static readonly DependencyProperty IsReceivePreviewMouseWheelProperty =
            DependencyProperty.RegisterAttached("IsReceivePreviewMouseWheel",
                typeof(bool), typeof(SliderBehavior),
                new PropertyMetadata(false, new PropertyChangedCallback(OnIsReceivePreviewMouseWheelChanged)));

        private static void OnIsReceivePreviewMouseWheelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as Slider;
            var value = e.NewValue as bool?;

            if (element != null && value != null && value.Value)
            {
                element.PreviewMouseWheel += (o, ea) =>
                {
                    var newValue = element.Value + Math.Sign(ea.Delta) * element.SmallChange;
                    if (newValue < element.Minimum)
                    {
                        newValue = element.Minimum;
                    }
                    if (newValue > element.Maximum)
                    {
                        newValue = element.Maximum;
                    }
                    element.Value = newValue;

                    ea.Handled = true;
                };
            }
        }

        #endregion

        #region IsSnapToTick

        public static bool GetIsSnapToTick(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSnapToTickProperty);
        }

        public static void SetIsSnapToTick(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSnapToTickProperty, value);
        }

        public static readonly DependencyProperty IsSnapToTickProperty =
            DependencyProperty.RegisterAttached("IsSnapToTick",
                typeof(bool), typeof(SliderBehavior),
                new PropertyMetadata(false, new PropertyChangedCallback(OnIsSnapToTickChanged)));

        private static void OnIsSnapToTickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as Slider;
            var value = e.NewValue as bool?;

            if (element != null && value != null && value.Value)
            {
                element.ValueChanged += (o, ea) =>
                {
                    double rounded = Math.Round
                        (element.Value / element.SmallChange) * element.SmallChange;

                    if (element.Value != rounded)
                    {
                        element.Value = rounded;
                    }


                };
            }
        }

        #endregion


    }
}
