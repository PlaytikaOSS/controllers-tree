using System;
using System.Text.RegularExpressions;
using System.Threading;
using Playtika.Controllers;
using NUnit.Framework;
using Playtika.Controllers.Substitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnitTests.Controllers
{
    [TestFixture]
    public class ControllersBaseTests
    {
        private IControllerFactory _controllerFactory;
        private TestControllersActionModel _testControllersActionModel;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken CancellationToken => _cancellationTokenSource.Token;

        [SetUp]
        public void SetUp()
        {
            var substituteControllerFactory = new SubstituteControllerFactory();
            _testControllersActionModel = new TestControllersActionModel();
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerBase(
                    substituteControllerFactory,
                    _testControllersActionModel));

            _cancellationTokenSource = new CancellationTokenSource();
            _controllerFactory = substituteControllerFactory;
        }

        [TearDown]
        public void TearDown()
        {
            _controllerFactory = null;
            _testControllersActionModel = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _controllerFactory = null;
            _cancellationTokenSource = null;
        }

        [Test]
        public void ControllerBase_NoFails_CorrectArguments()
        {
            // Arrange
            var startTriggered = false;
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;

            Assert.DoesNotThrow(
                () =>
                {
                    // Act
                    TestRootControllerLauncher.Launch<ActionModelTestControllerBase, TestControllerArgs>(
                        args,
                        _controllerFactory, CancellationToken);

                    _cancellationTokenSource.Cancel();

                    // Assert
                    Assert.AreEqual(args.InputString, _testControllersActionModel.Args, "Args are not equal");
                    Assert.True(startTriggered, "OnStart is not triggered");
                    Assert.True(stopTriggered, "OnStop is not triggered");
                    Assert.True(disposeTriggered, "Dispose is not triggered");
                });
        }

        [Test]
        public void ControllerBase_OnStartThrowsException_StopAndDisposeAreTriggered()
        {
            // Arrange
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => throw new TestControllersException(TestControllersMethodsNamesConsts.OnStartMethodName);
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;

            Assert.Throws<TestControllersException>(
                () =>
                {
                    // Act
                    TestRootControllerLauncher.Launch<ActionModelTestControllerBase, TestControllerArgs>(
                        args,
                        _controllerFactory, CancellationToken);

                    _cancellationTokenSource.Cancel();
                });

            // Assert
            Assert.True(stopTriggered, "OnStop is not triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
        }

        [Test]
        public void ControllerBase_OnStopThrowsException_OnStartAndOnDisposeAreTriggered()
        {
            var startTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.StopTriggered += () => throw new TestControllersException(TestControllersMethodsNamesConsts.OnStopMethodName);
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;

            Assert.Throws<AggregateException>(
                () =>
                {
                    // Act
                    TestRootControllerLauncher.Launch<ActionModelTestControllerBase, TestControllerArgs>(
                        args,
                        _controllerFactory, CancellationToken);

                    _cancellationTokenSource.Cancel();
                });

            // Assert
            Assert.True(startTriggered, "OnStart is not triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
        }

        [Test]
        public void ControllerBase_OnDisposeThrowsException_OnStartAndOnStopAreTriggered()
        {
            // Arrange
            var startTriggered = false;
            var stopTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => throw new TestControllersException(TestControllersMethodsNamesConsts.DisposeMethodName);

            LogAssert.Expect(LogType.Exception, new Regex("TestControllersException"));
            
            Assert.Throws<AggregateException>(
                () =>
                {
                    // Act
                    TestRootControllerLauncher.Launch<ActionModelTestControllerBase, TestControllerArgs>(
                        args,
                        _controllerFactory, CancellationToken);

                    _cancellationTokenSource.Cancel();
                });

            // Assert
            Assert.True(startTriggered, "OnStart is not triggered");
            Assert.True(stopTriggered, "OnStop has been triggered");
        }

        [Test]
        public void ControllerBase_OnStartTokenCancelled_OnStopAndOnDisposeAreTriggered()
        {
            // Arrange
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => _cancellationTokenSource.Cancel();
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;

            Assert.DoesNotThrow(
                () =>
                {
                    // Act
                    TestRootControllerLauncher.Launch<ActionModelTestControllerBase, TestControllerArgs>(
                        args,
                        _controllerFactory, CancellationToken);

                    // Assert
                    Assert.True(stopTriggered, "OnStop is not triggered");
                    Assert.True(disposeTriggered, "Dispose is not triggered");
                    Assert.True(CancellationToken.IsCancellationRequested, "Cancellation token should be cancelled");
                });
        }
    }
}