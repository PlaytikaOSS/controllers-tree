using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// AnimationExtensions is a static class providing extension methods for Unity's Animation class.
    /// </summary>
    public static class AnimationExtensions
    {
        public static async UniTask PlayAsync(
            this Animation animationComponent,
            string animationName,
            CancellationToken cancellationToken)
        {
            animationComponent.Play(animationName);

            while (animationComponent.isPlaying)
            {
                await UniTask.Yield();
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}