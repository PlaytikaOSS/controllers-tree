using System;
using System.Collections.Generic;
using System.Reflection;
using Playtika.Controllers;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Playtika.Controllers.Editor
{
    public class ControllerTreeView : TreeView<int>
    {
        private readonly IControllerDebugInfo _rootController;
        private readonly ControllersTreeViewModel _model;
        private readonly ControllersMethodsView _methodsView;
        private readonly ControllersInfoView _infoView;
        private GUIStyle _labelStyle;

        internal ControllerTreeView(
            TreeViewState<int> state,
            MultiColumnHeader multiColumnHeader,
            IControllerDebugInfo rootController,
            ControllersTreeViewModel model)
            : base(state, multiColumnHeader)
        {
            _rootController = rootController;
            _model = model;
            Reload();
            _methodsView = new ControllersMethodsView(_model);
            _infoView = new ControllersInfoView(_model);
        }

        public void DrawRightPanel()
        {
            _methodsView.DrawMethods();
        }

        public void DrawBottomPanel()
        {
            _infoView.DrawInfos();
        }

        protected override TreeViewItem<int> BuildRoot()
        {
            var root = new TreeViewItem<int>(0, -1, "Root");
            root.children = new List<TreeViewItem<int>>();

            if (_rootController != null)
            {
                var nextId = 0;
                var firstItem = new ControllersTreeViewItem(nextId, 0, _rootController);

                firstItem.children = InitializeTree(firstItem, _rootController, 1, ref nextId);
                firstItem.parent = root;

                root.children.Add(firstItem);
            }

            _labelStyle = new GUIStyle(EditorStyles.label);
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.alignment = TextAnchor.MiddleCenter;

            showAlternatingRowBackgrounds = true;
            showBorder = true;

            return root;
        }

        private List<TreeViewItem<int>> InitializeTree(
            ControllersTreeViewItem root,
            IControllerDebugInfo controllerDebugInfo,
            int depth,
            ref int nextId)
        {
            var childItems = new List<TreeViewItem<int>>();
            foreach (var childController in controllerDebugInfo)
            {
                var item = new ControllersTreeViewItem(++nextId, depth, childController);

                item.children = InitializeTree(item, childController, depth + 1, ref nextId);
                item.parent = root;

                childItems.Add(item);
            }

            return childItems;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var rowRect = args.rowRect;
            if (args.item is not ControllersTreeViewItem viewItem)
            {
                return;
            }

            if (viewItem.ControllerDebugInfo == null)
            {
                return;
            }

            DrawNameColumn(args, viewItem, rowRect);
            DrawStateColumn(viewItem, rowRect);
            DrawScopeColumn(viewItem, rowRect);
            DrawTypeColumn(viewItem, rowRect);
        }

        private void DrawNameColumn(
            RowGUIArgs args,
            ControllersTreeViewItem viewItem,
            Rect rowRect)
        {
            const int columnIndex = 0;
            if (multiColumnHeader.IsColumnVisible(columnIndex))
            {
                var visibleColumnIndex = multiColumnHeader.GetVisibleColumnIndex(columnIndex);
                var columnRect = multiColumnHeader.GetColumnRect(visibleColumnIndex);
                columnRect.y = rowRect.y;

                args.rowRect = columnRect;
                base.RowGUI(args);
                var buttonRect = columnRect;
                var center = buttonRect.center;
                buttonRect.width = 40;
                buttonRect.height -= 6;
                center.x = args.rowRect.width - buttonRect.width / 2;
                center.y -= 2;
                buttonRect.center = center;
                if (GUI.Button(buttonRect, "Pin", GUI.skin.button))
                {
                    _model.InvokeCreateNewTreeTab(viewItem.ControllerDebugInfo);
                }

                center = buttonRect.center;
                center.x = args.rowRect.width - buttonRect.width * 1.5f;
                buttonRect.center = center;
                if (GUI.Button(buttonRect, "Edit", GUI.skin.button))
                {
                    EditScript(args.item.id);
                }
            }
        }

        private void EditScript(int id)
        {
            var item = FindItem(id, rootItem);

            if (item is not ControllersTreeViewItem viewItem)
            {
                return;
            }

            var controller = viewItem.ControllerDebugInfo;
            var controllerName = controller.GetType().Name;
            var guids = AssetDatabase.FindAssets($"t:Script {controllerName}");
            for (var i = 0; i < guids.Length; ++i)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.Contains(controllerName))
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset);
                    return;
                }
            }
        }

        private void DrawStateColumn(ControllersTreeViewItem viewItem, Rect rowRect)
        {
            const int columnIndex = 1;
            if (multiColumnHeader.IsColumnVisible(columnIndex))
            {
                var visibleColumnIndex = multiColumnHeader.GetVisibleColumnIndex(columnIndex);
                var columnRect = multiColumnHeader.GetColumnRect(visibleColumnIndex);
                columnRect.y = rowRect.y;

                var backgroundRect = columnRect;
                backgroundRect.height = rowHeight - 2;
                backgroundRect.width -= 6;
                var backgroundCenter = backgroundRect.center;
                backgroundCenter.x += 3;
                backgroundCenter.y += 1;
                backgroundRect.center = backgroundCenter;

                var textRect = columnRect;
                var textCenter = columnRect.center;
                textCenter.y -= 3;
                textRect.center = textCenter;

                EditorGUI.DrawRect(backgroundRect, viewItem.ControllerDebugInfo.StateColor);
                EditorGUI.LabelField(textRect, viewItem.ControllerDebugInfo.StateName, _labelStyle);
            }
        }

        private void DrawScopeColumn(ControllersTreeViewItem viewItem, Rect rowRect)
        {
            const int columnIndex = 2;
            if (multiColumnHeader.IsColumnVisible(columnIndex))
            {
                var visibleColumnIndex = multiColumnHeader.GetVisibleColumnIndex(columnIndex);
                var columnRect = multiColumnHeader.GetColumnRect(visibleColumnIndex);
                columnRect.y = rowRect.y;

                var textRect = columnRect;
                var textCenter = columnRect.center;
                textCenter.y -= 3;
                textRect.center = textCenter;

                EditorGUI.LabelField(textRect, viewItem.ControllerDebugInfo.ScopeName, _labelStyle);
            }
        }

        private void DrawTypeColumn(ControllersTreeViewItem viewItem, Rect rowRect)
        {
            const int columnIndex = 3;
            if (multiColumnHeader.IsColumnVisible(columnIndex))
            {
                var visibleColumnIndex = multiColumnHeader.GetVisibleColumnIndex(columnIndex);
                var columnRect = multiColumnHeader.GetColumnRect(visibleColumnIndex);
                columnRect.y = rowRect.y;

                var textRect = columnRect;
                var textCenter = columnRect.center;
                textCenter.y -= 3;
                textRect.center = textCenter;

                EditorGUI.LabelField(textRect, viewItem.ControllerDebugInfo.ControllerType, _labelStyle);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            if (selectedIds != null && selectedIds.Count > 0)
            {
                SelectedItem(selectedIds[0]);
            }
        }

        private void SelectedItem(int id)
        {
            var item = FindItem(id, rootItem);

            if (item is not ControllersTreeViewItem viewItem)
            {
                return;
            }

            var controller = viewItem.ControllerDebugInfo;
            _methodsView.SelectedController(controller);
            _infoView.SelectedController(controller);
        }
    }
}
