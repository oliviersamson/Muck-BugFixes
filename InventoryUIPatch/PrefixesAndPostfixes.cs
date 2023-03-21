using HarmonyLib;
using UnityEngine;

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

        [HarmonyPatch(typeof(InventoryUI), "AddItemToInventory")]
        [HarmonyPostfix]
        static void AddItemToInventoryPostfix(InventoryItem item)
        {
            if (Boat.Instance)
            {
                Boat.Instance.CheckForMapUpdate(item.id);
            }
        }
    }
}
