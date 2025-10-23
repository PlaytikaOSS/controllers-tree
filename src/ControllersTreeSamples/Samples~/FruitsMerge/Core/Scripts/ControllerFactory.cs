using Playtika.Controllers;
using VContainer;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// The ControllerFactory class implements the IControllerFactory interface to facilitate the creation
    /// of controllers using a project-specific Dependency Injection (DI) container.
    /// It utilizes an IObjectResolver instance to resolve and instantiate controllers of type T
    /// that implement theIController interface.
    /// </summary>
    public class ControllerFactory : IControllerFactory
    {
        private readonly IObjectResolver _container;

        public ControllerFactory(IObjectResolver container)
        {
            _container = container;
        }

        public IController Create<T>() where T : class, IController
        {
            var controller = _container.Resolve<T>();
            return controller;
        }
    }
}