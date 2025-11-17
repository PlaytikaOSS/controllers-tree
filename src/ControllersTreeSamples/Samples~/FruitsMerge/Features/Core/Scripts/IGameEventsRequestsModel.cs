namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// IGameEventsRequestsModel defines methods for requesting game events related to game outcomes and restart functionality.
    /// This interface abstracts the interaction with game events related to game outcomes and restart operations, allowing
    /// for flexible implementation in game logic components.
    /// </summary>
    public interface IGameEventsRequestsModel
    {
        void RequestWin();
        void RequestLose();
        void RequestRestart();
    }
}