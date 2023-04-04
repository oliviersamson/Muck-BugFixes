using HarmonyLib;

namespace BugFixes.PlayerStatusPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(PlayerStatus), "Dracula")]
        [HarmonyPostfix]
        static void DraculaPostfix(PlayerStatus __instance)
        {
            if (__instance.hp > __instance.maxHp)
            {
                __instance.hp = __instance.maxHp;
            }
        }
    }
}
