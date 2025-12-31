using System;
using System.Threading;
using VContainer;
using VContainer.Unity;

namespace ControllersTree.Samples.FruitsMerge
{
    /// <summary>
    /// Bootstrap manages the application startup and shutdown.
    /// It utilizes dependency injection with IObjectResolver and BootstrapController to initialize and control the application flow.
    /// The Start() method launches the bootstrap process, handling exceptions and cancellation,
    /// while Dispose() ensures proper cleanup by cancelling ongoing operations and releasing associated resources.
    /// </summary>
    public class Bootstrap : IStartable, IDisposable
    {
        private readonly BootstrapController _bootstrapController;

        private CancellationTokenSource _cancellationTokenSource = new();

        public Bootstrap(BootstrapController bootstrapController)
        {
            _bootstrapController = bootstrapController;
        }

        public void Start()
        {
            try
            {
                _bootstrapController.LaunchTree(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError($"Can't start bootstrap controller {exception}");
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = null;
        }
    }
}