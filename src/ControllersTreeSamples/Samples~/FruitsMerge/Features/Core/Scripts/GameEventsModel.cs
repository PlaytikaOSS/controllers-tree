using System;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// The GameEventsModel class implements both IGameEventsModel and IGameEventsRequestsModel interfaces.
    /// This class serves as a centralized model for managing and signaling game events,
    /// facilitating interaction and synchronization between different game components.
    /// </summary>
    public class GameEventsModel : IGameEventsModel, IGameEventsRequestsModel
    {
        public event Action Win;
        public event Action Lose;
        public event Action RestartRequested;

        public void RequestWin()
        {
            Win?.Invoke();
        }

        public void RequestLose()
        {
            Lose?.Invoke();
        }
        
        public void RequestRestart()
        {
            RestartRequested?.Invoke();
        }
    }
}