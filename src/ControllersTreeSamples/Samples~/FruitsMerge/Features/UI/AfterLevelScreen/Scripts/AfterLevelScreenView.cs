using UnityEngine;
using UnityEngine.UI;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// AfterLevelScreenView is a MonoBehaviour, manages the view component displayed after completing a game level.
    /// </summary>
    public class AfterLevelScreenView : MonoBehaviour
    {
        private const string ShowAnimationName = "AfterLevelScreen_Show";

        public Button RestartButton => _restartButton;

        [SerializeField]
        private Button _restartButton;

        [SerializeField]
        private Animation _animationComponent;

        public void Show()
        {
            _animationComponent.Play(ShowAnimationName);
        }
    }
}