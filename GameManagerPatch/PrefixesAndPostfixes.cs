using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace BugFixes.GameManagerPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(GameManager), "Start")]
        [HarmonyPrefix]
        static void StartPrefix()
        {
            Plugin.Log.LogDebug("Setting seed for UnityEngine.Random");
            UnityEngine.Random.InitState(GameManager.GetSeed());
        }

        [HarmonyPatch(typeof(GameManager), "Awake")]
        [HarmonyPostfix]
        static void AwakePostfix(GameManager __instance)
        {
            __instance.gameoverUi.transform.Find("Overlay").Find("Stats").GetComponent<LayoutElement>().minHeight = 580;
        }
    }
}
