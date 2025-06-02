using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine.Pool;

namespace Playtika.Controllers
{
    public abstract partial class ControllerBase : IController, IDisposable
    {
        private readonly List<IController> _childControllers;
        private readonly ControllerCompositeDisposable _compositeDisposables = new ();
        private readonly IControllerFactory _controllerFactory;
        private CancellationToken _lifetimeToken;
        private CancellationTokenSource _lifetimeTokenSource;
        private ControllerState _state;

        public string Name => GetType().Name;
        protected CancellationToken CancellationToken => _lifetimeToken;

        CancellationToken IController.CancellationToken => _lifetimeToken;

        partial void ProfileOnCreated();
        partial void ProfileOnStart();
        partial void ProfileOnStop();

        protected ControllerBase(IControllerFactory controllerFactory)
        {
            _controllerFactory = controllerFactory;
            _childControllers = ListPool<IController>.Get();
            _state = ControllerState.Created;
        }

        public void AddDisposable(IDisposable disposable)
        {
            _compositeDisposables.Add(disposable);
        }

        public void AddDisposables(IEnumerable<IDisposable> collection)
        {
            _compositeDisposables.AddRange(collection);
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        void IController.Initialize(
            CancellationToken externalCancellationToken,
            CancellationToken parentCancellationToken)
        {
            switch (_state)
            {
                case ControllerState.Created:
                {
                    _lifetimeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken, parentCancellationToken);
                    AddDisposable(_lifetimeTokenSource);
                    _lifetimeToken = _lifetimeTokenSource.Token;
                    _lifetimeToken.ThrowIfCancellationRequested();
                    _state = ControllerState.Initialized;
                    ProfileOnCreated();
                    break;
                }
                default:
                    throw new InvalidOperationException($"{Name} Initialized called from non-'Created' state. Current state: {_state}");
            }
        }

        void IController.Start()
        {
            switch (_state)
            {
                case ControllerState.Initialized:
                {
                    _state = ControllerState.Running;
                    OnStart();
                    ProfileOnStart();
                    break;
                }
                default:
                    throw new InvalidOperationException($"{Name} Controller should be initialized before adding. Current state: {_state}");
            }
        }

        void IDisposable.Dispose()
        {
            try
            {
                ((IController)this).Stop();
            }
            finally
            {
                DisposeInternal();
            }
        }

        void IController.Stop()
        {
            switch (_state)
            {
                case ControllerState.Created:
                case ControllerState.Initialized:
                case ControllerState.Running:
                    StopChildrenAndSelf();
                    ProfileOnStop();
                    break;
                case ControllerState.Stopped:
                case ControllerState.Disposed:
                    break;
                default:
                    throw new InvalidOperationException($"{Name} Controller can not be stopped from current state: {_state}");
            }
        }

        void IController.Stop(Exception rootCauseException)
        {
            try
            {
                ((IController)this).Stop();
            }
            catch (Exception exception)
            {
                throw new AggregateException(rootCauseException, exception);
            }
        }

        private void DisposeInternal()
        {
            switch (_state)
            {
                case ControllerState.Initialized:
                case ControllerState.Running:
                case ControllerState.Stopped:
                {
                    _lifetimeTokenSource?.Cancel();
                    _compositeDisposables.Dispose();
                    ListPool<IController>.Release(_childControllers);
                    _state = ControllerState.Disposed;
                    break;
                }

                case ControllerState.Disposed:
                    break;

                default:
                    throw new InvalidOperationException($"{Name} Controller can not be disposed from current state: {_state}");
            }
        }

        private void AddChild(IController controller)
        {
            _childControllers.Add(controller);
        }

        private void RemoveChild(IController controller)
        {
            _childControllers.Remove(controller);
        }

        private void StopChildrenAndSelf()
        {
            using var poolToken = ListPool<Exception>.Get(out var exceptions);
            var childControllers = _childControllers.ToArray();
            foreach (var child in childControllers)
            {
                try
                {
                    child.Stop();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            AddDisposables(_childControllers);
            _state = ControllerState.Stopped;

            try
            {
                OnStop();
            }
            catch (Exception exception)
            {
                if (exceptions.Any())
                {
                    exceptions.Add(exception);
                }
                else
                {
                    throw;
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        public override string ToString()
        {
            return _state switch
            {
                ControllerState.Running => Name,
                _ => $"{Name} : {_state}"
            };
        }
    }
}
