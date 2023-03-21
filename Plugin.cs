using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace BugFixes
{
    public static class Globals 
    {
        public const string PLUGIN_GUID = "muck.mrboxxy.bugfixes";
        public const string PLUGIN_NAME = "BugFixes";
        public const string PLUGIN_VERSION = "1.4.1";
    }

    [BepInPlugin(Globals.PLUGIN_GUID, Globals.PLUGIN_NAME, Globals.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;

        public Harmony harmony;

        private void Awake()
        {
            // Plugin startup logic
            Log = base.Logger;

            harmony = new Harmony(Globals.PLUGIN_NAME);

            harmony.PatchAll(typeof(GameManagerPatch.PrefixesAndPostfixes));
            Log.LogInfo("Patched GameManager.Start()");

            harmony.PatchAll(typeof(GenerateCampPatch.SpawnObjects_GameObject));
            Log.LogInfo("Patched GenerateCamp.SpawnObjects(GameObject, int, ConsistentRandom)");

            harmony.PatchAll(typeof(GenerateCampPatch.SpawnObjects_StructureSpawner));
            Log.LogInfo("Patched GenerateCamp.SpawnObjects(StructureSpawner, int, ConsistentRandom)");

            harmony.PatchAll(typeof(StructureSpawnerPatch.Start));
            Log.LogInfo("Patched StructureSpawner.Start()");

            harmony.PatchAll(typeof(ResourceGeneratorPatch.PrefixesAndPostfixes));
            Log.LogInfo("Patched ResourceGenerator.SpawnTree(Vector3 pos)");
            Log.LogInfo("Patched ResourceGenerator.GenerateForest()");

            harmony.PatchAll(typeof(ContinousHitboxPatch.PrefixesAndPostfixes));
            Log.LogInfo("Patched ContinousHitbox.ResetHitbox()");

            harmony.PatchAll(typeof(SpawnZoneGeneratorPatch.Start));
            Log.LogInfo("Patched SpawnZoneGenerator.Start()");

            harmony.PatchAll(typeof(SpawnZonePatch.FindRandomPosTranspiler));
            Log.LogInfo("Patched SpawnZone.FindRandomPos()");

            harmony.PatchAll(typeof(HitableMobPatch.PrefixesAndPostfixes));
            Log.LogInfo("Patched HitableMob.OnKill()");

            harmony.PatchAll(typeof(InventoryUIPatch.PrefixesAndPostfixes));
            Log.LogInfo("Patched InventoryUI.Repair()");
            Log.LogInfo("Patched InventoryUI.AddItemToInventory()");

            harmony.PatchAll(typeof(LobbyVisualsPatch.PrefixesAndPostfixes));
            Log.LogInfo("Patched LobbyVisuals.Awake()");
        }
    }
}
