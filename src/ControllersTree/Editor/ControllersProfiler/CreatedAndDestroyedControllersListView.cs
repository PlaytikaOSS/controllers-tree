#if UNITY_CONTROLLERS_PROFILER
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Modules.Controller.Editor
{
    internal class CreatedAndDestroyedControllersListView : ListView
    {
        private const float ItemHeight = 16f;

        internal CreatedAndDestroyedControllersListView(List<ControllerData> controllers)
            : base()
        {
            itemsSource = controllers;
            Setup();
            Rebuild();
        }

        private void Setup()
        {
            fixedItemHeight = ItemHeight;
            makeItem = MakeStartStopListItem;
            bindItem = BindItem;
            style.flexGrow = 1.0f;
        }

        private void BindItem(VisualElement row, int index)
        {
            if (itemsSource == null || index >= itemsSource.Count)
            {
                return;
            }

            var controllerNameLabel = row[0] as Label;
            var timeDataLabel = row[1] as Label;

            var item = itemsSource[index] as ControllerData;

            controllerNameLabel.text = item.ControllerName;
            timeDataLabel.text = item.TimeData;
        }

        private static VisualElement MakeStartStopListItem()
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
            col2.style.width = 60;
            col2.style.marginRight = 4;
            row.Add(col1);
            row.Add(col2);

            return row;
        }
    }
}
#endif