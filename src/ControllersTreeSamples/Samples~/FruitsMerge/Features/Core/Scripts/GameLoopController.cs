using System.Threading;
using Cysharp.Threading.Tasks;
using Playtika.Controllers;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameLoopController extends ControllerWithResultBase to manage a game loop asynchronously.
    /// It initializes a GameModel and handles restart requests using a UniTaskCompletionSource.
    /// The controller starts and stops related controllers (GameUIController and GameEnvironmentController).
    /// It awaits restart signals via _gameModel.RestartRequested and completes the loop upon confirmation,
    /// ensuring orderly flow with Complete().
    /// </summary>
    public class GameLoopController : ControllerWithResultBase
    {
        private readonly GameModel _gameModel;
        private readonly IGameEventsModel _gameEventsModel;

        public GameLoopController(
            IControllerFactory controllerFactory,
            GameModel gameModel,
            IGameEventsModel gameEventsModel)
            : base(controllerFactory)
        {
            _gameModel = gameModel;
            _gameEventsModel = gameEventsModel;
        }

        protected override void OnStart()
        {
            _gameEventsModel.RestartRequested += OnRestartRequested;
        }

        protected override void OnStop()
        {
            _gameEventsModel.RestartRequested -= OnRestartRequested;
        }

        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            await _gameModel.InitializeAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            Execute<GameUIController>();
            ExecuteAndWaitResultAsync<GameEnvironmentController>(CancellationToken).Forget();
        }

        private void OnRestartRequested()
        {
            Complete();
        }
    }
}