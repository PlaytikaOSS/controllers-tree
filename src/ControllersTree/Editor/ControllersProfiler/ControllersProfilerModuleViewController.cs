#if UNITY_CONTROLLERS_PROFILER
using System;
using System.Collections.Generic;
using System.Text;
using Playtika.Controllers;
using Unity.Profiling.Editor;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Modules.Controller.Editor
{
    internal class ControllersProfilerModuleViewController : ProfilerModuleViewController
    {
        private Color LightGray = new Color(0.827f, 0.827f, 0.827f);
        const int ItemHeight = 16;
        private TextField _searchField;
        private VisualElement _leftElem;
        private VisualElement _rightElement;
        private VisualElement _leftContainer;
        private VisualElement _rightContainer;
        private ListView _liveControllersListView;
        private ListView _createdAndDestroyedControllersListView;
        private readonly Dictionary<int, string> _namesMap = new();
        private readonly HashSet<int> _createdControllers = new();
        private readonly List<ControllerData> _createdAndDestroyedControllersList = new List<ControllerData>(256);
        private readonly List<ControllerLiveData> _liveControllersList = new List<ControllerLiveData>(256);
        private readonly List<ControllerLiveData> _filteredLiveControllersList = new List<ControllerLiveData>(256);
        private readonly Dictionary<int, Color> _contextColorMap = new();
        private string _currentSearchFilter = string.Empty;

        public ControllersProfilerModuleViewController(ProfilerWindow profilerWindow)
            : base(profilerWindow)
        {
        }

        protected override VisualElement CreateView()
        {
            var view = new VisualElement();
            view.style.flexDirection = FlexDirection.Row;
            view.style.flexGrow = 1;

            _leftContainer = new VisualElement();
            _leftContainer.style.width = Length.Percent(40);
            _leftContainer.style.flexGrow = 1;

            _rightContainer = new VisualElement();
            _rightContainer.style.width = Length.Percent(60);
            _rightContainer.style.flexGrow = 1;

            var header = LiveControllersListView.FillLifetimeListHeader();
            _rightContainer.Add(header);

            _liveControllersListView = new LiveControllersListView(_liveControllersList);
            _rightContainer.Add(_liveControllersListView);

            var searchField = CreateSearchField();
            _rightContainer.Add(searchField);

            _createdAndDestroyedControllersListView = new CreatedAndDestroyedControllersListView(_createdAndDestroyedControllersList);
            _leftContainer.Add(_createdAndDestroyedControllersListView);

            view.Add(_leftContainer);
            view.Add(_rightContainer);

            var selectedFrameIndex = ProfilerWindow.selectedFrameIndex;
            ReloadData(selectedFrameIndex);

            ProfilerWindow.SelectedFrameIndexChanged -= OnSelectedFrameIndexChanged;
            ProfilerWindow.SelectedFrameIndexChanged += OnSelectedFrameIndexChanged;

            return view;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ProfilerWindow.SelectedFrameIndexChanged -= OnSelectedFrameIndexChanged;

            base.Dispose(true);
        }

        private VisualElement CreateSearchField()
        {
            var searchContainer = new VisualElement();
            searchContainer.style.flexDirection = FlexDirection.Row;
            searchContainer.style.paddingBottom = 5;
            searchContainer.style.paddingTop = 5;
            searchContainer.style.paddingLeft = 5;
            searchContainer.style.paddingRight = 5;
            searchContainer.style.flexShrink = 0;

            var searchLabel = new Label("Filter:");
            searchLabel.style.width = 50;
            searchLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            _searchField = new TextField();
            _searchField.style.flexGrow = 1;
            _searchField.RegisterValueChangedCallback(OnSearchValueChanged);

            searchContainer.Add(searchLabel);
            searchContainer.Add(_searchField);

            return searchContainer;
        }

        private void OnSearchValueChanged(ChangeEvent<string> evt)
        {
            _currentSearchFilter = evt.newValue?.ToLower() ?? string.Empty;
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            _filteredLiveControllersList.Clear();

            if (string.IsNullOrEmpty(_currentSearchFilter))
            {
                _filteredLiveControllersList.AddRange(_liveControllersList);
            }
            else
            {
                foreach (var controller in _liveControllersList)
                {
                    if (controller.ControllerName.ToLower().Contains(_currentSearchFilter) ||
                        controller.ScopeName.ToLower().Contains(_currentSearchFilter))
                    {
                        _filteredLiveControllersList.Add(controller);
                    }
                }
            }

            _liveControllersListView.itemsSource = _filteredLiveControllersList;
            _liveControllersListView.Rebuild();
        }

        private string GetControllerName(int hash, RawFrameDataView frameData)
        {
            return GetName(hash, frameData, Helper.ControllerNameTag);
        }

        private string GetScopeName(int hash, RawFrameDataView frameData)
        {
            return GetName(hash, frameData, Helper.ScopeNameTag);
        }

        private string GetName(int hash,
                               RawFrameDataView frameData,
                               int tag)
        {
            if (_namesMap.TryGetValue(hash, out var name))
            {
                return name;
            }

            var count = frameData.GetSessionMetaDataCount(Helper.ControllerProfilerGuid, tag);
            for (var i = 0; i < count; i++)
            {
                var controllerFrameData = frameData.GetSessionMetaData<NameSessionData>(
                    Helper.ControllerProfilerGuid, tag, i);

                if (controllerFrameData.Length == 0)
                {
                    continue;
                }

                foreach (var inf in controllerFrameData)
                {
                    _namesMap[inf.NameHash] = inf.Name.ToString();
                }
            }

            if (_namesMap.TryGetValue(hash, out var newName))
            {
                return newName;
            }

            return hash.ToString();
        }

        private void ReloadData(long selectedFrameIndex)
        {
            var selectedFrameIndexInt32 = System.Convert.ToInt32(selectedFrameIndex);

            using var frameData = ProfilerDriver.GetRawFrameDataView(selectedFrameIndexInt32, 0);

            if (frameData == null || !frameData.valid)
            {
                return;
            }

            _createdControllers.Clear();
            _createdAndDestroyedControllersList.Clear();
            GetCreatedControllers(frameData, _createdAndDestroyedControllersList);
            GetStoppedControllers(frameData, _createdAndDestroyedControllersList);
            _createdAndDestroyedControllersListView.itemsSource = _createdAndDestroyedControllersList;
            _createdAndDestroyedControllersListView.Rebuild();

            _liveControllersList.Clear();
            GetLiveControllers(frameData, _liveControllersList);
            ApplySearchFilter();
        }

        private void GetLiveControllers(RawFrameDataView frameData, List<ControllerLiveData> controllers)
        {
            var count = frameData.GetFrameMetaDataCount(Helper.ControllerProfilerGuid, Helper.ControllerRunTag);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                using var runControllerFrameData =
                    frameData.GetFrameMetaData<RunControllerFrameData>(
                        Helper.ControllerProfilerGuid, Helper.ControllerRunTag, i);

                if (runControllerFrameData.Length == 0)
                {
                    continue;
                }

                foreach (var inf in runControllerFrameData)
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(' ', inf.Level);
                    stringBuilder.Append(GetControllerName(inf.NameHash, frameData));
                    var controllerName = stringBuilder.ToString();
                    var controllerColor = _createdControllers.Contains(inf.NameHash)
                                              ? Color.green
                                              : LightGray;
                    var scopeName = GetScopeName(inf.ScopeHash, frameData);
                    var startTime = inf.StartTimeMSec;
                    var color = GetColorByHash(inf.ScopeHash);
                    var controllerLiveData = new ControllerLiveData()
                    {
                        ControllerName = controllerName,
                        ControllerColor = controllerColor,
                        ScopeName = scopeName,
                        ScopeColor = color,
                        StartTimeMSec = inf.StartTimeMSec,
                        LifeTimeMSec = inf.ElapsedMilliseconds,
                    };
                    controllers.Add(controllerLiveData);
                }
            }
        }

        private Color GetColorByHash(int hash)
        {
            if (_contextColorMap.TryGetValue(hash, out var color))
            {
                return color;
            }

            color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), 0.5f, 0.5f);
            _contextColorMap[hash] = color;
            return color;
        }

        private void GetStoppedControllers(RawFrameDataView frameData, List<ControllerData> controllers)
        {
            var stopCount = frameData.GetFrameMetaDataCount(Helper.ControllerProfilerGuid, Helper.ControllerStopTag);
            if (stopCount == 0)
            {
                return;
            }

            var createdHeader = new ControllerData()
            {
                ControllerName = $"<color=red><b>Controllers stopped this frame</b></color> ({stopCount})",
                TimeData = "<b>Lifetime</b>"
            };
            controllers.Add(createdHeader);

            for (var i = 0; i < stopCount; i++)
            {
                using var stopControllerFrameDatas =
                    frameData.GetFrameMetaData<StopControllerFrameData>(
                        Helper.ControllerProfilerGuid, Helper.ControllerStopTag, i);

                if (stopControllerFrameDatas.Length == 0)
                {
                    continue;
                }

                foreach (var inf in stopControllerFrameDatas)
                {
                    var controllerName = GetControllerName(inf.NameHash, frameData);
                    var lifetime = inf.ElapsedMilliseconds;
                    var controllerData = new ControllerData()
                    {
                        ControllerName = controllerName,
                        TimeData = lifetime.ToString()
                    };
                    controllers.Add(controllerData);
                }
            }
        }

        private void GetCreatedControllers(RawFrameDataView frameData, List<ControllerData> controllers)
        {
            var createCount = frameData.GetFrameMetaDataCount(Helper.ControllerProfilerGuid, Helper.ControllerStartTag);
            if (createCount == 0)
            {
                return;
            }

            var createdHeader = new ControllerData()
            {
                ControllerName = $"<b><color=green>Controllers created this frame</color> ({createCount})</b>",
                TimeData = "<b>OnStart</b>"
            };
            controllers.Add(createdHeader);

            for (var i = 0; i < createCount; i++)
            {
                using var createControllerFrameData =
                    frameData.GetFrameMetaData<CreateControllerFrameData>(
                        Helper.ControllerProfilerGuid, Helper.ControllerStartTag, i);

                if (createControllerFrameData.Length == 0)
                {
                    continue;
                }

                foreach (var inf in createControllerFrameData)
                {
                    var controllerName = GetControllerName(inf.NameHash, frameData);
                    var startTime = inf.StartTimeMSec;
                    _createdControllers.Add(inf.NameHash);

                    var controllerData = new ControllerData()
                    {
                        ControllerName = controllerName,
                        TimeData = startTime > 0
                                           ? $"<color=red>{startTime.ToString()}</color>"
                                           : $"<color=#808080>{startTime.ToString()}</color>"
                    };
                    controllers.Add(controllerData);
                }
            }

            var createdSeparator = new ControllerData()
            {
                ControllerName = "",
                TimeData = ""
            };
            _createdAndDestroyedControllersList.Add(createdSeparator);
        }

        private void OnSelectedFrameIndexChanged(long selectedFrameIndex)
        {
            ReloadData(selectedFrameIndex);
        }
    }
}
#endif
