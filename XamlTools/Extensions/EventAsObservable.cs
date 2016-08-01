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


        ///// <summary>
        ///// 辞書にIDisposableを追加し、同じキーが存在している場合は元のアイテムをDispose
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <typeparam name="TKey"></typeparam>
        ///// <param name="disposable"></param>
        ///// <param name="dictionary"></param>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public static T AddTo<T, TKey>
        //    (this T disposable, IDictionary<TKey, IDisposable> dictionary, TKey key) where T : IDisposable
        //{
        //    IDisposable result;
        //    if (dictionary.TryGetValue(key, out result))
        //    {
        //        result?.Dispose();
        //        dictionary.Remove(key);
        //    }

        //    dictionary.Add(key, disposable);

        //    return disposable;
        //}

        ///// <summary>
        ///// 一定時間ごとに一つだけ通過
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="source"></param>
        ///// <param name="interval"></param>
        ///// <returns></returns>
        //public static IObservable<T> DownSample<T>(this IObservable<T> source, TimeSpan interval)
        //{
        //    return Observable.Create<T>(o =>
        //    {
        //        var subscriptions = new CompositeDisposable();
        //        var acceepted = true;

        //        var pub = source.Where(x => acceepted);

        //        pub.Subscribe(o).AddTo(subscriptions);

        //        pub.Do(x => acceepted = false)
        //        .Delay(interval).Subscribe(x => acceepted = true)
        //        .AddTo(subscriptions);

        //        return subscriptions;
        //    });
        //}

        ///// <summary>
        ///// Throttleした値と、Throttle後に初めてきた値を流す
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="source"></param>
        ///// <param name="interval"></param>
        ///// <returns></returns>
        //public static IObservable<T> Restrict<T>(this IObservable<T> source, TimeSpan interval)
        //{
        //    var seq = source
        //        .Throttle(interval)
        //        .Select(x => new FlaggedItem<T>(x, true))
        //        .Merge(source.Select(x => new FlaggedItem<T>(x, false)))
        //        .Publish()
        //        .RefCount();

        //    return seq
        //        .Zip(seq.Skip(1), (Z2, Z1) => new { Z2, Z1 })
        //        .Zip(seq.Skip(2), (a, b) => new { Value = b.Value, Z0 = b.Flag, Z1 = a.Z1.Flag, Z2 = a.Z2.Flag })
        //        //.Do(x => Debug.WriteLine($"{x.Value},{x.Z0},{x.Z1},{x.Z2}"))
        //        .Where(x => x.Z1 || (x.Z0 && !x.Z2))
        //        .Select(x => x.Value)
        //        .Merge(source.Take(2));
        //}

        //private class FlaggedItem<T>
        //{
        //    public bool Flag { get; set; }
        //    public T Value { get; }

        //    public FlaggedItem(T value, bool flag)
        //    {
        //        this.Value = value;
        //        this.Flag = flag;
        //    }
        //}

        //public static ReactiveCommand<Tvalue> WithSubscribe<Tvalue>
        //    (this ReactiveCommand<Tvalue> observable, Action<Tvalue> action, ICollection<IDisposable> container)
        //{
        //    observable.Subscribe(action).AddTo(container);
        //    observable.AddTo(container);
        //    return observable;
        //}
        //public static ReactiveCommand WithSubscribe
        //    (this ReactiveCommand observable, Action<object> action, ICollection<IDisposable> container)
        //{
        //    observable.Subscribe(action).AddTo(container);
        //    observable.AddTo(container);
        //    return observable;
        //}
        //public static Subject<Tvalue> WithSubscribe<Tvalue>
        //    (this Subject<Tvalue> observable, Action<Tvalue> action, ICollection<IDisposable> container)
        //{
        //    observable.Subscribe(action).AddTo(container);
        //    observable.AddTo(container);
        //    return observable;
        //}

        //public static bool Toggle(this ReactiveProperty<bool> target)
        //{
        //    var newValue = !target.Value;
        //    target.Value = newValue;
        //    return newValue;
        //}

        //public static IObservable<T> SkipAfter<T, V>(this IObservable<T> source, IObservable<V> trigger, int count)
        //{
        //    return Observable.Create<T>(o =>
        //    {
        //        var subscriptions = new CompositeDisposable();

        //        var acceptCount = 0;

        //        trigger.Subscribe(_ => acceptCount = count).AddTo(subscriptions);

        //        source.Where(_ => --acceptCount < 0).Subscribe(o).AddTo(subscriptions);

        //        return subscriptions;
        //    });
        //}

        //public static IObservable<IList<T>> BufferUntilThrottle<T>
        //    (this IObservable<T> source, double timeMilliseconds)
        //{
        //    return source.BufferUntilThrottle(timeMilliseconds, true);
        //    //return source
        //    //    .Buffer(source.Throttle(TimeSpan.FromMilliseconds(timeMilliseconds)))
        //    //    .Where(x => x.Count > 0);
        //}
        //public static IObservable<IList<T>> BufferUntilThrottle<T>
        //    (this IObservable<T> source, double timeMilliseconds, bool publish)
        //{
        //    var observable = source;

        //    if (publish)
        //    {
        //        observable = source.Publish().RefCount();

        //        //return source
        //        //    .Buffer(source.Throttle(TimeSpan.FromMilliseconds(timeMilliseconds)))
        //        //    .Where(x => x.Count > 0);
        //    }

        //    //var s2 = source.Publish().RefCount();

        //    return observable
        //        .Buffer(observable.Throttle(TimeSpan.FromMilliseconds(timeMilliseconds)))
        //        .Where(x => x.Count > 0);
        //}

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
            // マウスダウン、マウスアップ、マウスムーブのIObservable
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
            /*
            return Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.TouchDown += h,
                h => target.TouchDown -= h)
                .Select(_ => 1)
                .Merge(Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.TouchEnter += h,
                h => target.TouchEnter -= h)
                .Select(_ => 1))
                .Merge(Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.TouchUp += h,
                h => target.TouchUp -= h)
                .Select(_ => -1))
                .Merge(Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                h => (s, e) => h(e),
                h => target.TouchLeave += h,
                h => target.TouchLeave -= h)
                .Select(_ => -1));*/
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
