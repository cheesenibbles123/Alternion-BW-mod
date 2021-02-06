using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BWModLoader;
using UnityEngine;

namespace Alternion
{
    [Mod]
    public class SailHandler : MonoBehaviour
    {

        [HarmonyPatch(typeof(SailHealth), "OnEnable")]
        static class sailSkinPatch
        {
            static void Postfix(SailHealth __instance)
            {
                if (AlternionSettings.useMainSails || AlternionSettings.useSecondarySails)
                {
                    try
                    {
                        Transform shipTransf = __instance.transform.root;
                        if (shipTransf)
                        {
                            int teamNum = int.Parse(shipTransf.name.Split('m')[1]);

                            string steamID = GameMode.Instance.teamCaptains[teamNum - 1].steamID.ToString();

                            if (!theGreatCacher.players.TryGetValue(steamID, out playerObject player))
                            {
                                __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                return;
                            }

                            string shipType = GameMode.Instance.shipTypes[teamNum - 1];
                            shipType = shipType.Remove(shipType.Length - 1);


                            switch (shipType)
                            {
                                case "cruiser":
                                    if (__instance.name == "hmsSophie_sails08" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }


                                        if (player.mainSailName != "default")
                                        {

                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        if (theGreatCacher.defaultSails != null)
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }


                                    if (__instance.name != "hmsSophie_sails08")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "galleon":
                                    if (__instance.name == "galleon_sails_01" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }


                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        if (theGreatCacher.defaultSails != null)
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }

                                    if (__instance.name != "galleon_sails_01")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "brig":
                                    if (__instance.name == "hmsSpeedy_sails04" && AlternionSettings.useMainSails)
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "hmsSpeedy_sails04")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "xebec":
                                    if (__instance.name == "xebec_sail03" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "xebec_sail03")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "bombvessel":
                                    if (__instance.name == "bombVessel_sails07" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "bombVessel_sails07")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "gunboat":
                                    if (__instance.name == "gunboat_sails02" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "gunboat_sails02")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "cutter":
                                    if (__instance.name == "hmsAlert_sails02" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "hmsAlert_sails02")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "bombketch":
                                    if (__instance.name == "bombKetch_sails06" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "bombKetch_sails06")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "carrack":
                                    if (__instance.name == "carrack_sail03" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "carrack_sail03")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "junk":
                                    if (__instance.name == "junk_sails_01" && AlternionSettings.useMainSails)
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "junk_sails_01")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                case "schooner":
                                    if ((__instance.name == "schooner_sails02" && AlternionSettings.useMainSails) || (__instance.name == "schooner_sails00" && AlternionSettings.useMainSails))
                                    {

                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.mainSailDict.Add((vessel.mainSailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.mainSailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }

                                        if (player.mainSailName != "default")
                                        {
                                            if (theGreatCacher.mainSails.TryGetValue(player.mainSailName, out Texture mainSail))
                                            {
                                                __instance.GetComponent<Renderer>().material.mainTexture = mainSail;
                                            }
                                        }
                                        else
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                        }
                                    }
                                    else if (player.sailSkinName != "default" && AlternionSettings.useSecondarySails)
                                    {
                                        if (theGreatCacher.secondarySails.TryGetValue(player.sailSkinName, out Texture secondarySail))
                                        {
                                            __instance.GetComponent<Renderer>().material.mainTexture = secondarySail;
                                        }
                                    }
                                    else
                                    {
                                        __instance.GetComponent<Renderer>().material.mainTexture = theGreatCacher.defaultSails;
                                    }

                                    if (__instance.name != "schooner_sails02" && __instance.name != "schooner_sails00")
                                    {
                                        if (theGreatCacher.ships.TryGetValue(teamNum.ToString(), out cachedShip vessel))
                                        {
                                            vessel.sailDict.Add((vessel.sailDict.Count + 1).ToString(), __instance);
                                        }
                                        else
                                        {
                                            cachedShip newVessel = new cachedShip();
                                            newVessel.sailDict.Add("1", __instance);
                                            theGreatCacher.ships.Add(teamNum.ToString(), newVessel);
                                        }
                                    }

                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.debugLog(e.Message);
                    }
                }
            }
        }

    }
}
