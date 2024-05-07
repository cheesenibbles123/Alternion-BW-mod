using Harmony;
using System;
using System.Linq;
using UnityEngine;

namespace Alternion.SkinHandlers
{
    public static class DebugLib
    {
        private static Logger logger = new Logger("[DebugLib]");

        public static void outputParentTransformPath(Transform item, bool showParentIndex = false)
        {
            string[] output = new string[] { item.name };
            Transform transf = item;
            while (transf.parent)
            {
                string parentIndex = showParentIndex ? "(" + GameMode.getParentIndex(transf) + ")" : "";
                logger.debugLog("Got entry: " + transf.name + parentIndex);
                output.AddToArray(transf.name);
                transf = transf.parent;
            }
            logger.debugLog("Parent path: " + String.Join(" > ", output.Reverse().ToArray()));
        }

        public static void outputChildrenTree(Transform parent, string prefix = "", bool showParentIndex = false)
        {
            int parentCount = parent.childCount;
            for (int i = 0; i < parentCount; i++)
            {
                string parentIndex = showParentIndex ? "(" + GameMode.getParentIndex(parent.GetChild(i)) + ")" : "";
                logger.debugLog(prefix + parent.name + parentIndex);
                outputChildrenTree(parent.GetChild(i), prefix += "-");
            }
        }
    }
}
