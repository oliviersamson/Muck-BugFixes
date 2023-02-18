using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace BugFixes
{
    public static class Globals 
    {
        public const string PLUGIN_GUID = "muck.mrboxxy.bugfixes";
        public const string PLUGIN_NAME = "BugFixes";
        public const string PLUGIN_VERSION = "1.1.1";
    }

    [BepInPlugin(Globals.PLUGIN_GUID, Globals.PLUGIN_NAME, Globals.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        public static bool randomSeedSet;

        public Harmony harmony;

        private void Awake()
        {
            // Plugin startup logic
            Log = base.Logger;
            randomSeedSet = false;

            harmony = new Harmony(Globals.PLUGIN_NAME);

            harmony.PatchAll(typeof(LootDropPatch.PrefixesAndPostfixes));
            Log.LogInfo("Patched LootDrop.GetLoot(ConsistentRandom)");

            harmony.PatchAll(typeof(GenerateCampPatch.SpawnObjects_GameObject));
            Log.LogInfo("Patched GenerateCamp.SpawnObjects(GameObject, int, ConsistentRandom)");

            harmony.PatchAll(typeof(GenerateCampPatch.SpawnObjects_StructureSpawner));
            Log.LogInfo("Patched GenerateCamp.SpawnObjects(StructureSpawner, int, ConsistentRandom)");

            harmony.PatchAll(typeof(StructureSpawnerPatch.Start));
            Log.LogInfo("Patched StructureSpawner.Start()");
        }
    }
}
