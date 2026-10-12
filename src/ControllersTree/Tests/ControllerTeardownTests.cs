using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Playtika.Controllers;
using Playtika.Controllers.Substitute;

namespace UnitTests.Controllers
{
    [TestFixture]
    public class ControllerTeardownTests
    {
        private const string OnStartFailed = "OnStart failed";
        private const string OnStopFailed = "OnStop failed";
        private const string DisposeFailed = "Dispose failed";
        private const string FlowFailed = "Flow failed";

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Creates the token source that bounds the lifetime of the test root controller.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Cancels the token source, so the test root controller is stopped after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// A child that throws in OnStart and then in OnStop during cleanup must not stay in the parent's children.
        /// </summary>
        [Test]
        public void Execute_OnStartAndOnStopThrow_FailedChildLeavesTree()
        {
            var root = Launch(factory => new ThrowingController(factory, throwOnStart: true, throwOnStop: true, throwOnDispose: false));

            Assert.Throws<AggregateException>(() => root.Execute<ThrowingController>());

            Assert.AreEqual(nameof(TestRootController), root.DumpControllersTree());
        }

        /// <summary>
        /// When OnStart throws and a disposable throws during cleanup, the caller gets both exceptions, OnStart first.
        /// </summary>
        [Test]
        public void Execute_OnStartAndDisposableThrow_OnStartExceptionIsReported()
        {
            var root = Launch(factory => new ThrowingController(factory, throwOnStart: true, throwOnStop: false, throwOnDispose: true));

            var exception = Assert.Throws<AggregateException>(() => root.Execute<ThrowingController>());

            CollectionAssert.AreEqual(new[] { OnStartFailed, DisposeFailed }, Messages(exception));
        }

        /// <summary>
        /// When OnStop throws and a disposable throws while the tree is disposed, the caller gets both exceptions, OnStop first.
        /// </summary>
        [Test]
        public void Dispose_OnStopAndDisposableThrow_OnStopExceptionIsReported()
        {
            var root = Launch(factory => new ThrowingController(factory, throwOnStart: false, throwOnStop: true, throwOnDispose: true));
            root.Execute<ThrowingController>();

            var exception = Assert.Throws<AggregateException>(() => ((IDisposable)root).Dispose());

            CollectionAssert.AreEqual(new[] { OnStopFailed, DisposeFailed }, Messages(exception));
        }

        /// <summary>
        /// When OnFlowAsync throws and a disposable throws during cleanup, the caller gets both exceptions, the flow one first.
        /// </summary>
        [Test]
        public async Task ExecuteAndWaitResultAsync_FlowAndDisposableThrow_FlowExceptionIsReported()
        {
            var root = Launch(factory => new ThrowingControllerWithResult(factory));
            AggregateException exception = null;

            try
            {
                await root.ExecuteAndWaitResultAsync<ThrowingControllerWithResult>(_cancellationTokenSource.Token);
            }
            catch (AggregateException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception, "AggregateException expected");
            CollectionAssert.AreEqual(new[] { FlowFailed, DisposeFailed }, Messages(exception));
        }

        /// <summary>
        /// Launches a test root controller whose factory returns the controller created by <paramref name="createController"/>.
        /// </summary>
        /// <typeparam name="T">The type of the controller the factory returns.</typeparam>
        /// <param name="createController">Creates the controller instance from the factory.</param>
        /// <returns>The launched test root controller.</returns>
        private TestRootController Launch<T>(Func<IControllerFactory, T> createController)
            where T : IController
        {
            var factory = new SubstituteControllerFactory();
            factory.AddInstance(createController(factory));
            var root = new TestRootController(factory);
            root.LaunchTree(_cancellationTokenSource.Token);
            return root;
        }

        /// <summary>
        /// Returns the messages of the direct inner exceptions, so nested aggregates show up as a mismatch.
        /// </summary>
        /// <param name="exception">The exception thrown by the controller tree.</param>
        /// <returns>The messages of the inner exceptions in order.</returns>
        private static string[] Messages(AggregateException exception) =>
            exception.InnerExceptions.Select(e => e.Message).ToArray();

        private class ThrowingController : ControllerBase
        {
            private readonly bool _throwOnStart;
            private readonly bool _throwOnStop;
            private readonly bool _throwOnDispose;

            /// <summary>
            /// Creates a controller that throws from the places selected by the flags.
            /// </summary>
            /// <param name="factory">The controller factory.</param>
            /// <param name="throwOnStart">Throw from OnStart.</param>
            /// <param name="throwOnStop">Throw from OnStop.</param>
            /// <param name="throwOnDispose">Add a disposable that throws when the controller is disposed.</param>
            public ThrowingController(IControllerFactory factory, bool throwOnStart, bool throwOnStop, bool throwOnDispose)
                : base(factory)
            {
                _throwOnStart = throwOnStart;
                _throwOnStop = throwOnStop;
                _throwOnDispose = throwOnDispose;
            }

            /// <summary>
            /// Adds the throwing disposable and throws when the controller is configured to fail in OnStart.
            /// </summary>
            protected override void OnStart()
            {
                if (_throwOnDispose)
                {
                    AddDisposable(new DisposableToken(() => throw new InvalidOperationException(DisposeFailed)));
                }

                if (_throwOnStart)
                {
                    throw new InvalidOperationException(OnStartFailed);
                }
            }

            /// <summary>
            /// Throws when the controller is configured to fail in OnStop.
            /// </summary>
            protected override void OnStop()
            {
                if (_throwOnStop)
                {
                    throw new InvalidOperationException(OnStopFailed);
                }
            }
        }

        private class ThrowingControllerWithResult : ControllerWithResultBase
        {
            /// <summary>
            /// Creates a controller whose flow and disposable both throw.
            /// </summary>
            /// <param name="factory">The controller factory.</param>
            public ThrowingControllerWithResult(IControllerFactory factory)
                : base(factory)
            {
            }

            /// <summary>
            /// Adds a disposable that throws when the controller is disposed.
            /// </summary>
            protected override void OnStart() =>
                AddDisposable(new DisposableToken(() => throw new InvalidOperationException(DisposeFailed)));

            /// <summary>
            /// Throws to fail the flow.
            /// </summary>
            /// <param name="cancellationToken">The controller cancellation token.</param>
            /// <returns>Never returns.</returns>
            protected override UniTask OnFlowAsync(CancellationToken cancellationToken) =>
                throw new InvalidOperationException(FlowFailed);
        }
    }
}
