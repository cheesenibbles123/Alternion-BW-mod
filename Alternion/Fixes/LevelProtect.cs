using Harmony;
using BWModLoader;
using UnityEngine;
using System.Threading;
using Steamworks;

namespace Alternion.Fixes
{
    // It works but just for now until it can be verified on more machines dont include it into release
#if DEBUG
    [Mod]
    public class LevelProtect : MonoBehaviour
    {
        private static Logger logger = new Logger("[LevelProtect]");

        public static readonly string scoreStat = "stat_score";

        public static LevelProtect Instance;
        public bool allowScoreToBeSent = false;

        readonly int maxPossibleScore = 10000; // 10K max per min, if you get more than that I don't know what you are doing
        readonly int secondsBetweenPush = 60;
        static int currentScore;
        static int scoreToAdd = 0;

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

        private void checkScoreBeforePush()
        {
            while (true) // On secondary thread so it's probably ok ;P
            {
                if (scoreToAdd > 0)
                {
                    if (scoreToAdd < maxPossibleScore)
                    {
                        allowScoreToBeSent = true;
                        currentScore += scoreToAdd;
                        SteamUserStats.SetStat(scoreStat, currentScore);
                        logger.logLow("Updating steam score to: " + currentScore);
                    }
                    else
                    {
                        logger.debugLog("Got extreme value attempt: " + scoreToAdd);
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

    }
#endif
}
