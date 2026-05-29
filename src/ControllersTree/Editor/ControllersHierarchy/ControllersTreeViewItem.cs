using System;
using System.Linq;
using System.Reflection;
using Playtika.Controllers;
using UnityEditor.IMGUI.Controls;

namespace Playtika.Controllers.Editor
{
    internal class ControllersTreeViewItem : TreeViewItem<int>
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
