using HarmonyLib;
using UnityEngine;

namespace BugFixes.UiControllerPatch
{
    public static class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(UiController), "Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(UiController __instance)
        {
            Plugin.Log.LogDebug("Adding back button callback on settings menu to set the pause UI active again");
            GameObject pauseUI = __instance.transform.Find("PauseUI").gameObject;

            Settings settings = __instance.transform.Find("Settings").gameObject.GetComponent<Settings>();
            settings.backBtn.onClick.AddListener(
                () =>
                {
                    pauseUI.SetActive(true);
                });

            Transform keyListener = __instance.transform.Find("KeyListener");
            keyListener.SetAsLastSibling();
        }
    }
}
