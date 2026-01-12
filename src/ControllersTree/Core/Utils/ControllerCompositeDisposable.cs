using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Playtika.Controllers
{
    internal sealed class ControllerDisposeAggregateException : AggregateException
    {
        public ControllerDisposeAggregateException(IEnumerable<Exception> innerExceptions)
            : base(innerExceptions)
        {
        }
    }

    /// <summary>
    /// Component of controller that keeps related disposable object that must be disposed together with the running controller.
    /// </summary>
    public class ControllerCompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables = ListPool<IDisposable>.Get();
        private bool _disposed = false;

        /// <summary>
        /// Adds a disposable object to the internal list of disposables.
        /// </summary>
        /// <param name="disposable">The disposable object to add to the list.</param>
        public void Add(IDisposable disposable)
        {
            if (_disposed)
            {
                disposable?.Dispose();
            }
            else if (disposable != null)
            {
                _disposables.Add(disposable);
            }
        }

        /// <summary>
        /// Adds a collection of disposable objects to the internal list of disposables.
        /// </summary>
        /// <param name="collection">The collection of disposable objects to add to the list.</param>
        public void AddRange(IEnumerable<IDisposable> collection)
        {
            if (collection == null)
            {
                return;
            }

            if (_disposed)
            {
                using var pooledObject = ListPool<IDisposable>.Get(out var disposablesList);
                disposablesList.AddRange(collection.Where(c => c != null));
                DisposeMany(disposablesList);
            }
            else
            {
                _disposables.AddRange(collection);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                DisposeMany(_disposables);
            }
            finally
            {
                ListPool<IDisposable>.Release(_disposables);
            }
        }

        private static void DisposeMany(IReadOnlyCollection<IDisposable> disposables)
        {
            if (disposables == null || disposables.Count == 0)
            {
                return;
            }

            using var pooledObject = ListPool<Exception>.Get(out var exceptionList);
            foreach (var disposable in disposables)
            {
                try
                {
                    disposable?.Dispose();
                }
                catch (AggregateException e)
                {
                    exceptionList.AddRange(e.InnerExceptions);
                    if (e is not ControllerDisposeAggregateException)
                    {
                        Debug.LogException(e);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    exceptionList.Add(e);
                }
            }

            if (exceptionList.Count > 0)
            {
                throw new ControllerDisposeAggregateException(exceptionList.ToList());
            }
        }
    }
}