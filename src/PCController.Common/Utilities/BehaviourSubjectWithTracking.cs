using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace PCController.Local
{
    public class BehaviourSubjectWithTracking<T> : SubjectBase<T>
    {
        private readonly BehaviorSubject<T> behaviorSubject;
        private readonly BehaviorSubject<bool> onSubscibersChanged = new BehaviorSubject<bool>(false);

        private bool previousHasObservers;

        private volatile int subscribers;

        public BehaviourSubjectWithTracking(T defaultValue)
        {
            this.behaviorSubject = new BehaviorSubject<T>(defaultValue);
        }

        public IObservable<bool> OnSubscibersChanged => this.onSubscibersChanged;

        public T Value => this.behaviorSubject.Value;

        public override bool HasObservers => this.subscribers > 0;

        public override bool IsDisposed => this.behaviorSubject.IsDisposed;

        public override void Dispose()
        {
            this.behaviorSubject.Dispose();
        }

        public override void OnCompleted()
        {
            this.behaviorSubject.OnCompleted();
        }

        public override void OnError(Exception error)
        {
            this.behaviorSubject.OnError(error);
        }

        public override void OnNext(T value)
        {
            this.behaviorSubject.OnNext(value);
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            this.subscribers++;
            var subscription = this.behaviorSubject.Subscribe(observer);
            this.UpdateOnSubscibersChanged();
            return Disposable.Create(() =>
            {
                this.subscribers--;
                subscription.Dispose();
                this.UpdateOnSubscibersChanged();
            });
        }

        private void UpdateOnSubscibersChanged()
        {
            if (this.previousHasObservers == this.HasObservers)
            {
                return;
            }

            this.previousHasObservers = this.HasObservers;
            this.onSubscibersChanged.OnNext(this.HasObservers);
        }
    }
}
