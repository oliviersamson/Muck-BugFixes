using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugFixes.HitableMobPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(HitableMob), "OnKill")]
        [HarmonyPrefix]
        static void OnKillPrefix(HitableMob __instance)
        {
            if (__instance.gameObject.TryGetComponent<WoodmanBehaviour>(out WoodmanBehaviour woodman))
            {
                woodman.MakeAggressive(true);

                Plugin.Log.LogDebug("Woodmen of this camp set to aggressive!");
            }
        }
    }
}
