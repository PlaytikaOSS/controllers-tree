using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameView is a MonoBehaviour representing the main game view in the Unity engine.
    /// It provides access to UiView and EnvironmentView, which are serialized references to GameUIView and
    /// GameEnvironmentView components, respectively. This class acts as a bridge for accessing and manipulating
    /// UI and environment elements within the game scene.
    /// </summary>
    public class GameView : MonoBehaviour
    {
        public GameUIView UiView => _uiView;
        public GameEnvironmentView EnvironmentView => _environmentView;

        [SerializeField]
        private GameUIView _uiView;

        [SerializeField]
        private GameEnvironmentView _environmentView;
    }
}