#if UNITY_CONTROLLERS_PROFILER
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Unity.Collections;
using UnityEngine.Profiling;

namespace Playtika.Controllers
{
    public partial class ControllerBase
    {
        protected static int _totalCount;
        protected static int _activeCount;
        protected static int _createdThisFrameCount;

        internal int _instanceId;
        internal long _startTimeMSec;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        internal List<IController> ChildControllers => _childControllers;
        internal string ScopeName => _controllerFactory?.ToString();

        internal long GetLifetime() => _stopwatch.ElapsedMilliseconds;
        private HashSet<int> _savedNames = new HashSet<int>();

        partial void ProfileOnCreated()
        {
            ++_totalCount;
            ++_activeCount;
            ++_createdThisFrameCount;
        }

        partial void ProfileOnStart()
        {
            _startTimeMSec = _stopwatch.ElapsedMilliseconds;
            PushCreateControllerToProfilerStream(Name, _startTimeMSec);
            _stopwatch.Restart();
        }

        partial void ProfileOnStop()
        {
            --_activeCount;
            var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
            PushStopControllerToProfilerStream(Name, elapsedMilliseconds);
        }

        private void PushCreateControllerToProfilerStream(
            string name,
            long startTimeMSec)
        {
            if (!Profiler.enabled || string.IsNullOrEmpty(name))
            {
                return;
            }

            var nameHash = name.GetHashCode();
            var data = new NativeArray<CreateControllerFrameData>(1, Allocator.Temp);
            data[0] = new CreateControllerFrameData()
            {
                NameHash = nameHash,
                StartTimeMSec = startTimeMSec
            };

            Profiler.EmitFrameMetaData(Helper.ControllerProfilerGuid, Helper.ControllerStartTag, data);
            data.Dispose();

            if (_savedNames.Contains(nameHash))
            {
                return;
            }

            var nameData = new NativeArray<NameSessionData>(1, Allocator.Temp);
            nameData[0] = new NameSessionData()
            {
                NameHash = nameHash,
                Name = name
            };
            Profiler.EmitSessionMetaData(Helper.ControllerProfilerGuid, Helper.ControllerNameTag, nameData);
            nameData.Dispose();
            _savedNames.Add(nameHash);
        }

        private static void PushStopControllerToProfilerStream(string name, long elapsedMilliseconds)
        {
            if (!Profiler.enabled || string.IsNullOrEmpty(name))
            {
                return;
            }

            var data = new NativeArray<StopControllerFrameData>(1, Allocator.Temp);
            data[0] = new StopControllerFrameData()
            {
                NameHash = name.GetHashCode(),
                ElapsedMilliseconds = elapsedMilliseconds
            };

            Profiler.EmitFrameMetaData(Helper.ControllerProfilerGuid, Helper.ControllerStopTag, data);
            data.Dispose();
        }
    }
}
#endif
