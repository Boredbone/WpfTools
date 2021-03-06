﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfTools.Behaviors
{

    public class FullScreenBehavior
    {
        public static WindowState GetPrevWindowState(DependencyObject obj)
            => (WindowState)obj.GetValue(PrevWindowStateProperty);
        public static void SetPrevWindowState(DependencyObject obj, WindowState value)
            =>  obj.SetValue(PrevWindowStateProperty, value);
        public static readonly DependencyProperty PrevWindowStateProperty =
            DependencyProperty.RegisterAttached("PrevWindowState",
                typeof(WindowState), typeof(FullScreenBehavior),
                new PropertyMetadata(WindowState.Normal));

        #region IsFullScreen

        public static bool GetIsFullScreen(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFullScreenProperty);
        }

        public static void SetIsFullScreen(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFullScreenProperty, value);
        }

        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.RegisterAttached("IsFullScreen",
                typeof(bool), typeof(FullScreenBehavior),
                new PropertyMetadata(false, new PropertyChangedCallback(OnIsFullScreenChanged)));


        private static void OnIsFullScreenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as Window;
            var value = e.NewValue as bool?;

            if (element != null && value.HasValue)
            {
                if (value.Value)
                {
                    SetPrevWindowState(sender, element.WindowState);
                    element.WindowState = WindowState.Normal;
                    element.ResizeMode = ResizeMode.NoResize;
                    element.WindowStyle = WindowStyle.None;
                    element.Topmost = true;
                    element.WindowState = WindowState.Maximized;
                }
                else
                {
                    element.WindowState = GetPrevWindowState(sender);
                    element.Topmost = false;
                    element.WindowStyle = WindowStyle.SingleBorderWindow;
                    element.ResizeMode = ResizeMode.CanResize;
                }
            }
        }

        #endregion
        
    }
}
