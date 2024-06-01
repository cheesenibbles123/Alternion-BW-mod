using Harmony;
using BWModLoader;
using UnityEngine;
using System.Threading;
using Steamworks;

namespace Alternion.Fixes
{
    [Mod]
    public class LevelProtect : MonoBehaviour
    {
        private static Logger logger = new Logger("[LevelProtect]");

        public static readonly string scoreStat = "stat_score";

        public static LevelProtect Instance;
        static bool shouldResetNumber = false;

        readonly int maxPossibleScore = 10000; // 10K max per min, if you get more than that I don't know what you are doing
        readonly int secondsBetweenPush = 60;
        static int currentScore;
        static int scoreToAdd = 0;
        static int totalRoundScore = 0;

        void Awake()
        {
            if (!Instance) {
                Instance = this;
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        void Start()
        {
            Thread thread = new Thread(checkScoreBeforePush);
            thread.IsBackground = true;
            thread.Start();

            currentScore = íëåäòéðåïîé.åêìóëîñçîèì(scoreStat);
            logger.logLow("Got base score: " + currentScore);
        }

        public void roundEnd()
        {
            totalRoundScore = 0;
        }

        private void checkScoreBeforePush()
        {
            while (true) // On secondary thread so it's probably ok ;P
            {
                if (scoreToAdd > 0)
                {
                    if (scoreToAdd < maxPossibleScore)
                    {
                        currentScore += scoreToAdd;
                        totalRoundScore += scoreToAdd;

                        SteamUserStats.SetStat(scoreStat, currentScore);
                        logger.logLow("Updating steam score to: " + currentScore);
                    }
                    else
                    {
                        logger.debugLog("Got extreme value attempt: " + scoreToAdd);
                        shouldResetNumber = true;
                    }
                    scoreToAdd = 0;
                }

                Thread.Sleep(secondsBetweenPush * 1000);
            }
        }
        
        [HarmonyPatch(typeof(íëåäòéðåïîé), "óèçéñçëñìóð")]
        class setSteamScorePatch
        {
            static bool Prefix(string çìîñìëðêëéò, int åéçèêêðíçòñ)
            {
                if (çìîñìëðêëéò == scoreStat)
                {
                    scoreToAdd = (åéçèêêðíçòñ - currentScore);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerInfo), "updateScore")]
        class updateScorePatch
        {
            static bool Prefix(PlayerInfo __instance, int amount, string note, bool tickSound)
            {
                if (shouldResetNumber)
                {
                    Traverse info = Traverse.Create(__instance);
                    info.Field("score").SetValue(totalRoundScore);
                    shouldResetNumber = false;
                    return false;
                }
                return true;
            }
        }

    }
}
