using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameUIView is a MonoBehaviour representing the UI view components in the game.
    /// It provides access to PopupsContainer, a serialized reference to manage pop-up UI elements,
    /// and CollectedFruitsView, which connects to GameUICollectedFruitsView for displaying collected fruits UI.
    /// This class facilitates interaction with and management of UI elements within the game scene.
    /// </summary>
    public class GameUIView : MonoBehaviour
    {
        public Transform PopupsContainer => _popupsContainer;
        public GameUICollectedFruitsView CollectedFruitsView => _collectedFruitsView;

        [SerializeField]
        private Transform _popupsContainer;

        [SerializeField]
        private GameUICollectedFruitsView _collectedFruitsView;
    }
}