using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace BugFixes
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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

            harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(BugFixes));
            harmony.PatchAll(typeof(GenerateCamp_SpawnObjects_Patch));
            harmony.PatchAll(typeof(GenerateCamp_SpawnObjects2_Patch));
            harmony.PatchAll(typeof(StructureSpawner_Start_Patch));
        }
    }

    class BugFixes
    {
        [HarmonyPatch(typeof(LootDrop), "GetLoot", new Type[] { typeof(ConsistentRandom) })]
        [HarmonyPrefix]
        static void GetLootPrefix()
        {
            if(!Plugin.randomSeedSet)
            {
                Plugin.randomSeedSet = true;
                UnityEngine.Random.InitState(GameManager.GetSeed());

                Plugin.Log.LogInfo($"Random Seed is set!");
            }
        }
    }

    [HarmonyPatch(typeof(GenerateCamp), "SpawnObjects", new Type[] { typeof(GameObject), typeof(int), typeof(ConsistentRandom) })]
    class GenerateCamp_SpawnObjects_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions);

            // Match beginning of loop
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Br));

            // Add label at current position
            Label label = generator.DefineLabel();
            codeMatcher.SetInstructionAndAdvance(codeMatcher.InstructionAt(0).WithLabels(label));

            // Match the instruction loading gameObject on stack for GetComponent call
            codeMatcher = codeMatcher.MatchForward(true, 
                new CodeMatch(OpCodes.Ldc_R4),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Callvirt));

            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_3));

            // Load the current GenerateCamp instance and the GameObject gameObject (local variable at index 3) into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3));

            // Emit new call to delegate, consuming current GenerateCamp instance and GameObject obj
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<GenerateCamp, GameObject, bool>>(
                (instance, gameObject) => {

                    bool distanceToGroundTooShort = false;

                    // If gameObject is a tent
                    if (gameObject.name.Contains(instance.hut.name))
                    {
                        // Set pos a bit on top of object
                        Vector3 castPos = gameObject.transform.position + (gameObject.transform.up * 7.0f);

                        // Create box parameters with same size and oriantation as object
                        Vector3 halfExtents = gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
                        Quaternion orientation = Quaternion.LookRotation(gameObject.transform.up);

                        // Cast Box ray 
                        RaycastHit hit = default(RaycastHit);
                        if (Physics.BoxCast(castPos, halfExtents, -(gameObject.transform.up), out hit, orientation, 20f, instance.whatIsGround))
                        {
                            // If tent distance to ground is too short
                            if (hit.distance < 2.0f)
                            {
                                Plugin.Log.LogInfo($"Tent placement redone because its distance to the ground was too low");
                                //Plugin.Log.LogInfo($"Tent distance to ground is too short!");
                                //Plugin.Log.LogInfo($"Raycast hit at {hit.point}");
                                //Plugin.Log.LogInfo($"Distance = {hit.distance}");

                                // Destroy game object
                                GameObject.Destroy(gameObject);

                                //Increase camp radius in order to not have any infinite loops due to always failing to spawn object
                                AccessTools.Field(typeof(GenerateCamp), "campRadius").SetValue(instance, (float)AccessTools.Field(typeof(GenerateCamp), "campRadius").GetValue(instance) + 1.0f);

                                distanceToGroundTooShort = true;
                            }
                        }
                    }

                    return distanceToGroundTooShort;
            }));

            // Jump back to beginning of loop if distanceToGroundTooShort is true
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, label));

            return codeMatcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(GenerateCamp), "SpawnObjects", new Type[] { typeof(StructureSpawner), typeof(int), typeof(ConsistentRandom) })]
    class GenerateCamp_SpawnObjects2_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions);

            // Match beginning of loop
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Br));

            // Add label at current position
            Label label = generator.DefineLabel();
            codeMatcher.SetInstructionAndAdvance(codeMatcher.InstructionAt(0).WithLabels(label));

            // Match the instruction loading gameObject on stack for GetComponent call
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S));

            // Load the current GenerateCamp instance and the GameObject gameObject (local variable at index 4) into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4));

            // Emit new call to delegate, consuming current GenerateCamp instance and GameObject obj
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<GenerateCamp, GameObject, bool>>(
                (instance, gameObject) => {

                    bool distanceToGroundTooShort = false;

                    // If gameObject is a tent
                    if (gameObject.name.Contains("House"))
                    {
                        // Set pos a bit on top of object
                        Vector3 castPos = gameObject.transform.position + (gameObject.transform.forward * 8.7f);

                        // Create box parameters with same size and oriantation as object
                        Vector3 halfExtents = gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
                        Quaternion orientation = Quaternion.LookRotation(gameObject.transform.forward);

                        // Cast Box ray 
                        RaycastHit hit = default(RaycastHit);
                        if (Physics.BoxCast(castPos, halfExtents, -(gameObject.transform.forward), out hit, orientation, 20f, instance.whatIsGround))
                        {
                            // If tent distance to ground is too short
                            if (hit.distance < 2.0f)
                            {
                                Plugin.Log.LogInfo($"House placement redone because its distance to the ground was too low");
                                //Plugin.Log.LogInfo($"House distance to ground is too short!");
                                //Plugin.Log.LogInfo($"Raycast hit at {hit.point}");
                                //Plugin.Log.LogInfo($"Distance = {hit.distance}");

                                // Destroy game object
                                GameObject.Destroy(gameObject);

                                //Increase camp radius in order to not have any infinite loops due to always failing to spawn object
                                AccessTools.Field(typeof(GenerateCamp), "campRadius").SetValue(instance, (float)AccessTools.Field(typeof(GenerateCamp), "campRadius").GetValue(instance) + 1.0f);

                                distanceToGroundTooShort = true;
                            }
                        }
                    }

                    return distanceToGroundTooShort;
            }));

            // Jump back to beginning of loop if distanceToGroundTooShort is true
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, label));

            return codeMatcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(StructureSpawner), "Start")]
    class StructureSpawner_Start_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions);

            // Match beginning of loop
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Br));

            // Add label at current position
            Label label = generator.DefineLabel();
            codeMatcher.SetInstructionAndAdvance(codeMatcher.InstructionAt(0).WithLabels(label));

            // Match the instructions calling instantiate on game object
            codeMatcher = codeMatcher.MatchForward(true,
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Call));

            // Move after instantiate call
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S));

            // Load the current StructureSpawner instance and the GameObject gameObject2 (local variable at index 6) into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 6));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4));

            // Emit new call to delegate, consuming current StructureSpawner instance and GameObject gameObject2
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<StructureSpawner, GameObject, RaycastHit, bool>>(
                (instance, gameObject, initialHit) => {

                    bool distanceToGroundTooShort = false;

                    // If gameObject is a tent
                    if (gameObject.name.Contains("House"))
                    {
                        // Get rotated transform since it is not yet calculated for this GameObject
                        Transform gameObjectTransform = gameObject.transform;
                        gameObjectTransform.rotation = Quaternion.LookRotation(initialHit.normal);

                        // Set pos a bit on top of object
                        Vector3 castPos = gameObject.transform.position + (gameObjectTransform.forward * 8.7f);

                        // Create box parameters with same size and oriantation as object
                        Vector3 halfExtents = gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
                        Quaternion orientation = Quaternion.LookRotation(gameObjectTransform.forward);

                        // Cast Box ray 
                        RaycastHit hit = default(RaycastHit);
                        if (Physics.BoxCast(castPos, halfExtents, -(gameObjectTransform.forward), out hit, orientation, 20f, instance.whatIsTerrain))
                        {
                            // If tent distance to ground is too short
                            if (hit.distance < 2.0f)
                            {
                                Plugin.Log.LogInfo($"House placement redone because its distance to the ground was too low");
                                //Plugin.Log.LogInfo($"House distance to ground is too short!");
                                //Plugin.Log.LogInfo($"Raycast hit at {hit.point}");
                                //Plugin.Log.LogInfo($"Distance = {hit.distance}");

                                // Destroy game object
                                GameObject.Destroy(gameObject);

                                distanceToGroundTooShort = true;
                            }
                        }
                    }

                    return distanceToGroundTooShort;
            }));

            // Jump back to beginning of loop if distanceToGroundTooShort is true
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, label));

            return codeMatcher.InstructionEnumeration();
        }
    }
}
