using Playtika.Controllers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// RootLifetimeScope sets up the DI container for the game application, configuring essential components such as
    /// the entry point (Bootstrap), controllers, and services. This setup ensures efficient dependency management and lifecycle handling during gameplay.
    /// </summary>
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private GameView _gameView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<Bootstrap>();
            builder.Register<IControllerFactory, ControllerFactory>(Lifetime.Scoped);
            builder.Register<BootstrapController>(Lifetime.Transient);
            builder.Register<GameLoopController>(Lifetime.Transient);

            builder.RegisterInstance(_gameView.EnvironmentView);
            builder.RegisterInstance(_gameView.UiView);
            builder.RegisterInstance(_gameView.UiView.CollectedFruitsView);

            builder.Register<GameModel>(Lifetime.Singleton);
            builder.Register<IGameEventsModel, IGameEventsRequestsModel, GameEventsModel>(Lifetime.Singleton);

            builder.Register<GameEnvironmentController>(Lifetime.Transient);
            builder.Register<FruitBoardController>(Lifetime.Transient);

            builder.Register<GameUIController>(Lifetime.Transient);
            builder.Register<GameUICollectedFruitsController>(Lifetime.Transient);
            builder.Register<AfterLevelScreenController>(Lifetime.Transient);

            builder.Register<ResourcesProvider>(Lifetime.Singleton);
            builder.Register<FruitVisualisationProvider>(Lifetime.Singleton);
            builder.Register<FruitBoardFactory>(Lifetime.Singleton);
        }
    }
}