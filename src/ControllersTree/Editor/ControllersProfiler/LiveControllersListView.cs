#if UNITY_CONTROLLERS_PROFILER
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace Modules.Controller.Editor
{
    internal class LiveControllersListView : ListView
    {
        private const float ItemHeight = 16f;

        internal LiveControllersListView(List<ControllerLiveData> controllers)
            : base()
        {
            itemsSource = controllers;
            Setup();
            Rebuild();
        }

        internal static VisualElement FillLifetimeListHeader()
        {
            var row = MakeLifetimeListItem();
            row.style.height = ItemHeight * 2;
            var controllerNameLabel = row[0] as Label;
            var scopeNameLabel = row[1] as Label;
            var onStartTimeLabel = row[2] as Label;
            var lifetimeLabel = row[3] as Label;

            controllerNameLabel.text = "Controller name";
            ApplyLabelStyle(controllerNameLabel);
            scopeNameLabel.text = "Scope name";
            ApplyLabelStyle(scopeNameLabel);
            onStartTimeLabel.text = "OnStart\n(MSec)";
            ApplyLabelStyle(onStartTimeLabel);
            onStartTimeLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            lifetimeLabel.text = "Lifetime\n(MSec)";
            ApplyLabelStyle(lifetimeLabel);
            lifetimeLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            lifetimeLabel.style.width = 74;

            return row;
        }

        private void Setup()
        {
            fixedItemHeight = ItemHeight;
            makeItem = MakeLifetimeListItem;
            bindItem = BindItem;
            style.flexGrow = 1.0f;
        }

        private static VisualElement MakeLifetimeListItem()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;

            var col1 = new Label();
            col1.style.flexGrow = 1;
            col1.style.marginRight = 4;
            col1.style.overflow = Overflow.Hidden;
            col1.style.whiteSpace = WhiteSpace.NoWrap;

            var col2 = new Label();
            col2.style.width = 150;
            col2.style.unityTextAlign = TextAnchor.MiddleCenter;
            col2.style.marginRight = 4;
            col2.style.overflow = Overflow.Hidden;
            col2.style.whiteSpace = WhiteSpace.NoWrap;

            var col3 = new Label();
            col3.style.width = 60;
            col3.style.marginRight = 4;

            var col4 = new Label();
            col4.style.width = 60;

            row.Add(col1);
            row.Add(col2);
            row.Add(col3);
            row.Add(col4);

            return row;
        }

        private void BindItem(VisualElement row, int index)
        {
            if (itemsSource == null || index >= itemsSource.Count)
            {
                return;
            }

            var controllerNameLabel = row[0] as Label;
            var scopeNameLabel = row[1] as Label;
            var onStartTimeLabel = row[2] as Label;
            var lifetimeLabel = row[3] as Label;

            var item = itemsSource[index] as ControllerLiveData;

            controllerNameLabel.text = item.ControllerName;
            controllerNameLabel.style.color = item.ControllerColor;

            scopeNameLabel.text = item.ScopeName;
            scopeNameLabel.style.backgroundColor = item.ScopeColor;

            var startTime = item.StartTimeMSec;
            onStartTimeLabel.text = startTime.ToString();
            onStartTimeLabel.style.color = startTime > 0
                                   ? Color.red
                                   : Color.gray;

            lifetimeLabel.text = item.LifeTimeMSec.ToString();
        }

        private static void ApplyLabelStyle(Label label)
        {
            label.style.color = Color.white;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.height = ItemHeight * 2f;
            label.style.alignSelf = Align.Center;
            label.style.justifyContent = Justify.Center;
        }
    }
}
#endif
