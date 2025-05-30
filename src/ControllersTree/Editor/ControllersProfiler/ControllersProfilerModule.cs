#if UNITY_CONTROLLERS_PROFILER
using Playtika.Controllers;
using Unity.Profiling;
using Unity.Profiling.Editor;

namespace Modules.Controller.Editor
{
    [System.Serializable]
    [ProfilerModuleMetadata("Controllers profiler")]
    public class ControllersProfilerModule : ProfilerModule
    {
        private static readonly ProfilerCounterDescriptor[] Counters = new ProfilerCounterDescriptor[]
        {
            new ProfilerCounterDescriptor(Helper.ActiveControllersCount, ProfilerCategory.Scripts),
            new ProfilerCounterDescriptor(Helper.TotalControllersCount, ProfilerCategory.Scripts),
            new ProfilerCounterDescriptor(Helper.CreateThisFrameControllersCount, ProfilerCategory.Scripts),
        };

        public ControllersProfilerModule()
            : base(Counters)
        {
        }

        public override ProfilerModuleViewController CreateDetailsViewController()
        {
            return new ControllersProfilerModuleViewController(ProfilerWindow);
        }
    }
}
#endif