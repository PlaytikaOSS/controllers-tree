#if UNITY_CONTROLLERS_PROFILER
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine.Profiling;

namespace Playtika.Controllers
{
    public partial class RootController
    {
        private static readonly ProfilerCategory ControllerProfilerCategory = ProfilerCategory.Scripts;

        private static readonly ProfilerCounterValue<int> TotalControllersCount =
            new ProfilerCounterValue<int>(
                ControllerProfilerCategory, Helper.TotalControllersCount,
                ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

        private static readonly ProfilerCounterValue<int> ActiveControllersCount =
            new ProfilerCounterValue<int>(
                ControllerProfilerCategory, Helper.ActiveControllersCount,
                ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

        private static readonly ProfilerCounterValue<int> CreateThisFrameControllersCount =
            new ProfilerCounterValue<int>(
                ControllerProfilerCategory, Helper.CreateThisFrameControllersCount,
                ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);
        private HashSet<int> _savedNames = new HashSet<int>();

        partial void ProfileOnStart()
        {
            MadeSnapshot(CancellationToken).Forget();
        }

        private async UniTaskVoid MadeSnapshot(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.LastUpdate);
                PushToProfilerStream();
            }
        }

        private void PushToProfilerStream()
        {
            if (!Profiler.enabled)
            {
                return;
            }

            TotalControllersCount.Value = _totalCount;
            ActiveControllersCount.Value = _activeCount;
            CreateThisFrameControllersCount.Value = _createdThisFrameCount;
            _createdThisFrameCount = 0;

            var count = _activeCount;
            var frameData = new NativeArray<RunControllerFrameData>(count, Allocator.Temp);
            var frameDataIndex = 0;
            ProfilerDumpTree((ControllerBase) this, frameData, ref frameDataIndex, 0);
            Profiler.EmitFrameMetaData(Helper.ControllerProfilerGuid, Helper.ControllerRunTag, frameData);
            frameData.Dispose();
        }

        private void ProfilerDumpTree(
            ControllerBase controllerBase,
            NativeArray<RunControllerFrameData> frameData,
            ref int frameDataIndex,
            int level)
        {
            var scopeName = controllerBase.ScopeName;
            frameData[frameDataIndex] = new RunControllerFrameData()
            {
                NameHash = controllerBase.Name.GetHashCode(),
                ScopeHash = scopeName.GetHashCode(),
                InstanceId = controllerBase._instanceId,
                Level = level,
                StartTimeMSec = controllerBase._startTimeMSec,
                ElapsedMilliseconds = controllerBase.GetLifetime(),
            };

            ++frameDataIndex;

            var nameHash = scopeName.GetHashCode();
            if (!_savedNames.Contains(nameHash))
            {
                PushScopeNameToStream(scopeName, nameHash);
                _savedNames.Add(nameHash);
            }

            foreach (var child in controllerBase.ChildControllers)
            {
                ProfilerDumpTree((ControllerBase) child, frameData, ref frameDataIndex, level + 2);
            }
        }

        private static void PushScopeNameToStream(string name, int nameHash)
        {
            var nameData = new NativeArray<NameSessionData>(1, Allocator.Temp);
            nameData[0] = new NameSessionData()
            {
                NameHash = nameHash,
                Name = name
            };
            Profiler.EmitSessionMetaData(Helper.ControllerProfilerGuid, Helper.ScopeNameTag, nameData);
            nameData.Dispose();
        }
    }
}
#endif
