#if UNITY_CONTROLLERS_PROFILER
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Playtika.Controllers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NameSessionData
    {
        public int NameHash;
        public FixedString Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CreateControllerFrameData
    {
        public int NameHash;
        public long StartTimeMSec;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StopControllerFrameData
    {
        public int NameHash;
        public long ElapsedMilliseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RunControllerFrameData
    {
        public int NameHash;
        public int ScopeHash;
        public int InstanceId;
        public int Level;
        public long StartTimeMSec;
        public long ElapsedMilliseconds;
    }

    public static class Helper
    {
        public static readonly Guid ControllerProfilerGuid = new Guid("3E26292C-09A6-4DFD-8EC7-9BF7E5E3B837");
        public const int ControllerNameTag = 0;
        public const int ControllerStartTag = 1;
        public const int ControllerStopTag = 2;
        public const int ControllerRunTag = 3;
        public const int ScopeNameTag = 4;

        public const string TotalControllersCount = "Total Controllers Count";
        public const string ActiveControllersCount = "Active Controllers Count";
        public const string CreateThisFrameControllersCount = "Create This Frame Controllers Count";
    }
}
#endif
