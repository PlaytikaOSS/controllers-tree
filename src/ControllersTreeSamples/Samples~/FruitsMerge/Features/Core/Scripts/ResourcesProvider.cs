using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// This class simplifies the process of loading resources asynchronously in Unity, enhancing flexibility
    /// and performance in resource management tasks within game development.
    /// </summary>
    public class ResourcesProvider
    {
        public async UniTask<T> LoadAsync<T>(string resourceId, CancellationToken cancellationToken) where T : Object
        {
            var result = await Resources.LoadAsync<T>(resourceId).WithCancellation(cancellationToken);
            return result as T;
        }
    }
}