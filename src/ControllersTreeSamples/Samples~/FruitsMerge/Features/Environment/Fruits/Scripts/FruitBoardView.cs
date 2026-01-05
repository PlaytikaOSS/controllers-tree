using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// FruitBoardView, a MonoBehaviour, represents a visual component in the game for displaying fruits.
    /// This class facilitates the visual representation and interaction handling of individual fruits within the game's environment.
    /// </summary>
    public class FruitBoardView : MonoBehaviour, IPointerClickHandler
    {
        private const string ShowAnimationName = "FruitBoard_Show";
        private const string HideAnimationName = "FruitBoard_Hide";

        public event Action Clicked;

        [SerializeField]
        private SpriteRenderer _renderer;

        [SerializeField]
        private Animation _animationComponent;

        public void SetVisualisation(Sprite sprite)
        {
            _renderer.sprite = sprite;
        }

        public void Show()
        {
            _animationComponent.Play(ShowAnimationName);
        }

        public UniTask HideAsync(CancellationToken cancellationToken)
        {
            return _animationComponent.PlayAsync(HideAnimationName, cancellationToken);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke();
        }
    }
}