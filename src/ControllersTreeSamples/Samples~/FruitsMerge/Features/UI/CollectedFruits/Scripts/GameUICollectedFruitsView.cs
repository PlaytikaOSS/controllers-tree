using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameUICollectedFruitsView, a MonoBehaviour, manages UI components for displaying collected fruits in the game.
    /// These elements facilitate the organization and presentation of collected fruits within the game's UI system.
    /// </summary>
    public class GameUICollectedFruitsView : MonoBehaviour
    {
        public Transform Container => _container;
        public GameUICollectedFruitSlotView SlotPrefab => _slotPrefab;
        public GameUICollectedFruitItemView ItemPrefab => _itemPrefab;

        [SerializeField]
        private Transform _container;

        [SerializeField]
        private GameUICollectedFruitSlotView _slotPrefab;

        [SerializeField]
        private GameUICollectedFruitItemView _itemPrefab;
    }
}