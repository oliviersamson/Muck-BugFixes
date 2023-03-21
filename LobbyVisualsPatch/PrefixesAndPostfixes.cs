using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BugFixes.LobbyVisualsPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(LobbyVisuals), "Awake")]
        [HarmonyPostfix]
        static void AwakePostfix()
        {
            // Get background texture from another UI element
            GameObject lobbyID = GameObject.Find("LobbyID");
            Texture texture = lobbyID.GetComponent<RawImage>().mainTexture;

            // Get LobbySettings UI element and apply new texture
            GameObject menuButton = GameObject.Find("LobbySettings");
            menuButton.GetComponent<RawImage>().texture = texture;
        }
    }
}
