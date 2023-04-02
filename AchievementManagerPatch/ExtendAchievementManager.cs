using Steamworks;
using Steamworks.Data;

namespace UnityEngine
{
    public static class ExtendAchievementManager
    {
        public static void OnStatsReceived(this AchievementManager manager, SteamId id, Result result)
        {
            if (!manager.CanUseAchievements())
            {
                return;
            }

            if (SteamUserStats.GetStatInt("Buffkills") >= 250)
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
