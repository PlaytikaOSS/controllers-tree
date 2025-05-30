using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Playtika.Controllers;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Playtika.Controllers.Editor
{
    public class ControllersHierarchy : EditorWindow
    {
        [MenuItem("Tools/Controllers/Controllers Hierarchy", false, 102)]
        private static void ShowWindow()
        {
            GetWindow<ControllersHierarchy>("Controllers Hierarchy");
        }
        
        private const string UnityControllerProfilerDefine = "UNITY_CONTROLLERS_PROFILER";
#if UNITY_CONTROLLERS_PROFILER
        [MenuItem("Tools/Controllers/Disable UNITY_CONTROLLERS_PROFILER define", priority = 103)]
        public static void DisableUnityControllerProfilerDefine()
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            var definesList = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            definesList.Remove(UnityControllerProfilerDefine);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, definesList.ToArray());
        }
#else
        [MenuItem("Tools/Controllers/Enable UNITY_CONTROLLERS_PROFILER define", priority = 103)]
        public static void EnableUnityControllerProfilerDefine()
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            var definesList = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            definesList.Add(UnityControllerProfilerDefine);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, definesList.ToArray());
        }
#endif

        private ControllersTreeViewModel _model;
        private ControllersTabBarDrawer _tabBarDrawer;
        private ControllersHierarchyView _rootHierarchy;
        private MultiColumnHeader _multiColumnHeader;

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            _model ??= new ControllersTreeViewModel();
            _model.CreateNewTreeTab += OnCreateNewTab;
            _model.LoadGuiSkin();

            InitializeMultiColumns();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            _model.CreateNewTreeTab -= OnCreateNewTab;
        }

        private void InitializeMultiColumns()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = 300.0f,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    headerContent = new GUIContent("Name", "A name of a controller."),
                    headerTextAlignment = TextAlignment.Left,
                },
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    minWidth = 150.0f,
                    maxWidth = 200.0f,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    headerContent = new GUIContent("State", "A state of a controller."),
                    headerTextAlignment = TextAlignment.Center,
                },
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    minWidth = 200.0f,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    headerContent = new GUIContent("Scope", "A Scope of the controller it belongs to."),
                    headerTextAlignment = TextAlignment.Center,
                },
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    minWidth = 150.0f,
                    maxWidth = 150.0f,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    headerContent = new GUIContent("Type", "A type of a controller."),
                    headerTextAlignment = TextAlignment.Center,
                }
            };

            var multiColumnHeaderState = new MultiColumnHeaderState(columns);
            _multiColumnHeader = new MultiColumnHeader(multiColumnHeaderState);
            _multiColumnHeader.height = 20;
            _multiColumnHeader.visibleColumnsChanged += (multiColumnHeader) => multiColumnHeader.ResizeToFit();
            _multiColumnHeader.ResizeToFit();
        }

        private void OnCreateNewTab(IControllerDebugInfo controller)
        {
            var treeView = new ControllersHierarchyView(new ControllerTreeView(new TreeViewState(), _multiColumnHeader, controller, _model), _model);
            _tabBarDrawer.AddData(new ControllerTabData(controller.GetType().Name, controller.ToString(), true, treeView.Draw, treeView.Reload));
        }

        private void OnGUI()
        {
            TryInitialize();
            if (_rootHierarchy == null)
            {
                EditorGUILayout.HelpBox("Not founded root controller", MessageType.None);
                GUILayout.FlexibleSpace();
            }
            else
            {
                _model.SetWindowSize(position.size);
                _tabBarDrawer.Draw();
            }

            DrawBottomPart();
        }

        private void TryInitialize()
        {
            var rootController = RootController.Instance;
            if (rootController != null && _rootHierarchy == null)
            {
                var treeView = new ControllerTreeView(new TreeViewState(), _multiColumnHeader, rootController, _model);
                _rootHierarchy = new ControllersHierarchyView(treeView, _model);
                var tabData = new ControllerTabData("ROOT", rootController.ToString(), false, _rootHierarchy.Draw, _rootHierarchy.Reload);
                _tabBarDrawer = new ControllersTabBarDrawer(_model, tabData);
                _multiColumnHeader.ResizeToFit();

                if (rootController is IControllerDebugInfo rootControllerDebugInfo)
                {
                    rootControllerDebugInfo.AddDisposable(new DisposableToken(() => _rootHierarchy = null));
                }

                var ignoredMethods = new List<string>();
                var methods = rootController.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttributes(typeof(DebugMethodAttribute), true).SingleOrDefault();
                    if (method.GetParameters().Length == 0 && attribute == null)
                    {
                        ignoredMethods.Add(method.Name);
                    }
                }

                _model.SetIgnoredMethods(ignoredMethods.ToArray());
            }
        }

        private void DrawBottomPart()
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = ControllersTreeHelper.DarkGrayColor;
            using (new GUILayout.HorizontalScope(_model.ToolbarStyle, GUILayout.Height(30)))
            {
                GUI.backgroundColor = Color.clear;
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("Infos", "Info panel")))
                {
                    _model.SwitchBottomPanelState();
                }

                if (GUILayout.Button(new GUIContent("Methods", "Method panel")))
                {
                    _model.SwitchRightPanelState();
                }
            }

            GUI.backgroundColor = backgroundColor;

            var rect = GUILayoutUtility.GetLastRect();
            rect.height = 1f;
            EditorGUI.DrawRect(rect, ControllersTreeHelper.DividingLineColor);
        }

        private void Update()
        {
            _tabBarDrawer?.Reload();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            TryInitialize();
        }
    }
}
