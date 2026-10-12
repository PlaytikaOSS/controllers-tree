using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameUICollectedFruitItemView, a MonoBehaviour, manages individual visual representations of collected fruits in the game's UI.
    /// This class facilitates dynamic display and animation management of collected fruit items within the game's UI system.
    /// </summary>
    public class GameUICollectedFruitItemView : MonoBehaviour
    {
        private const string ShowAnimationName = "CollectedFruitItem_Show";
        private const string HideAnimationName = "CollectedFruitItem_Hide";

        [SerializeField]
        private Image _image;

        [SerializeField]
        private Animation _animationComponent;

        public void SetVisualisation(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        public void Show()
        {
            _animationComponent.Play(ShowAnimationName);
        }

        public UniTask HideAsync(CancellationToken cancellationToken)
        {
            return _animationComponent.PlayAsync(HideAnimationName, cancellationToken);
        }
    }
}