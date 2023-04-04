using HarmonyLib;
using System.Collections.Generic;
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

            // Set render fogcolour to day sky
            foreach (Material mat in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                string matName = mat.name.Replace(" (Instance)", "");
                if (matName == "Skybox Cubemap Extended Day")
                {                    
                    mat.SetFloat("_Exposure", 1f);
                }
            }
        }
    }
}
