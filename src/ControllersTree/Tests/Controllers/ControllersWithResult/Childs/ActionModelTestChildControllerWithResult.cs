using System.Threading;
using System.Threading.Tasks;
using Playtika.Controllers;
using Cysharp.Threading.Tasks;

namespace UnitTests.Controllers
{
    internal class ActionModelTestChildControllerWithResult : ControllerWithResultBase<string, EmptyControllerResult>
    {
        protected readonly TestChildControllersActionModel TestChildControllersActionModel;
        protected string TestControllerGuid => Args;

        public ActionModelTestChildControllerWithResult(
            IControllerFactory controllerFactory,
            TestChildControllersActionModel testChildControllersModel)
            : base(controllerFactory)
        {
            TestChildControllersActionModel = testChildControllersModel;
        }

        protected override void OnStart()
        {
            base.OnStart();
            AddDisposable(new DisposableToken(() =>
            {
                TestChildControllersActionModel.TriggerChildDispose(TestControllerGuid);
            }));
            TestChildControllersActionModel.TriggerChildStart(TestControllerGuid);
        }

        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            await base.OnFlowAsync(cancellationToken);
            await Task.Delay(50, cancellationToken).ConfigureAwait(false);

            TestChildControllersActionModel.TriggerChildFlow(TestControllerGuid);

            await Task.Delay(50, cancellationToken).ConfigureAwait(false);

            Complete(default);
        }

        protected override void OnStop()
        {
            TestChildControllersActionModel.TriggerChildStop(TestControllerGuid);
        }
    }

    internal class ActionModelTestChildControllerWithResult1 : ActionModelTestChildControllerWithResult
    {
        public ActionModelTestChildControllerWithResult1(
            IControllerFactory controllerFactory,
            TestChildControllersActionModel testChildControllersModel)
            : base(controllerFactory, testChildControllersModel)
        {
        }
    }

    internal class ActionModelTestChildControllerWithResult2 : ActionModelTestChildControllerWithResult
    {
        public ActionModelTestChildControllerWithResult2(
            IControllerFactory controllerFactory,
            TestChildControllersActionModel testChildControllersModel)
            : base(controllerFactory, testChildControllersModel)
        {
        }
    }
    internal class ActionModelTestChildControllerWithResult3 : ActionModelTestChildControllerWithResult
    {
        public ActionModelTestChildControllerWithResult3(
            IControllerFactory controllerFactory,
            TestChildControllersActionModel testChildControllersModel)
            : base(controllerFactory, testChildControllersModel)
        {
        }
    }
    internal class ActionModelTestChildControllerWithResult4 : ActionModelTestChildControllerWithResult
    {
        public ActionModelTestChildControllerWithResult4(
            IControllerFactory controllerFactory,
            TestChildControllersActionModel testChildControllersModel)
            : base(controllerFactory, testChildControllersModel)
        {
        }
    }
    internal class ActionModelTestChildControllerWithResult5 : ActionModelTestChildControllerWithResult
    {
        public ActionModelTestChildControllerWithResult5(
            IControllerFactory controllerFactory,
            TestChildControllersActionModel testChildControllersModel)
            : base(controllerFactory, testChildControllersModel)
        {
        }
    }
}