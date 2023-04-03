using HarmonyLib;

namespace BugFixes.MoveCameraPatch
{
    class PrefixesAndPostfixes
    {
        [HarmonyPatch(typeof(MoveCamera), "SpectateCamera")]
        [HarmonyPrefix]
        static bool SpectateCameraPrefix()
        {
            if (GameManager.state == GameManager.GameState.GameOver)
            {
                // Skip original
                return false;
            }

            // Reset hitbox in original
            return true;
        }

        [HarmonyPatch(typeof(MoveCamera), "FreeCam")]
        [HarmonyPrefix]
        static bool FreeCamPrefix()
        {
            if (GameManager.state == GameManager.GameState.GameOver)
            {
                // Skip original
                return false;
            }

            // Reset hitbox in original
            return true;
        }
    }
}
