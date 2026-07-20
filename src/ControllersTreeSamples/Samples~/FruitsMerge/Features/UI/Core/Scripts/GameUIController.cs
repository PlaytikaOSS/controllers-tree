using Cysharp.Threading.Tasks;
using Playtika.Controllers;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameUIController extends ControllerBase to manage the game's UI interactions.
    /// It listens for events from GameModel (Win and Lose) to trigger UI updates through OnLevelEnd().
    /// Initialization includes starting a GameUICollectedFruitsController to handle fruit collection UI.
    /// Cleanup involves removing event listeners to ensure proper lifecycle management.
    /// </summary>
    public class GameUIController : ControllerBase
    {
        private readonly IGameEventsModel _gameEventsModel;

        public GameUIController(
            IControllerFactory controllerFactory,
            IGameEventsModel gameEventsModel)
            : base(controllerFactory)
        {
            _gameEventsModel = gameEventsModel;
        }

        protected override void OnStart()
        {
            _gameEventsModel.Win += OnLevelEnd;
            _gameEventsModel.Lose += OnLevelEnd;

            Execute<GameUICollectedFruitsController>();
        }

        protected override void OnStop()
        {
            _gameEventsModel.Win -= OnLevelEnd;
            _gameEventsModel.Lose -= OnLevelEnd;
        }

        private void OnLevelEnd()
        {
            ExecuteAndWaitResultAsync<AfterLevelScreenController>(CancellationToken).Forget();
        }
    }
}