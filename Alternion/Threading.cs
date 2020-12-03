using System;
using System.Net;
using System.Threading;
using Harmony;
using UnityEngine;

namespace Alternion
{
    class ThreadCreationProgram : MonoBehaviour
    {

        void Start() {
            debugLog("Started threader object");
        }
        static void debugLog(string message)
        {
            //Just easier to type than Log.logger.Log
            //Will always log, so only use in try{} catch(Exception e) {} when absolutely needed
            Log.logger.Log(message);
        }

        [HarmonyPatch(typeof(PlayerInfo), "setupRemotePlayer")]
        public static class getPlayerPatch
        {
            static void Postfix(TeamSelect __instance, string pName, int t, int s, int k, int d, int a, bool back, string dStat, int dMedal, int ks, float kd, short wins, float wl, short bm, short sm, short gm)
            {
                try
                {
                    debugLog("Entered patch");
                    debugLog(pName);
                    PlayerInfo plrInf = GameMode.getPlayerInfo(pName);
                    debugLog("Gotten playerInfo");
                    string steamID = plrInf.steamID.ToString();
                    debugLog("In Patch: Creating the Child thread");
                    Thread childThread = new Thread(() => ChildThreadJoinIngame(steamID));
                    debugLog("Starting thread");
                    childThread.Start();
                    debugLog("Thread Started");
                }catch (Exception e)
                {
                    debugLog(e.Message);
                }
            }
        }

        public static void ChildThreadJoinIngame(string steamID)
        {
            debugLog("Child thread starts");
            debugLog($"Gotten steamID {steamID}");

            string response = new WebClient().DownloadString("http://www.archiesbots.com/BlackwakeStuff/" + "playerList.json");
            debugLog("Gotten response");
            string[] json = response.Split('&');

            for (int i = 0; i < json.Length; i++)
            {
                playerObject player = JsonUtility.FromJson<playerObject>(json[i]);
                debugLog("Parsed json");
                if (player.steamID == steamID) {

                    debugLog("Found user");
                    if (theGreatCacher.players.ContainsKey(steamID))
                    {
                        debugLog("Updated user");
                        theGreatCacher.players[steamID] = player;
                        debugLog(theGreatCacher.players[steamID].badgeName);
                    }
                    else
                    {
                        debugLog("Added user");
                        theGreatCacher.players.Add(player.steamID, player);
                    }
                    debugLog("Fetched new player");
                    break;
                }
            }
            debugLog("Finished.");
        }
    }
}
