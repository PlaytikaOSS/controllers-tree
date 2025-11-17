using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// GameConfiguration is a ScriptableObject used to define game settings related to fruit collection.
    /// This class allows flexible adjustment of game parameters through the Unity Editor's Inspector,
    /// providing essential configuration options for gameplay mechanics related to fruit collection.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "Scriptable Objects/Game Configuration")]
    public class GameConfiguration : ScriptableObject
    {
        [Range(2, 4)]
        public int FruitCopyCollectAmount = 3;
        
        [Range(3, 6)]
        public int MaxFruitsCollectedCount = 6;
        
        [Range(10, 50)]
        public int InitialFruitsCount = 40;
    }
}