using System.Threading;
using Playtika.Controllers;
using Cysharp.Threading.Tasks;

namespace UnitTests.Controllers
{
    internal class ActionModelTestControllerWithResult : ControllerWithResultBase<TestControllerArgs, TestEmptyControllerResult>
    {
        protected readonly TestControllersActionModel TestControllersActionModel;

        internal ActionModelTestControllerWithResult(
            IControllerFactory controllerFactory,
            TestControllersActionModel testControllersActionModel)
            : base(controllerFactory)
        {
            TestControllersActionModel = testControllersActionModel;
        }

        protected override void OnStart()
        {
            base.OnStart();
            AddDisposable(new DisposableToken(() =>
            {
                TestControllersActionModel.TriggerDispose();
            }));
            TestControllersActionModel.Args = Args.InputString;
            TestControllersActionModel.TriggerStart();
        }

        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            await base.OnFlowAsync(cancellationToken);
            await UniTask.Delay(10, cancellationToken: cancellationToken);
            TestControllersActionModel.TriggerFlow();
            await UniTask.Delay(10, cancellationToken: cancellationToken);
        }

        protected override void OnStop()
        {
            TestControllersActionModel.TriggerStop();
        }
    }
}
