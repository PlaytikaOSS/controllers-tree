using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Playtika.Controllers;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameEnvironmentController manages the game environment setup using GameEnvironmentView and GameModel.
    /// Initialization involves populating the environment with fruits based on configuration and randomizing their order.
    /// It asynchronously initializes each fruit using FruitBoardController instances.
    /// </summary>
    public class GameEnvironmentController : ControllerWithResultBase
    {
        private readonly GameModel _gameModel;
        private readonly ResourcesProvider _resourcesProvider;
        private readonly GameEnvironmentView _environmentView;

        public GameEnvironmentController(
            IControllerFactory controllerFactory,
            GameModel gameModel,
            ResourcesProvider resourcesProvider,
            GameEnvironmentView environmentView)
            : base(controllerFactory)
        {
            _gameModel = gameModel;
            _resourcesProvider = resourcesProvider;
            _environmentView = environmentView;
        }

        protected override UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            return InitializeBoard(cancellationToken);
        }

        private async UniTask InitializeBoard(CancellationToken cancellationToken)
        {
            _environmentView.Container.Clear();

            var fruitCollection = await _resourcesProvider.LoadAsync<FruitDataCollectionDefinition>("FruitsCollection", cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var fruitsList = new List<FruitData>(_gameModel.FruitsOnBoardCount);
            for (var i = 0; i < _gameModel.FruitsOnBoardCount; i++)
            {
                var index = (int)Mathf.Repeat(i / _gameModel.GameConfiguration.FruitCopyCollectAmount,
                    fruitCollection.List.Count);
                fruitsList.Add(fruitCollection.List[index]);
            }

            var random = new System.Random();
            fruitsList = fruitsList.OrderBy(x => random.Next()).ToList();

            foreach (var fruitData in fruitsList)
            {
                var args = new FruitBoardControllerArgs(fruitData);
                ExecuteAndWaitResultAsync<FruitBoardController, FruitBoardControllerArgs>(args, CancellationToken)
                    .Forget();

                await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);
            }
        }
    }
}