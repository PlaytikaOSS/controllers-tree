using System;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// IGameEventsModel defines events representing game events such as winning, losing, and restart requests.
    /// This interface serves as a contract for components that need to subscribe to or invoke these game events,
    /// providing a structured approach to handle game state transitions and actions in a game development context.
    /// </summary>
    public interface IGameEventsModel
    {
        public event Action Win;
        public event Action Lose;
        public event Action RestartRequested;
    }
}