using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using System.Reactive.Subjects;
using System.Diagnostics;
#if WINDOWS_APP || WINDOWS_UWP
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Windows.Input;
#endif

namespace Boredbone.XamlTools.Extensions
{
    public static class EventAsObservable
    {
        public static IObservable<SizeChangedEventArgs> SizeChangedAsObservable(this FrameworkElement target)
            => Observable.FromEvent<SizeChangedEventHandler, SizeChangedEventArgs>
                (h => (sender, e) => h(e), h => target.SizeChanged += h, h => target.SizeChanged -= h);

        public static IObservable<RoutedEventArgs> LoadedAsObservable(this FrameworkElement target)
            => Observable.FromEvent<RoutedEventHandler, RoutedEventArgs>
                (h => (sender, e) => h(e), h => target.Loaded += h, h => target.Loaded -= h);


        public static IObservable<RoutedEventArgs> UnloadedAsObservable(this FrameworkElement target)
            => Observable.FromEvent<RoutedEventHandler, RoutedEventArgs>
                (h => (sender, e) => h(e), h => target.Unloaded += h, h => target.Unloaded -= h);





        public static IObservable<PointerTapEventArgs> PreviewPointerDownAsObservable
            (this UIElement target)
        {
            return Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
                h => (s, e) => h(e),
                h => target.PreviewMouseLeftButtonDown += h,
                h => target.PreviewMouseLeftButtonDown -= h)
                .Select(x => new PointerTapEventArgs(x))
                .Merge(Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.PreviewTouchDown += h,
                h => target.PreviewTouchDown -= h)
                .Where(_ => target.IsManipulationEnabled)
                .Select(x => new PointerTapEventArgs(x)));
        }


        public static IObservable<PointerTapEventArgs> PointerMoveAsObservable
            (this UIElement target)
        {
            return Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                h => (s, e) => h(e),
                h => target.MouseMove += h,
                h => target.MouseMove -= h)
                .Select(x => new PointerTapEventArgs(x))
                .Merge(Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.TouchMove += h,
                h => target.TouchMove -= h)
                .Where(_ => target.IsManipulationEnabled)
                .Select(x => new PointerTapEventArgs(x)));

        }
        public static IObservable<PointerTapEventArgs> PreviewPointerUpAsObservable
            (this UIElement target)
        {
            return Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
                h => (s, e) => h(e),
                h => target.PreviewMouseLeftButtonUp += h,
                h => target.PreviewMouseLeftButtonUp -= h)
                .Select(x => new PointerTapEventArgs(x))
                .Merge(Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.PreviewTouchUp += h,
                h => target.PreviewTouchUp -= h)
                .Where(_ => target.IsManipulationEnabled)
                .Select(x => new PointerTapEventArgs(x)));
        }


        public static IObservable<PointerTapEventArgs> ManipulationStartedAsObservable
            (this UIElement target)
        {
            return Observable.FromEvent<EventHandler<ManipulationStartedEventArgs>, ManipulationStartedEventArgs>(
                h => (s, e) => h(e),
                h => target.ManipulationStarted += h,
                h => target.ManipulationStarted -= h)
                .Where(_ => target.IsManipulationEnabled)
                .Select(x => new PointerTapEventArgs(_ => x.ManipulationOrigin));
        }
        public static IObservable<PointerTapEventArgs> ManipulationDeltaAsObservable
            (this UIElement target)
        {
            return Observable.FromEvent<EventHandler<ManipulationDeltaEventArgs>, ManipulationDeltaEventArgs>(
                h => (s, e) => h(e),
                h => target.ManipulationDelta += h,
                h => target.ManipulationDelta -= h)
                .Where(_ => target.IsManipulationEnabled)
                .Select(x => new PointerTapEventArgs(_ => x.ManipulationOrigin));
        }
        public static IObservable<PointerTapEventArgs> ManipulationCompletedAsObservable
            (this UIElement target)
        {
            return Observable.FromEvent<EventHandler<ManipulationCompletedEventArgs>, ManipulationCompletedEventArgs>(
                h => (s, e) => h(e),
                h => target.ManipulationCompleted += h,
                h => target.ManipulationCompleted -= h)
                .Where(_ => target.IsManipulationEnabled)
                .Select(x => new PointerTapEventArgs(_ => x.ManipulationOrigin));
        }

        public static IObservable<int> ObserveTouchCountDelta
            (this UIElement target)
        {
            return Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.TouchEnter += h,
                h => target.TouchEnter -= h)
                .Select(_ => 1)
                .Merge(Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.TouchLeave += h,
                h => target.TouchLeave -= h)
                .Select(_ => -1));
        }
    }
}
