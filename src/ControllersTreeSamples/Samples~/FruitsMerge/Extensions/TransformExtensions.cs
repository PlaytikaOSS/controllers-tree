using UnityEngine;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// TransformExtensions is a static class providing extension methods for Unity's Transform class.
    /// </summary>
    public static class TransformExtensions
    {
        public static void Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }
}