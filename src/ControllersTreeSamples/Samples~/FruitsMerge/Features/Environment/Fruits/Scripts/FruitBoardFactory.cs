using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// FruitBoardFactory creates FruitBoardView instances within the game environment.
    /// It utilizes a FruitVisualisationProvider for visual assets and a GameEnvironmentView for placement.
    /// Upon creation, each board is positioned randomly within a specified range and visually represented using asset
    /// data obtained from FruitVisualisationProvider.
    /// </summary>
    public class FruitBoardFactory
    {
        private readonly FruitVisualisationProvider _visualisationProvider;
        private readonly GameEnvironmentView _environmentView;

        public FruitBoardFactory(
            FruitVisualisationProvider visualisationProvider,
            GameEnvironmentView environmentView)
        {
            _visualisationProvider = visualisationProvider;
            _environmentView = environmentView;
        }

        public async UniTask<FruitBoardView> CreateAsync(string resourceId, CancellationToken cancellationToken)
        {
            var result = await UniTask.WhenAll(_visualisationProvider.GetFruitBoardPrefabAsync(cancellationToken),
                                  _visualisationProvider.GetSpriteAsync(resourceId, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();

            var instance = Object.Instantiate(result.Item1, _environmentView.Container);

            instance.transform.localPosition = Vector3.right * Random.Range(_environmentView.SpawnArea.x, _environmentView.SpawnArea.y);
            instance.SetVisualisation(result.Item2);

            return instance;
        }
    }
}