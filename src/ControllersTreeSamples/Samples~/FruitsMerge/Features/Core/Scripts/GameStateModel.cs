using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    public class GameStateModel
    {
        public event Action<int, FruitData> FruitAddedToList;
        public event Action<List<int>> FruitsRemovedFromList;

        public GameConfiguration GameConfiguration { get; }
        public int FruitsOnBoardCount { get; private set; }

        private FruitData[] _fruitsCollected;

        public GameStateModel()
        {
            GameConfiguration = Resources.Load<GameConfiguration>("GameConfiguration");
        }

        public void Initialize()
        {
            _fruitsCollected = new FruitData[GameConfiguration.MaxFruitsCollectedCount];
            FruitsOnBoardCount = GameConfiguration.InitialFruitsCount - GameConfiguration.InitialFruitsCount %
                GameConfiguration.FruitCopyCollectAmount;
        }

        public void RequestFruitRemove(FruitData data)
        {
            AddToList(data);
            FruitsOnBoardCount--;

            CheckForCopiesCollected();
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