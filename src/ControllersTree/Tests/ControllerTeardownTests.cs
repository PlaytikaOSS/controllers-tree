using System;
using System.Threading;
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
            var root = Launch(factory => new ThrowingController(factory, throwOnStart: true, throwOnStop: true));

            Assert.Throws<AggregateException>(() => root.Execute<ThrowingController>());

            Assert.AreEqual(nameof(TestRootController), root.DumpControllersTree());
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

        private class ThrowingController : ControllerBase
        {
            private readonly bool _throwOnStart;
            private readonly bool _throwOnStop;

            /// <summary>
            /// Creates a controller that throws from the lifecycle methods selected by the flags.
            /// </summary>
            /// <param name="factory">The controller factory.</param>
            /// <param name="throwOnStart">Throw from OnStart.</param>
            /// <param name="throwOnStop">Throw from OnStop.</param>
            public ThrowingController(IControllerFactory factory, bool throwOnStart, bool throwOnStop)
                : base(factory)
            {
                _throwOnStart = throwOnStart;
                _throwOnStop = throwOnStop;
            }

            /// <summary>
            /// Throws when the controller is configured to fail in OnStart.
            /// </summary>
            protected override void OnStart()
            {
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
    }
}
