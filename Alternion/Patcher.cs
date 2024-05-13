using Alternion.Fixes;
using Alternion.SkinHandlers;
using Alternion.Structs;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alternion
{

    class Helpers
    {
        public static Logger logger = new Logger("[Helpers]");

        /// <summary>
        /// Resets all ship assets to default textures. Cannons + Sails
        /// </summary>
        public static void resetAllShipsToDefault()
        {
            // Loop through all ships, and set all visuals to defaults in the following order:
            // Sails
            // Main Sails
            // Functioning cannons
            // Destroyed cannons
            // Swivels
            // Mortars
            foreach (KeyValuePair<string, cachedShip> individualShip in TheGreatCacher.Instance.ships)
            {
                if (individualShip.Value.hasChangedSails)
                {
                    foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.sailDict)
                    {
                        indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                        indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                    }

                    foreach (KeyValuePair<string, SailHealth> indvidualSail in individualShip.Value.mainSailDict)
                    {
                        indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                        indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                    }

                    individualShip.Value.hasChangedSails = false;
                }

                if (individualShip.Value.hasChangedCannons)
                {
                    for (int i = individualShip.Value.cannons.Count - 1; i >= 0; i--)
                    {
                        Renderer rend = individualShip.Value.cannons[i];
                        if (rend)
                        {
                            rend.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                            rend.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                            rend.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                        }
                        else
                        {
                            individualShip.Value.cannons.RemoveAt(i);
                        }
                    }

                    if (individualShip.Value.cannonLOD != null)
                    {
                        individualShip.Value.cannonLOD.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                        individualShip.Value.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                        individualShip.Value.cannonLOD.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                    }
                    individualShip.Value.hasChangedCannons = false;
                }

                if (individualShip.Value.hasChangedCannonModels)
                {
                    for (int i = individualShip.Value.cannons.Count - 1; i >= 0; i--)
                    {
                        MeshFilter rend = individualShip.Value.cannonModels[i];
                        if (rend)
                        {
                            rend.mesh = TheGreatCacher.Instance.defaultCannonMesh;
                        }
                        else
                        {
                            individualShip.Value.cannonModels.RemoveAt(i);
                        }
                    }
                    individualShip.Value.hasChangedCannonModels = false;
                }

                if (individualShip.Value.hasChangedFlag)
                {
                    individualShip.Value.flags.ForEach((flag) => flag.material.mainTexture = individualShip.Value.isNavy ? TheGreatCacher.Instance.navyFlag : TheGreatCacher.Instance.pirateFlag);
                    individualShip.Value.hasChangedFlag = false;
                }

                if (individualShip.Value.hasChangedSwivels)
                {
                    foreach (Renderer swivel in individualShip.Value.Swivels)
                    {
                        if (swivel != null)
                        {
                            swivel.material.mainTexture = TheGreatCacher.Instance.defaultSwivel;
                            swivel.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultSwivelMet);
                            swivel.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultSwivelNrm);
                        }
                    }
                    individualShip.Value.hasChangedSwivels = false;
                }

                if (individualShip.Value.hasChangedMortars)
                {
                    foreach (Renderer mortar in individualShip.Value.mortars)
                    {
                        if (mortar != null)
                        {
                            mortar.material.mainTexture = TheGreatCacher.Instance.defaultMortar;
                            mortar.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMortarMet);
                            mortar.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                        }
                    }
                    individualShip.Value.hasChangedMortars = false;
                }

            }
        }

        /// <summary>
    /// Sets the new textures for a cached ship.
    /// </summary>
    /// <param name="steamID">Captain SteamID</param>
    /// <param name="index">Ship Index</param>
        public static void assignNewTexturesToShips(string steamID, string index)
        {
            try
            {
                // Loop through the cached vessel and apply new textures in the following order:
                // Sails
                // Main Sails
                // Flags
                // Functional Cannons
                // Destroyed Cannons
                Texture newTex;
                if (TheGreatCacher.Instance.ships.TryGetValue(index, out cachedShip mightyVessel))
                {
                    if (TheGreatCacher.Instance.players.TryGetValue(steamID, out playerObject player))
                    {
                        // Only apply new texture if config has sail textures enabled
                        if (AlternionSettings.useSecondarySails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                if (TheGreatCacher.Instance.secondarySails.TryGetValue(player.sailSkinName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                    indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                                else
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                    indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                            }
                        }

                        // Only apply new texture if config has main sail textures enabled
                        if (AlternionSettings.useMainSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                if (TheGreatCacher.Instance.mainSails.TryGetValue(player.mainSailName, out newTex))
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = newTex;
                                    indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = newTex;
                                }
                                else
                                {
                                    indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                    indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                            }
                        }

                        if (AlternionSettings.useMainSails || AlternionSettings.useSecondarySails)
                        {
                            sailHandler.Instance.handleClosedSails(mightyVessel, player);
                        }
                        else
                        {
                            sailHandler.Instance.resetClosedSails(mightyVessel);
                        }

                        // Only apply new texture if config has cannon textures enabled
                        if (AlternionSettings.useCannonSkins)
                        {
                            TheGreatCacher.Instance.skinAttributes.TryGetValue("cannon_" + player.cannonSkinName, out weaponSkinAttributes attrib);

                            Texture mainTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                            {
                                mainTex = newTex;
                            }
                            else { mainTex = TheGreatCacher.Instance.defaultCannons; }
                            Texture metTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                            {
                                metTex = newTex;
                            }
                            else { metTex = TheGreatCacher.Instance.defaultCannonsMet; }
                            Texture nrmTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_nrm", out newTex))
                            {
                                nrmTex = newTex;
                            }
                            else { nrmTex = TheGreatCacher.Instance.defaultCannonsNrm; }

                            for (int i = mightyVessel.cannons.Count - 1; i >= 0; i--)
                            {
                                Renderer rend = mightyVessel.cannons[i];
                                if (rend)
                                {
                                    rend.material.mainTexture = mainTex;
                                    rend.material.SetTexture("_Metallic", metTex);
                                    rend.material.SetTexture("_BumpMap", nrmTex);
                                }
                                else
                                {
                                    mightyVessel.cannons.RemoveAt(i);
                                    i--;
                                }
                            }

                            bool hasAppliedNewSkin = false;

                            // LOD
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                            {
                                mightyVessel.cannonLOD.material.mainTexture = newTex;
                                hasAppliedNewSkin = true;
                            }
                            else
                            {
                                mightyVessel.cannonLOD.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                            }

                            if (attrib.hasMet && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_Metallic", newTex);
                                hasAppliedNewSkin = true;
                            }
                            else
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                            }

                            if (attrib.hasNrm && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_nrm", out newTex))
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_BumpMap", newTex);
                                hasAppliedNewSkin = true;
                            }
                            else
                            {
                                mightyVessel.cannonLOD.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultCannonsNrm);
                            }
                            mightyVessel.hasChangedCannons = hasAppliedNewSkin;
                        }
                        else if (mightyVessel.hasChangedCannons)
                        {
                            foreach (Renderer cannon in mightyVessel.cannons)
                            {
                                cannon.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                                cannon.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                                cannon.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultCannonsNrm);
                            }

                            mightyVessel.cannonLOD.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                            mightyVessel.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                            mightyVessel.hasChangedCannons = false;
                        }

                        if (AlternionSettings.useCannonSkins)
                        {
                            Mesh cannon;
                            if (TheGreatCacher.Instance.cannonModels.TryGetValue(player.cannonSkinName, out Mesh cannonMesh))
                            {
                                cannon = cannonMesh;
                            }
                            else { cannon = TheGreatCacher.Instance.defaultCannonMesh; }

                            for (int i = mightyVessel.cannonModels.Count - 1; i >= 0; i--)
                            {
                                MeshFilter meshFilter = mightyVessel.cannonModels[i];
                                if (meshFilter)
                                {
                                    meshFilter.mesh = cannon;
                                }
                                else
                                {
                                    mightyVessel.cannonModels.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                        else if (mightyVessel.hasChangedCannonModels)
                        {
                            foreach (MeshFilter meshFilter in mightyVessel.cannonModels)
                            {
                                meshFilter.mesh = TheGreatCacher.Instance.defaultCannonMesh;
                            }

                            mightyVessel.hasChangedCannonModels = false;
                        }

                        // Only apply new texture if config has flag textures enabled
                        if (AlternionSettings.showFlags)
                        {
                            string flagSkin = mightyVessel.isNavy ? player.flagNavySkinName : player.flagPirateSkinName;
                            if (flagSkin != "default" && TheGreatCacher.Instance.flags.TryGetValue(flagSkin, out newTex))
                            {
                                flagHandler.setFlagTextures(mightyVessel, newTex);
                            }
                            else
                            {
                                flagHandler.resetFlag(mightyVessel);
                            }
                        }
                        else if (mightyVessel.hasChangedFlag)
                        {
                            flagHandler.resetFlag(mightyVessel);
                        }

                        // Only apply new texture if config has swivel textures enabled
                        if (AlternionSettings.useSwivelSkins)
                        {
                            swivelHandler.Instance.updateSwivels(mightyVessel, player);
                        }
                        else
                        {
                            swivelHandler.Instance.resetSwivels(mightyVessel);
                        }

                        // Only apply new texture if config has mortar textures enabled
                        if (AlternionSettings.useMortarSkins)
                        {
                            TheGreatCacher.Instance.skinAttributes.TryGetValue("cannon_" + player.cannonSkinName, out weaponSkinAttributes attrib);
                            Texture mainTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName, out newTex))
                            {
                                mainTex = newTex;
                            }
                            else { mainTex = TheGreatCacher.Instance.defaultMortar; }
                            Texture metTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_met", out newTex))
                            {
                                metTex = newTex;
                            }
                            else { metTex = TheGreatCacher.Instance.defaultMortarMet; }
                            Texture nrmTex;
                            if (attrib.hasAlb && TheGreatCacher.Instance.cannonSkins.TryGetValue(player.cannonSkinName + "_nrm", out newTex))
                            {
                                nrmTex = newTex;
                            }
                            else { nrmTex = TheGreatCacher.Instance.defaultMortarNrm; }

                            for (int i = 0; i < mightyVessel.mortars.Count - 1; i++)
                            {
                                if (!mightyVessel.mortars[i]) { continue; }
                                mightyVessel.mortars[i].material.mainTexture = mainTex;
                                mightyVessel.mortars[i].material.SetTexture("_Metallic", metTex);
                                mightyVessel.mortars[i].material.SetTexture("_BumpMap", nrmTex);

                            }
                        }
                        else if (mightyVessel.hasChangedMortars)
                        {
                            for (int i = 0; i < mightyVessel.mortars.Count; i++)
                            {
                                if (!mightyVessel.mortars[i]) { continue; }
                                mightyVessel.mortars[i].material.mainTexture = TheGreatCacher.Instance.defaultMortar;
                                mightyVessel.mortars[i].material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMortarMet);
                                mightyVessel.mortars[i].material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                            }

                            mightyVessel.hasChangedMortars = false;
                        }
                    }
                    else
                    {
                        if (mightyVessel.hasChangedCannons)
                        {
                            for (int i = mightyVessel.cannons.Count - 1; i >= 0; i--)
                            {
                                Renderer rend = mightyVessel.cannons[i];
                                if (rend)
                                {
                                    rend.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                                    rend.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                                    rend.material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultCannonsNrm);
                                }
                                else
                                {
                                    mightyVessel.cannons.RemoveAt(i);
                                    i--;
                                }
                            }

                            mightyVessel.cannonLOD.material.mainTexture = TheGreatCacher.Instance.defaultCannons;
                            mightyVessel.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsMet);
                            mightyVessel.cannonLOD.material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultCannonsNrm);

                            mightyVessel.hasChangedCannons = false;
                        }
                        if (mightyVessel.hasChangedCannonModels)
                        {
                            for (int i = mightyVessel.cannonModels.Count - 1; i >= 0; i--)
                            {
                                if (mightyVessel.cannonModels[i])
                                {
                                    mightyVessel.cannonModels[i].mesh = TheGreatCacher.Instance.defaultCannonMesh;
                                }
                            }
                        }
                        if (mightyVessel.hasChangedSails)
                        {
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.mainSailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                            }
                            foreach (KeyValuePair<string, SailHealth> indvidualSail in mightyVessel.sailDict)
                            {
                                indvidualSail.Value.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                                indvidualSail.Value.êæïäîæïïíñå.GetComponent<Renderer>().material.mainTexture = TheGreatCacher.Instance.defaultSails;
                            }

                            sailHandler.Instance.resetClosedSails(mightyVessel);

                            mightyVessel.hasChangedSails = false;
                        }
                        flagHandler.resetFlag(mightyVessel);
                        swivelHandler.Instance.resetSwivels(mightyVessel);
                        if (mightyVessel.hasChangedMortars)
                        {
                            for (int i = 0; i < mightyVessel.mortars.Count; i++)
                            {
                                if (!mightyVessel.mortars[i]) { continue; }
                                mightyVessel.mortars[i].material.mainTexture = TheGreatCacher.Instance.defaultMortar;
                                mightyVessel.mortars[i].material.SetTexture("_Metallic", TheGreatCacher.Instance.defaultMortarMet);
                                mightyVessel.mortars[i].material.SetTexture("_BumpMap", TheGreatCacher.Instance.defaultMortarNrm);
                            }

                            mightyVessel.hasChangedMortars = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.logLow(e.Message);
            }
        }

    }


    /// <summary>
    /// Harmony patch to trigger resetting of ships
    /// </summary>
    [HarmonyPatch(typeof(GameMode), "newRound")]
    class newRoundPatch
    {
        static void Postfix(GameMode __instance)
        {
            // Reset all ship skins that are cached on newRound() to default textures
            Helpers.resetAllShipsToDefault();
            LevelProtect.Instance.roundEnd();
        }
    }

    /// <summary>
    /// Harmony patch to setup ships on captain pass
    /// </summary>
    [HarmonyPatch(typeof(PlayerOptions), "passCaptain")]
    class passCaptainPatch
    {
        static void Prefix(PlayerOptions __instance)
        {
            // Untested
            try
            {
                PlayerInfo player = GameMode.getPlayerInfo(__instance.êåééóæåñçòì.text);
                Helpers.assignNewTexturesToShips(player.steamID.ToString(), player.team.ToString());
            }
            catch (Exception e)
            {
                Helpers.logger.debugLog("[passCaptainPatch]: " + e.Message);
            }
        }
    }
}
