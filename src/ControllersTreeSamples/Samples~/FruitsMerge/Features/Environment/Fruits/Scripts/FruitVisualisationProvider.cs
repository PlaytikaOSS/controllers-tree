using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// FruitVisualisationProvider facilitates the retrieval of visual assets for fruits in the game.
    /// It offers methods to obtain the prefab for FruitBoardView and sprites associated with specific resourceId values.
    /// This allows dynamic loading and display of visual elements based on the game's resource configuration.
    /// </summary>
    public class FruitVisualisationProvider
    {
        private readonly ResourcesProvider _resourcesProvider;

        public FruitVisualisationProvider(ResourcesProvider resourcesProvider)
        {
            _resourcesProvider = resourcesProvider;
        }

        public UniTask<FruitBoardView> GetFruitBoardPrefabAsync(CancellationToken cancellationToken)
        {
            return _resourcesProvider.LoadAsync<FruitBoardView>("FruitBoard", cancellationToken);
        }

        public UniTask<Sprite> GetSpriteAsync(string resourceId, CancellationToken cancellationToken)
        {
            return _resourcesProvider.LoadAsync<Sprite>($"{resourceId}_Sprite", cancellationToken);
        }
    }
}