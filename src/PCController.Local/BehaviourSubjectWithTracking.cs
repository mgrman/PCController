using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace PCController.Local
{
    internal class BehaviourSubjectWithTracking<T> : SubjectBase<T>
    {
        public readonly BehaviorSubject<bool> _onSubscibersChanged = new BehaviorSubject<bool>(false);

        private readonly BehaviorSubject<T> _behaviorSubject;

        private bool _previousHasObservers;

        public BehaviourSubjectWithTracking(T defaultValue)
        {
            _behaviorSubject = new BehaviorSubject<T>(defaultValue);
        }

        public IObservable<bool> OnSubscibersChanged => _onSubscibersChanged;

        public override bool IsDisposed => _behaviorSubject.IsDisposed;
        public override bool HasObservers => _behaviorSubject.HasObservers;

        public override void Dispose()
        {
            _behaviorSubject.Dispose();
        }

        public override void OnCompleted()
        {
            _behaviorSubject.OnCompleted();
        }

        public override void OnError(Exception error)
        {
            _behaviorSubject.OnError(error);
        }

        public override void OnNext(T value)
        {
            _behaviorSubject.OnNext(value);
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            var subscription = _behaviorSubject.Subscribe(observer);
            UpdateOnSubscibersChanged();
            return Disposable.Create(() =>
            {
                subscription.Dispose();
                UpdateOnSubscibersChanged();
            });
        }

        private void UpdateOnSubscibersChanged()
        {
            if (_previousHasObservers == HasObservers)
            {
                return;
            }
            _previousHasObservers = HasObservers;
            _onSubscibersChanged.OnNext(HasObservers);
        }
    }
}
