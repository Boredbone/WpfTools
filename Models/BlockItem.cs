using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using System.Reactive;

namespace WpfTools.Models
{

    internal class BlockItem<T> : IDisposable
    {
        private readonly CompositeDisposable disposables;

        public Block<T> Parent { get; }

        public BlockItem<T> PrevItem
        {
            get { return this._prevItem; }
            set
            {
                if (this._prevItem != value)
                {
                    var old = this._prevItem;
                    this._prevItem = value;

                    if (value != null)
                    {
                        value.NextItem = this;
                        if (old != null)
                        {
                            value.PrevItem = old;
                        }
                    }
                    else
                    {
                        old.NextItem = null;
                    }
                }
            }
        }
        private BlockItem<T> _prevItem;


        public BlockItem<T> NextItem
        {
            get { return this._nextItem; }
            set
            {
                if (this._nextItem != value)
                {
                    var old = this._nextItem;
                    this._nextItem = value;

                    if (value != null)
                    {
                        value.PrevItem = this;
                        if (old != null)
                        {
                            value.NextItem = old;
                        }
                    }
                    else
                    {
                        old.PrevItem = null;
                    }
                }
            }
        }
        private BlockItem<T> _nextItem;


        public T Value
        {
            get { return _fieldValue; }
            set
            {
                _fieldValue = value;
                this.stateChangedSubject.OnNext(Unit.Default);
            }
        }
        private T _fieldValue;


        public bool IsSelected
        {
            get { return _fieldIsSelected; }
            set
            {
                if (_fieldIsSelected != value)
                {
                    _fieldIsSelected = value;
                    this.stateChangedSubject.OnNext(Unit.Default);
                }
            }
        }
        private bool _fieldIsSelected;

        private Subject<Unit> stateChangedSubject;
        public IObservable<Unit> StateChanged => this.stateChangedSubject.AsObservable();



        public int Index { get; }

        public BlockItem(T value, Block<T> parent, int index)
        {
            this.disposables = new CompositeDisposable();

            this.stateChangedSubject = new Subject<Unit>().AddTo(this.disposables);
            this.Value = value;
            this.Parent = parent;
            this.Index = index;
        }


        public void Dispose()
        {
            this.disposables.Dispose();
        }
    }
}
