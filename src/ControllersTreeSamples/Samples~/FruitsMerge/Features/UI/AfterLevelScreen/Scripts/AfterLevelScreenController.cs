using System.Threading;
using Cysharp.Threading.Tasks;
using Playtika.Controllers;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// AfterLevelScreenController manages the post-level screen UI, instantiated from AfterLevelScreenView.
    /// It handles UI setup and cleanup during its lifecycle.
    /// Initialization includes displaying the screen and binding a restart button click event to RestartButtonClicked().
    /// On stopping, it removes event listeners and destroys the UI instance. RestartButtonClicked() triggers a restart
    /// request through GameModel.RequestRestart().
    /// </summary>
    public class AfterLevelScreenController : ControllerWithResultBase
    {
        private readonly IGameEventsRequestsModel _gameRequestsModel;
        private readonly ResourcesProvider _resourcesProvider;
        private readonly Transform _container;

        private AfterLevelScreenView _view;

        public AfterLevelScreenController(
            IControllerFactory controllerFactory,
            GameUIView uiView,
            IGameEventsRequestsModel gameRequestsModel,
            ResourcesProvider resourcesProvider)
            : base(controllerFactory)
        {
            _container = uiView.PopupsContainer;
            _gameRequestsModel = gameRequestsModel;
            _resourcesProvider = resourcesProvider;
        }

        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            var prefab = await _resourcesProvider.LoadAsync<AfterLevelScreenView>("AfterLevelScreen", cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            _view = Object.Instantiate(prefab, _container);
            _view.Show();

            _view.RestartButton.onClick.AddListener(RestartButtonClicked);
        }

        protected override void OnStop()
        {
            _view.RestartButton.onClick.RemoveAllListeners();

            Object.Destroy(_view.gameObject);
        }

        private void RestartButtonClicked()
        {
            _gameRequestsModel.RequestRestart();
        }
    }
}