using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Playtika.Controllers
{
    public abstract partial class ControllerBase
    {
        /// <summary>
        /// Executes a controller of type T.
        /// This method creates an instance of the controller and starts it.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IController.</typeparam>
        /// <param name="ct">A cancellation token that can be used to stop the controller.</param>
        protected void Execute<T>(CancellationToken ct = default)
            where T : class, IController
        {
            ThrowIfControllerWithResult<T>();
            ExecuteInternal<T>(null, ct);
        }

        /// <summary>
        /// Executes a controller of type T that requires an argument of type TArg.
        /// This method creates an instance of the controller, sets the argument, and starts the controller.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IController and IController&lt;TArg&gt;.</typeparam>
        /// <typeparam name="TArg">The type of the argument required by the controller.</typeparam>
        /// <param name="arg">The argument to pass to the controller.</param>
        /// <param name="ct">A cancellation token that can be used to stop the controller.</param>
        protected void Execute<T, TArg>(TArg arg, CancellationToken ct = default)
            where T : class, IController, IController<TArg>
        {
            ThrowIfControllerWithResult<T>();
            ExecuteInternal<T, TArg>(arg, null, ct);
        }

        /// <summary>
        /// Executes a controller of type T using a specified controller factory.
        /// This method creates an instance of the controller and starts it.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IController.</typeparam>
        /// <param name="factory">The factory to use when creating the controller.</param>
        /// <param name="ct">A cancellation token that can be used to stop the controller.</param>
        protected void Execute<T>(IControllerFactory factory, CancellationToken ct = default)
            where T : class, IController
        {
            ThrowIfControllerWithResult<T>();
            ExecuteInternal<T>(factory, ct);
        }

        /// <summary>
        /// Executes a controller of type T that requires an argument of type TArg using a specified controller factory.
        /// This method creates an instance of the controller, sets the argument, and starts the controller.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IController and IController&lt;TArg&gt;.</typeparam>
        /// <typeparam name="TArg">The type of the argument required by the controller.</typeparam>
        /// <param name="arg">The argument to pass to the controller.</param>
        /// <param name="factory">The factory to use when creating the controller.</param>
        /// <param name="ct">A cancellation token that can be used to stop the controller.</param>
        protected void Execute<T, TArg>(TArg arg, IControllerFactory factory, CancellationToken ct = default)
            where T : class, IController, IController<TArg>
        {
            ThrowIfControllerWithResult<T>();
            ExecuteInternal<T, TArg>(arg, factory, ct);
        }

        private IController ExecuteInternal<T>(
            IControllerFactory factory,
            CancellationToken cancellationToken)
            where T : class, IController
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfControllerHasIncorrectState();
            var controller = Create<T>(factory);
            Start(controller, cancellationToken);
            return controller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IController ExecuteInternal<T, TArg>(
            TArg arg,
            IControllerFactory factory,
            CancellationToken cancellationToken)
            where T : class, IController, IController<TArg>
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfControllerHasIncorrectState();
            var controller = Create<T>(factory);
            ((IController<TArg>)controller).SetArgInternal(arg);
            Start(controller, cancellationToken);
            return controller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IController Create<T>(IControllerFactory factory)
            where T : class, IController
        {
            factory ??= _controllerFactory;
            var controller = factory.Create<T>();
            return controller;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Start(IController controller, CancellationToken token)
        {
            controller.Initialize(token, CancellationToken);

            AddChild(controller);

            try
            {
                controller.Start();
            }
            catch (Exception exception)
            {
                using (controller)
                {
                    controller.Stop(exception);
                    RemoveChild(controller);
                }

                throw;
            }
        }

        [Conditional("UNITY_EDITOR")]
        private static void ThrowIfControllerWithResult<T>()
        {
            var controllerType = typeof(T);
            var controllerWithResultBaseInterface = typeof(IControllerWithResult<>);
            var interfaces = controllerType.GetInterfaces();
            if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == controllerWithResultBaseInterface))
            {
                throw new Exception(
                    $"{controllerType.Name}: ControllerWithResult cannot be started with {nameof(Execute)}");
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void ThrowIfControllerHasIncorrectState()
        {
            if (_state != ControllerState.Running)
            {
                throw new InvalidOperationException($"{Name} Can't Execute from {_state} state.");
            }
        }
    }
}