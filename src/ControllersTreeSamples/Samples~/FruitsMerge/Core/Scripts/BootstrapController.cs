using Cysharp.Threading.Tasks;
using Playtika.Controllers;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// BootstrapController extends RootController to manage the initialization flow of the application.
    /// It asynchronously starts a game loop by repeatedly executing GameLoopController instances until cancellation is requested.
    /// </summary>
    public class BootstrapController : RootController
    {
        public BootstrapController(IControllerFactory controllerFactory)
            : base(controllerFactory)
        {
        }

        protected override void OnStart()
        {
            FlowAsync().Forget();

            base.OnStart();
        }

        private async UniTask FlowAsync()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                await ExecuteAndWaitResultAsync<GameLoopController>(CancellationToken);
            }
        }
    }
}