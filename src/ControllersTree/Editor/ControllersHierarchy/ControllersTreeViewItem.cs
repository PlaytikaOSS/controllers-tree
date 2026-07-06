using System;
using System.Linq;
using System.Reflection;
using Playtika.Controllers;
using UnityEditor.IMGUI.Controls;

#if UNITY_6000_0_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#endif

namespace Playtika.Controllers.Editor
{
    internal class ControllersTreeViewItem : TreeViewItem
    {
        private readonly WeakReference _controllerWeakReference;

        internal IControllerDebugInfo ControllerDebugInfo
        {
            get
            {
                if (_controllerWeakReference.IsAlive)
                {
                    return (IControllerDebugInfo)_controllerWeakReference.Target;
                }

                return null;
            }
        }

        internal ControllersTreeViewItem(
            int id,
            int depth,
            IControllerDebugInfo controllerDebugInfo)
            : base(id, depth, controllerDebugInfo.ToString())
        {
            _controllerWeakReference = new WeakReference(controllerDebugInfo);
        }
    }
}
