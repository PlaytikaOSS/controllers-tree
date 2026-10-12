using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameModel manages game logic related to fruit collection and events in a game.
    /// This class orchestrates the game mechanics related to fruit collection, including managing events and
    /// checking game conditions based on configured rules, enhancing gameplay logic and event handling within
    /// a game environment.
    /// </summary>
    public class GameModel
    {
        public event Action<int, FruitData> FruitAddedToList;
        public event Action<List<int>> FruitsRemovedFromList;

        public GameConfiguration GameConfiguration { get; private set; }
        public int FruitsOnBoardCount { get; private set; }

        private readonly ResourcesProvider _resourcesProvider;
        private readonly IGameEventsRequestsModel _eventRequestsModel;

        private FruitData[] _fruitsCollected;

        public GameModel(ResourcesProvider resourcesProvider,
            IGameEventsRequestsModel eventRequestsModel)
        {
            _resourcesProvider = resourcesProvider;
            _eventRequestsModel = eventRequestsModel;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            GameConfiguration = await _resourcesProvider.LoadAsync<GameConfiguration>("GameConfiguration", cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            
            _fruitsCollected = new FruitData[GameConfiguration.MaxFruitsCollectedCount];
            FruitsOnBoardCount = GameConfiguration.InitialFruitsCount - GameConfiguration.InitialFruitsCount %
                GameConfiguration.FruitCopyCollectAmount;
        }

        public void RequestFruitRemove(FruitData data)
        {
            AddToList(data);
            FruitsOnBoardCount--;

            CheckForCopiesCollected();
            
            if (IsFruitBoardEmpty())
            {
                _eventRequestsModel.RequestWin();
            }
            else if (IsFruitLineFilled())
            {
                _eventRequestsModel.RequestLose();
            }
        }

        public bool IsFruitBoardEmpty()
        {
            return FruitsOnBoardCount <= 0;
        }

        public bool IsFruitLineFilled()
        {
            return _fruitsCollected.All(x => x != null);
        }

        private void CheckForCopiesCollected()
        {
            var duplicates = _fruitsCollected
                .Where(x => x != null)
                .GroupBy(x => x)
                .Where(g => g.Count() >= GameConfiguration.FruitCopyCollectAmount)
                .Select(g => g.Key);
            var array = duplicates as FruitData[] ?? duplicates.ToArray();
            if (array.Any())
            {
                var removedIndexes = new List<int>(GameConfiguration.FruitCopyCollectAmount);
                for (var i = 0; i < GameConfiguration.FruitCopyCollectAmount; i++)
                {
                    var index = RemoveFromList(array[0]);
                    removedIndexes.Add(index);
                }

                FruitsRemovedFromList?.Invoke(removedIndexes);
            }
        }

        private void AddToList(FruitData data)
        {
            var firstEmptyIndex = Array.IndexOf(_fruitsCollected, null);
            _fruitsCollected[firstEmptyIndex] = data;

            FruitAddedToList?.Invoke(firstEmptyIndex, data);
        }

        private int RemoveFromList(FruitData data)
        {
            var index = Array.IndexOf(_fruitsCollected, data);
            _fruitsCollected[index] = null;

            return index;
        }
    }
}