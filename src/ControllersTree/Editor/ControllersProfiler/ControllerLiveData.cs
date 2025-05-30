#if UNITY_CONTROLLERS_PROFILER
using UnityEngine;

namespace Modules.Controller.Editor
{
    internal sealed class ControllerLiveData
    {
        public string ControllerName;
        public Color ControllerColor;
        public string ScopeName;
        public Color ScopeColor;
        public long StartTimeMSec;
        public long LifeTimeMSec;
    }
}
#endif