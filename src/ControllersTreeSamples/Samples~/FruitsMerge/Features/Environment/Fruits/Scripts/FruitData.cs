using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// FruitData is a ScriptableObject representing a type of fruit in the game.
    /// It contains a ResourceId property that uniquely identifies the resource associated with the fruit,
    /// facilitating resource management and retrieval within the game.
    /// </summary>
    [CreateAssetMenu(fileName = "FruitData", menuName = "Scriptable Objects/Fruit Data")]
    public class FruitData : ScriptableObject
    {
        public string ResourceId => _resourceId;
        
        [SerializeField]
        private string _resourceId;
    }
}