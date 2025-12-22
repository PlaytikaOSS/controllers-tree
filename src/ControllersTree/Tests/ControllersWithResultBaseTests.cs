using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Playtika.Controllers;
using NUnit.Framework;
using Playtika.Controllers.Substitute;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnitTests.Controllers
{
    public partial class ControllersWithResultBaseTests
    {
        private IControllerFactory _controllerFactory;
        private TestControllersActionModel _testControllersActionModel;
        private TestChildControllersActionModel _testChildControllersActionModel;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken CancellationToken => _cancellationTokenSource.Token;

        [SetUp]
        public void SetUp()
        {
            var substituteControllerFactory = new SubstituteControllerFactory();
            _testControllersActionModel = new TestControllersActionModel();
            _testChildControllersActionModel = new TestChildControllersActionModel();
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResult_CompleteOnFlowAsync(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResult_CompleteAfterOnFlowAsync(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResult_FailOnFlowAsync(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResult_CompleteOnStart(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResultAndChildControllers(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResult_FailInDispose(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResult_FailOnStart(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResult_FailOnStop(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestControllerWithResultAndChildControllers_FailOnFlowAsync(
                    substituteControllerFactory,
                    _testControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestChildControllerWithResult1(
                    substituteControllerFactory,
                    _testChildControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestChildControllerWithResult2(
                    substituteControllerFactory,
                    _testChildControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestChildControllerWithResult3(
                    substituteControllerFactory,
                    _testChildControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestChildControllerWithResult4(
                    substituteControllerFactory,
                    _testChildControllersActionModel));
            substituteControllerFactory.AddInstance(
                new ActionModelTestChildControllerWithResult5(
                    substituteControllerFactory,
                    _testChildControllersActionModel));
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

        [Test]
        public async Task ControllerWithResultBase_Completed_OnStart()
        {
            // Arrange
            var startTriggered = false;
            var flowTriggered = false;
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;

            // Act
            var result = await TestRootControllerLauncher.LaunchAsync
                             <ActionModelTestControllerWithResult_CompleteOnStart, TestControllerArgs, TestEmptyControllerResult>(
                                 args,
                                 _controllerFactory, CancellationToken);

            // Assert
            Assert.True(startTriggered, "OnStart os not triggered");
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(stopTriggered, "OnStop is not triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
            Assert.IsNotNull(result, "Result shouldn't be null");
            var resultCorrect = string.Equals(args.InputString, result.ResultString);
            Assert.True(resultCorrect, "Result is incorrect");
        }

        [Test]
        public async Task ControllerWithResultBase_Exception_OnStart()
        {
            // Arrange
            var flowTriggered = false;
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => throw new TestControllersException(TestControllersMethodsNamesConsts.OnStartMethodName);
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;
            var exceptionThrown = false;

            // Act
            try
            {
                _ = await TestRootControllerLauncher
                        .LaunchAsync<ActionModelTestControllerWithResult_CompleteOnStart, TestControllerArgs, TestEmptyControllerResult>(
                            args, _controllerFactory, CancellationToken);
            }
            catch (TestControllersException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(stopTriggered, "OnStop is not triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
            Assert.IsTrue(exceptionThrown, "TestControllersException expected");
        }

        [Test]
        public async Task ControllerWithResultBase_Exception_OnStop()
        {
            // Arrange
            var startTriggered = false;
            var flowTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => throw new TestControllersException(TestControllersMethodsNamesConsts.OnStopMethodName);
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;
            var exceptionThrown = false;

            // Act
            try
            {
                _ = await TestRootControllerLauncher
                        .LaunchAsync<ActionModelTestControllerWithResult_CompleteOnStart, TestControllerArgs, TestEmptyControllerResult>(
                            args, _controllerFactory, CancellationToken);
            }
            catch (TestControllersException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.True(startTriggered, "OnStart is not triggered");
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
            Assert.IsTrue(exceptionThrown, "TestControllersException expected");
        }

        [Test]
        public async Task ControllerWithResultBase_Exception_Dispose()
        {
            // Arrange
            var startTriggered = false;
            var flowTriggered = false;
            var stopTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => throw new TestControllersException(TestControllersMethodsNamesConsts.DisposeMethodName);
            var exceptionThrown = false;
            LogAssert.Expect(LogType.Exception, new Regex(nameof(TestControllersException)));

            // Act
            try
            {
                _ = await TestRootControllerLauncher
                        .LaunchAsync<ActionModelTestControllerWithResult_CompleteOnStart, TestControllerArgs, TestEmptyControllerResult>(
                            args, _controllerFactory, CancellationToken);
            }
            catch (AggregateException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.True(startTriggered, "OnStart is not triggered");
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(stopTriggered, "OnStop is not triggered");
            Assert.IsTrue(exceptionThrown, "TestControllersException expected");
        }

        [Test]
        public async Task ControllerWithResultBase_Exception_OnStartOnStop()
        {
            // Arrange
            var flowTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => throw new TestControllersException(TestControllersMethodsNamesConsts.OnStartMethodName);
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => throw new TestControllersException(TestControllersMethodsNamesConsts.OnStopMethodName);
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;
            var exceptionThrown = false;

            // Act
            try
            {
                _ = await TestRootControllerLauncher
                        .LaunchAsync<ActionModelTestControllerWithResult_CompleteOnStart, TestControllerArgs, TestEmptyControllerResult>(
                            args, _controllerFactory, CancellationToken);
            }
            catch (AggregateException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
            Assert.IsTrue(exceptionThrown, "TestControllersException expected");
        }

        [Test]
        public async Task ControllerWithResultBase_Fail_OnStart()
        {
            // Arrange
            var startTriggered = false;
            var flowTriggered = false;
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;
            var exceptionThrown = false;

            // Act
            try
            {
                _ = await TestRootControllerLauncher
                        .LaunchAsync<ActionModelTestControllerWithResult_FailOnStart, TestControllerArgs, TestEmptyControllerResult>(
                            args, _controllerFactory, CancellationToken);
            }
            catch (TestControllersException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.True(startTriggered, "OnStart is not triggered");
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(stopTriggered, "OnStop is not triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
            Assert.IsTrue(exceptionThrown, "TestControllersException expected");
        }

        [Test]
        public async Task ControllerWithResultBase_Fail_OnStop()
        {
            // Arrange
            var startTriggered = false;
            var flowTriggered = false;
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;

            // Act
            var result = await TestRootControllerLauncher
                             .LaunchAsync<ActionModelTestControllerWithResult_FailOnStop, TestControllerArgs, TestEmptyControllerResult>(
                                 args,
                                 _controllerFactory, CancellationToken);

            // Assert
            Assert.True(startTriggered, "OnStart is not triggered");
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(stopTriggered, "OnStop is not triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
            Assert.IsNotNull(result, "Result shouldn't be null");
            var resultCorrect = string.Equals(args.InputString, result.ResultString);
            Assert.IsTrue(resultCorrect, "Result is incorrect");
        }

        [Test]
        public async Task ControllerWithResultBase_Fail_InDispose()
        {
            // Arrange
            var startTriggered = false;
            var flowTriggered = false;
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);
            _testControllersActionModel.StartTriggered += () => startTriggered = true;
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;

            // Act
            var result = await TestRootControllerLauncher
                             .LaunchAsync<ActionModelTestControllerWithResult_FailInDispose, TestControllerArgs, TestEmptyControllerResult>(
                                 args,
                                 _controllerFactory, CancellationToken);

            // Assert
            Assert.True(startTriggered, "OnStart is not triggered");
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(stopTriggered, "OnStop is not triggered");
            Assert.False(disposeTriggered, "Dispose has been triggered");
            Assert.IsNotNull(result, "Result shouldn't be null");
            var resultCorrect = string.Equals(args.InputString, result.ResultString);
            Assert.IsTrue(resultCorrect, "Result is incorrect");
        }

        [Test]
        public async Task ControllerWithResultBase_CancellationToken_OnStart()
        {
            // Arrange
            var flowTriggered = false;
            var stopTriggered = false;
            var disposeTriggered = false;

            var testGuid = TestControllersUtils.GenerateTestGuid();
            var args = new TestControllerArgs(testGuid);

            _testControllersActionModel.StartTriggered += () => _cancellationTokenSource.Cancel();
            _testControllersActionModel.FlowTriggered += () => flowTriggered = true;
            _testControllersActionModel.StopTriggered += () => stopTriggered = true;
            _testControllersActionModel.DisposeTriggered += () => disposeTriggered = true;
            var exceptionThrown = false;

            // Act
            try
            {
                _ = await TestRootControllerLauncher
                        .LaunchAsync<ActionModelTestControllerWithResult_CompleteOnFlowAsync, TestControllerArgs, TestEmptyControllerResult>(
                            args,
                            _controllerFactory, CancellationToken);
            }
            catch (OperationCanceledException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.False(flowTriggered, "OnFlowAsync has been triggered");
            Assert.True(stopTriggered, "OnStop is not triggered");
            Assert.True(disposeTriggered, "Dispose is not triggered");
            Assert.IsTrue(exceptionThrown, "TestControllersException expected");
            Assert.True(CancellationToken.IsCancellationRequested, "Cancellation token should be cancelled");
        }
    }
}
