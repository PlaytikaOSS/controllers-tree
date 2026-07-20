using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameEnvironmentView, a MonoBehaviour, represents the view component managing the game environment within the Unity scene.
    /// </summary>
    public class GameEnvironmentView : MonoBehaviour
    {
        public Transform Container => _container;
        public Vector2 SpawnArea => _spawnArea;

        [SerializeField]
        private Transform _container;
        
        [SerializeField]
        private Vector2 _spawnArea;
    }
}