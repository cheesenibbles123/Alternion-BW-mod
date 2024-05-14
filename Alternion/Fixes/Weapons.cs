using Harmony;
using UnityEngine;

namespace Alternion.Fixes
{
    [HarmonyPatch(typeof(WeaponHandler), "setAiming")]
    class SetAimingPatch
    {
        static bool Prefix(WeaponHandler __instance, bool ñèìóêëíìòíè, string ïèåæóåóêåóå)
        {
            PlayerInfo plyrInf = Traverse.Create(__instance).Field("æðíèæðóæíæå").GetValue() as PlayerInfo;
            if (plyrInf && plyrInf.character)
            {
                return true;
            }

            // Ensure bool status is maintained
            __instance.ïðìðçòçíðóë = ñèìóêëíìòíè;
            return false;
        }
    }

    [HarmonyPatch(typeof(KillogFeed), "íïòéóéêêìèè")]
    class KillogFeedPatch
    {
        static bool Prefix(PlayerInfo èíëçòåëçäìð, PlayerInfo îíïòçñéëíîé, int ïïîíäêèééíñ)
        {
            return èíëçòåëçäìð && îíïòçñéëíîé;
        }
    }

    /*
    [HarmonyPatch(typeof(CannonUse), "clearCannon")]
    class ClearCannonPatch
    {
        static bool Prefix(CannonUse __instance, ïçîìäîóäìïæ.åéðñðçîîïêç äíìíëðñïñéè)
        {
            if (äíìíëðñïñéè.åéñëîíèðòçé)
            {
                Traverse cannonUse = Traverse.Create(__instance);
                __instance.íëæéòñíóòëê = false;
                __instance.éìîñðîæêïäð = çëèèñêëðïìó.ìëððïðäîæäé;
                __instance.ìçóéóïìñçíë.localRotation = Quaternion.Euler(__instance.ñåóëóåîðèêé);
                __instance.transform.localPosition = cannonUse.Field("ìéìêîòææïíï").GetValue() as Vector3;
                cannonUse.Method("setStep", 0);
            }
            return false;
        }
    }*/

#if DEBUG
    [HarmonyPatch(typeof(ShipHealth), "updateShipGrappled")]
    class UpdateGrappledStatePatch
    {
        static bool Prefix(ShipHealth __instance, bool èîìíðïóèììå)
        {
            if (!__instance.êæòðóîíìîäò) return false;
            return true;
        }
    }
#endif
}
