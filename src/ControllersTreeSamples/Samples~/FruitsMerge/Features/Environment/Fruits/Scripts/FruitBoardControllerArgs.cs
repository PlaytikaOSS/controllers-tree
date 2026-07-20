namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// FruitBoardControllerArgs is a readonly struct used to pass data to FruitBoardController instances during
    /// initialization. It encapsulates a FruitData instance, ensuring that the data is immutable and efficiently passed
    /// between components in the game's architecture.
    /// </summary>
    public readonly struct FruitBoardControllerArgs
    {
        public readonly FruitData Data;

        public FruitBoardControllerArgs(FruitData data)
        {
            Data = data;
        }
    }
}