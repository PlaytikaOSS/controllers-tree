using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Playtika.Controllers;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameUICollectedFruitsController handles the UI display of collected fruits in the game.
    /// It dynamically manages visual slots based on changes in the GameModel.
    /// Initialization clears and sets up the UI slots. Event handlers update the UI when fruits are added or removed,
    /// providing visual feedback asynchronously for removal animations.
    /// </summary>
    public class GameUICollectedFruitsController : ControllerBase
    {
        private readonly GameModel _gameModel;
        private readonly FruitVisualisationProvider _visualisationProvider;
        private readonly GameUICollectedFruitsView _view;

        private List<GameUICollectedFruitSlotView> _slotViews;

        public GameUICollectedFruitsController(
            IControllerFactory controllerFactory,
            GameModel gameModel,
            FruitVisualisationProvider visualisationProvider,
            GameUICollectedFruitsView view)
            : base(controllerFactory)
        {
            _gameModel = gameModel;
            _visualisationProvider = visualisationProvider;
            _view = view;

            _slotViews = new List<GameUICollectedFruitSlotView>(_gameModel.GameConfiguration.MaxFruitsCollectedCount);
        }

        protected override void OnStart()
        {
            _view.Container.Clear();

            for (var i = 0; i < _slotViews.Capacity; i++)
            {
                var slotInstance = Object.Instantiate(_view.SlotPrefab, _view.Container);
                _slotViews.Add(slotInstance);
            }

            _gameModel.FruitAddedToList += OnFruitAddedToList;
            _gameModel.FruitsRemovedFromList += OnFruitsRemovedFromList;
        }

        protected override void OnStop()
        {
            _gameModel.FruitAddedToList -= OnFruitAddedToList;
            _gameModel.FruitsRemovedFromList -= OnFruitsRemovedFromList;
        }

        private void OnFruitAddedToList(int index, FruitData data)
        {
            OnFruitAddedToListAsync(index, data).Forget();
        }

        private async UniTask OnFruitAddedToListAsync(int index, FruitData data)
        {
            var slotView = _slotViews[index];

            var fruitInstance = Object.Instantiate(_view.ItemPrefab, slotView.Container);
            var sprite = await _visualisationProvider.GetSpriteAsync(data.ResourceId, CancellationToken);

            fruitInstance.SetVisualisation(sprite);
            fruitInstance.Show();
        }

        private void OnFruitsRemovedFromList(List<int> indexesList)
        {
            OnFruitsRemovedFromListAsync(indexesList).Forget();
        }
        
        private async UniTask OnFruitsRemovedFromListAsync(List<int> indexesList)
        {
            await UniTask.WaitForSeconds(0.2f, cancellationToken: CancellationToken);

            foreach (var index in indexesList)
            {
                var slotView = _slotViews[index];
                ClearSlot(slotView).Forget();

                await UniTask.WaitForSeconds(0.1f, cancellationToken: CancellationToken);
            }
        }

        private async UniTaskVoid ClearSlot(GameUICollectedFruitSlotView slotView)
        {
            var fruitInstance = slotView.Container.GetComponentInChildren<GameUICollectedFruitItemView>();
            await fruitInstance.HideAsync(CancellationToken);

            Object.Destroy(fruitInstance.gameObject);
        }
    }
}