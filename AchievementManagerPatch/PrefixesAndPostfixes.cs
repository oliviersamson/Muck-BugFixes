using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace BugFixes.AchievementManagerPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(AchievementManager), "Awake")]
        [HarmonyPostfix]
        static void AwakePostfix()
        {
            SteamUserStats.OnUserStatsReceived += AchievementManager.Instance.OnStatsReceived;
        }

        [HarmonyPatch(typeof(AchievementManager), "AddKill")]
        [HarmonyPostfix]
        static void AddKillPostfix()
        {
            if (SteamUserStats.GetStatInt("Buffkills") == 250)
            {
                foreach (Achievement ach in SteamUserStats.Achievements)
                {
                    if (ach.Name == "Underdog" && !ach.State)
                    {
                        ach.Trigger();
                    }
                }
            }
        }       
    }
}