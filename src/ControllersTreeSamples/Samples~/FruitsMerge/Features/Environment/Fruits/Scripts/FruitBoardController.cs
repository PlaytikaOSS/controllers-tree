using System.Threading;
using Cysharp.Threading.Tasks;
using Playtika.Controllers;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// FruitBoardController manages interactions for individual fruit in the game.
    /// It initializes with a FruitData instance and utilizes a FruitBoardFactory to create and display corresponding views.
    /// Upon user interaction, it triggers removal requests through GameModel.RequestFruitRemove() and completes
    /// operations with EmptyControllerResult. The controller handles lifecycle events, ensuring proper initialization,
    /// interaction handling, and cleanup of associated views.
    /// </summary>
    public class FruitBoardController : ControllerWithResultBase<FruitBoardControllerArgs, EmptyControllerResult>
    {
        private readonly GameModel _gameModel;
        private readonly FruitBoardFactory _factory;

        private FruitBoardView _view;
        private UniTaskCompletionSource _clickCs = new();
        private FruitData _data => Args.Data;

        public FruitBoardController(
            IControllerFactory controllerFactory,
            GameModel gameModel,
            FruitBoardFactory factory)
            : base(controllerFactory)
        {
            _gameModel = gameModel;
            _factory = factory;
        }

        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            _view = await _factory.CreateAsync(_data.ResourceId, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            _view.Show();
            _view.Clicked += OnClicked;
            
            await _clickCs.Task;
            cancellationToken.ThrowIfCancellationRequested();

            _gameModel.RequestFruitRemove(_data);

            await _view.HideAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            Complete(new EmptyControllerResult());
        }

        protected override void OnStop()
        {
            if (_view != null)
            {
                _view.Clicked -= OnClicked;
                Object.Destroy(_view.gameObject);
            }
        }

        private void OnClicked()
        {
            if (_gameModel.IsFruitLineFilled())
            {
                return;
            }

            _clickCs.TrySetResult();
        }
    }
}