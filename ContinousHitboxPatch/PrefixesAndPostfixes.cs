using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BugFixes.ContinousHitbox
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(global::ContinousHitbox), "ResetHitbox")]
        [HarmonyPrefix]
        static bool ResetHitbox(global::ContinousHitbox __instance)
        {
            if (__instance.transform.root.gameObject.TryGetComponent<DestroyObject>(out DestroyObject destroyObject))
            {
                Plugin.Log.LogDebug($"Stopping ContinousHitbox because source is dead");
                GameObject.Destroy(__instance);

                // Skip original
                return false;
            }

            // Reset hitbox in original
            return true;
        }
    }
}
