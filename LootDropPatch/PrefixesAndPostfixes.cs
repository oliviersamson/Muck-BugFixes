using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugFixes.LootDropPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(LootDrop), "GetLoot", new Type[] { typeof(ConsistentRandom) })]
        [HarmonyPrefix]
        static void GetLootPrefix()
        {
            if (!Plugin.randomSeedSet)
            {
                Plugin.randomSeedSet = true;
                UnityEngine.Random.InitState(GameManager.GetSeed());
            }
        }
    }
}
