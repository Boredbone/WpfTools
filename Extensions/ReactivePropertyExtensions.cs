using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Internals;

namespace WpfTools.Extensions
{
    public static class ReactivePropertyExtensions
    {
        public static ReactivePropertySlim<TProperty> ToReactivePropertySlimAsSynchronized<TSubject, TProperty>(
            this TSubject subject, Expression<Func<TSubject, TProperty>> propertySelector, CompositeDisposable disposables,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe)
            where TSubject : INotifyPropertyChanged
        {
            var setter = AccessorCache<TSubject>.LookupSet(propertySelector, out _);

            var rp = new ReactivePropertySlim<TProperty>(default, mode, null);
            subject.ObserveProperty(propertySelector, isPushCurrentValueAtFirst: true)
                .Subscribe(x => rp.Value = x).AddTo(disposables);
            rp.Subscribe(x => setter(subject, x));
            return rp;
        }

        public static ReactivePropertySlim<TResult> ToReactivePropertySlimAsSynchronized<TSubject, TProperty, TResult>(
            this TSubject subject, Expression<Func<TSubject, TProperty>> propertySelector, CompositeDisposable disposables,
            Func<TProperty, TResult> convert,
            Func<TResult, TProperty> convertBack,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe)
            where TSubject : INotifyPropertyChanged =>
            ToReactivePropertySlimAsSynchronized(subject, propertySelector, disposables,
                ox => ox.Select(convert),
                ox => ox.Select(convertBack),
                mode);

        public static ReactivePropertySlim<TResult> ToReactivePropertySlimAsSynchronized<TSubject, TProperty, TResult>(
            this TSubject subject, Expression<Func<TSubject, TProperty>> propertySelector, CompositeDisposable disposables,
            Func<IObservable<TProperty>, IObservable<TResult>> convert,
            Func<IObservable<TResult>, IObservable<TProperty>> convertBack,
            ReactivePropertyMode mode = ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe)
            where TSubject : INotifyPropertyChanged
        {
            var setter = AccessorCache<TSubject>.LookupSet(propertySelector, out _);

            var rp = new ReactivePropertySlim<TResult>(default, mode, null);
            convert(subject.ObserveProperty(propertySelector, isPushCurrentValueAtFirst: true))
                 .Subscribe(x => rp.Value = x).AddTo(disposables);
            convertBack(rp).Subscribe(x => setter(subject, x));
            return rp;
        }

    }
}