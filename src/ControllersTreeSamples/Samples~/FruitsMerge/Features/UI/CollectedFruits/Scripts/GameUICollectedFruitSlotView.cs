using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameUICollectedFruitSlotView, a MonoBehaviour, represents a slot in the UI for displaying collected fruit items.
    /// </summary>
    public class GameUICollectedFruitSlotView : MonoBehaviour
    {
        public Transform Container => _container;

        [SerializeField]
        private Transform _container;
    }
}