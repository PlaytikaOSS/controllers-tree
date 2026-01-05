using System.Collections.Generic;
using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// FruitDataCollectionDefinition is a ScriptableObject that holds a collection of FruitData instances.
    /// It provides a structured way to store and manage multiple types of fruit data within game resources.
    /// </summary>
    [CreateAssetMenu(fileName = "FruitsCollection", menuName = "Scriptable Objects/Fruits Collection")]
    public class FruitDataCollectionDefinition : ScriptableObject
    {
        public IReadOnlyList<FruitData> List => _list;
        
        [SerializeField]
        private List<FruitData> _list;
    }
}