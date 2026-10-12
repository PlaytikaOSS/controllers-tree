using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Playtika.Controllers
{
    public partial class ControllerBase
    {
        /// <summary>
        /// Asynchronously executes a controller of type T that implements IControllerWithResult using a specified controller factory.
        /// This method creates an instance of the controller using the provided factory and starts it.
        /// It then waits for the controller to complete its execution and return a result.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IControllerWithResult.</typeparam>
        /// <param name="factory">The factory to use when creating the controller.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        protected async UniTask ExecuteAndWaitResultAsync<T>(
            [NotNull] IControllerFactory factory,
            CancellationToken cancellationToken)
            where T : class, IControllerWithResult<EmptyControllerResult>, IController<EmptyControllerArg>
        {
            await ExecuteAndWaitResultAsyncInternal<T, EmptyControllerResult>(
                factory,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a controller of type T that implements IControllerWithResult.
        /// This method creates an instance of the controller and starts it.
        /// It then waits for the controller to complete its execution and return a result.
        /// The controller instance is created using the default controller factory.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IControllerWithResult.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        protected async UniTask ExecuteAndWaitResultAsync<T>(
            CancellationToken cancellationToken)
            where T : class, IControllerWithResult<EmptyControllerResult>, IController<EmptyControllerArg>
        {
            await ExecuteAndWaitResultAsyncInternal<T, EmptyControllerResult>(
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a controller of type T that implements IControllerWithResult&lt;TResult&gt;
        /// This method creates an instance of the controller and starts it.
        /// It then waits for the controller to complete its execution and return a result.
        /// The controller instance is created using the default controller factory.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IControllerWithResult&lt;TResult&gt;.</typeparam>
        /// <typeparam name="TResult">The type of the result expected from the controller's execution.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        protected UniTask<TResult> ExecuteAndWaitResultAsync<T, TResult>(
            CancellationToken cancellationToken)
            where T : class, IControllerWithResult<TResult>, IController<EmptyControllerArg>
        {
            return ExecuteAndWaitResultAsyncInternal<T, TResult>(
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a controller of type T that implements IControllerWithResult&lt;TResult&gt; using a specified controller factory.
        /// This method creates an instance of the controller using the provided factory and starts it.
        /// It then waits for the controller to complete its execution and return a result.
        /// The controller instance is created using the default controller factory.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IControllerWithResult&lt;TResult&gt;.</typeparam>
        /// <typeparam name="TResult">The type of the result expected from the controller's execution.</typeparam>
        /// <param name="factory">The factory to use when creating the controller.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        protected UniTask<TResult> ExecuteAndWaitResultAsync<T, TResult>(
            IControllerFactory factory,
            CancellationToken cancellationToken)
            where T : class, IControllerWithResult<TResult>, IController<EmptyControllerArg>
        {
            return ExecuteAndWaitResultAsyncInternal<T, TResult>(
                factory,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a controller of type T that implements IControllerWithResult&lt;EmptyControllerResult&gt; and IController&lt;TArg&gt; using a specified controller factory and an argument.
        /// This method creates an instance of the controller using the provided factory, sets the argument, and starts it.
        /// It then waits for the controller to complete its execution and return a result.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IControllerWithResult&lt;EmptyControllerResult&gt; and IController&lt;TArg&gt;.</typeparam>
        /// <typeparam name="TArg">The type of the argument required by the controller.</typeparam>
        /// <param name="arg">The argument to pass to the controller.</param>
        /// <param name="factory">The factory to use when creating the controller.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        protected async UniTask ExecuteAndWaitResultAsync<T, TArg>(
            [NotNull] TArg arg,
            [NotNull] IControllerFactory factory,
            CancellationToken cancellationToken)
            where T : class, IControllerWithResult<EmptyControllerResult>, IController<TArg>
        {
            await ExecuteAndWaitResultAsyncInternal<T, TArg, EmptyControllerResult>(
                arg,
                factory,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a controller of type T that implements IControllerWithResult&lt;EmptyControllerResult&gt; and IController&lt;TArg&gt;.
        /// This method creates an instance of the controller, sets the argument, and starts it.
        /// It then waits for the controller to complete its execution and return a result.
        /// The controller instance is created using the default controller factory.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IControllerWithResult&lt;EmptyControllerResult&gt; and IController&lt;TArg&gt;.</typeparam>
        /// <typeparam name="TArg">The type of the argument required by the controller.</typeparam>
        /// <param name="arg">The argument to pass to the controller.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        protected async UniTask ExecuteAndWaitResultAsync<T, TArg>(
            [NotNull] TArg arg,
            CancellationToken cancellationToken)
            where T : class, IControllerWithResult<EmptyControllerResult>, IController<TArg>
        {
            await ExecuteAndWaitResultAsyncInternal<T, TArg, EmptyControllerResult>(
                arg,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a controller of type T that implements IControllerWithResult&lt;TResult&gt; and IController&lt;TArg&gt; using a specified controller factory and an argument.
        /// This method creates an instance of the controller using the provided factory, sets the argument, and starts it.
        /// It then waits for the controller to complete its execution and return a result.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IControllerWithResult&lt;TResult&gt; and IController&lt;TArg&gt;.</typeparam>
        /// <typeparam name="TArg">The type of the argument required by the controller.</typeparam>
        /// <typeparam name="TResult">The type of the result expected from the controller's execution.</typeparam>
        /// <param name="arg">The argument to pass to the controller.</param>
        /// <param name="factory">The factory to use when creating the controller.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        protected UniTask<TResult> ExecuteAndWaitResultAsync<T, TArg, TResult>(
            [NotNull] TArg arg,
            [NotNull] IControllerFactory factory,
            CancellationToken cancellationToken)
            where T : class, IControllerWithResult<TResult>, IController<TArg>
        {
            return ExecuteAndWaitResultAsyncInternal<T, TArg, TResult>(
                arg,
                factory,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes a controller of type T that implements IControllerWithResult&lt;TResult&gt; and IController&lt;TArg&gt; using an argument.
        /// This method creates an instance of the controller, sets the argument, and starts it.
        /// It then waits for the controller to complete its execution and return a result.
        /// The controller instance is created using the default controller factory.
        /// </summary>
        /// <typeparam name="T">The type of the controller to execute. Must implement IControllerWithResult&lt;TResult&gt; and IController&lt;TArg&gt;.</typeparam>
        /// <typeparam name="TArg">The type of the argument required by the controller.</typeparam>
        /// <typeparam name="TResult">The type of the result expected from the controller's execution.</typeparam>
        /// <param name="arg">The argument to pass to the controller.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        protected UniTask<TResult> ExecuteAndWaitResultAsync<T, TArg, TResult>(
            [NotNull] TArg arg,
            CancellationToken cancellationToken)
            where T : class, IControllerWithResult<TResult>, IController<TArg>
        {
            return ExecuteAndWaitResultAsyncInternal<T, TArg, TResult>(
                arg,
                cancellationToken: cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask<TResult> ExecuteAndWaitResultAsyncInternal<T, TResult>(
            IControllerFactory factory = default,
            CancellationToken cancellationToken = default)
            where T : class, IControllerWithResult<TResult>
        {
            using var controller = ExecuteInternal<T>(factory, cancellationToken);
            return await WaitResultAsync<TResult>((IControllerWithResult<TResult>) controller, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask<TResult> ExecuteAndWaitResultAsyncInternal<T, TArg, TResult>(
            TArg arg = default,
            IControllerFactory factory = default,
            CancellationToken cancellationToken = default)
            where T : class, IControllerWithResult<TResult>, IController<TArg>
        {
            using var controller = ExecuteInternal<T, TArg>(arg, factory, cancellationToken);
            return await WaitResultAsync<TResult>((IControllerWithResult<TResult>) controller, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask<TResult> WaitResultAsync<TResult>(
            IControllerWithResult<TResult> controller,
            CancellationToken cancellationToken)
        {
            try
            {
                controller.FlowAsync(controller.CancellationToken).Forget(controller.FailInternal);
                return await controller.GetResult(cancellationToken);
            }
            catch (Exception exception)
            {
                DisposeAfterFailure(controller, exception);
                throw;
            }
            finally
            {
                RemoveChild(controller);
            }
        }
    }
}
