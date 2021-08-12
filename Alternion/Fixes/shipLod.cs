using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace Alternion.Fixes
{
    class shipLod
    {
        [HarmonyPatch(typeof(ShipLOD), "Update")]
        class shipLodUpdatePatch
        {
            static bool Prefix(ShipLOD __instance)
            {
                if (__instance.transform.parent == null)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
