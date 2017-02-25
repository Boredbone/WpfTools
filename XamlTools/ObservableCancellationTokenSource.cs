using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Boredbone.Utility.Notification;
using Reactive.Bindings.Extensions;

namespace Boredbone.XamlTools
{
    public class ObservableCancellationTokenSource : DisposableBase
    {
        public CancellationTokenSource CancellationTokenSource { get; }
        public CancellationToken Token => this.CancellationTokenSource.Token;

        private Subject<Unit> CanceledSubject { get; }
        public IObservable<Unit> Canceled => this.CanceledSubject.AsObservable();

        public ObservableCancellationTokenSource()
        {
            this.CancellationTokenSource = new CancellationTokenSource().AddTo(this.Disposables);
            this.CanceledSubject = new Subject<Unit>().AddTo(this.Disposables);
        }


        public void Cancel(bool throwOnFirstException)
        {
            this.CancellationTokenSource.Cancel(throwOnFirstException);
            this.CanceledSubject.OnCompleted();
        }
        public void Cancel()
        {
            this.CancellationTokenSource.Cancel();
            this.CanceledSubject.OnCompleted();
        }
    }
}
