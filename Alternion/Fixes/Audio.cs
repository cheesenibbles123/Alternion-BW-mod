using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alternion.Fixes
{
    class AudioFixes
    {
        [HarmonyPatch(typeof(CaptainVoiceSet), "ëèïèòêêòíîï")]
        class CaptainedVoicePatch
        {
            static bool Prefix(AudioClip[] çíëòðêîïåëå, int ïóäæëòñçéóæ, AudioSource çèêèëäóïéëç)
            {
                if (çèêèëäóïéëç && !çèêèëäóïéëç.isPlaying && ïóäæëòñçéóæ >= 0 && ïóäæëòñçéóæ < çíëòðêîïåëå.Length)
                {
                    çèêèëäóïéëç.clip = çíëòðêîïåëå[ïóäæëòñçéóæ];
                    çèêèëäóïéëç.Play();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerVoiceLibrary), "ðêòæòïòèëîî")]
        class PlayerVoicePatch
        {
            static bool Prefix(PlayerVoiceLibrary __instance, ref CaptainVoiceSet __result, PlayerInfo äíìíëðñïñéè)
            {
                CaptainVoiceSet[] voiceSet;

                if (GameMode.Instance.teamFactions[äíìíëðñïñéè.team] == "Navy")
                {
                    voiceSet = äíìíëðñïñéè.playerGender != 1 ? __instance.æíëåòíðéòïñ : __instance.ëñëðäñìååðñ;
                }
                else
                {
                    voiceSet = äíìíëðñïñéè.playerGender != 1 ? __instance.òåðìëðìêìîî : __instance.êèëêíëçïêîì;
                }

                int index = Math.Min(voiceSet.Length - 1, äíìíëðñïñéè.voiceIndex);
                __result = voiceSet[index];
                return false;
            }
        }
    }
}
