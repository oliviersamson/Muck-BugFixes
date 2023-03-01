using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugFixes.InventoryUIPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(InventoryUI), "Repair")]
        [HarmonyPostfix]
        static void RepairPostfix(InventoryUI __instance)
        {
            __instance.hotbar.UpdateHotbar();
        }
    }
}
