using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BugFixes.StructureSpawnerPatch
{
    [HarmonyPatch(typeof(StructureSpawner), "Start")]
    class Start
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new(instructions);

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
                        Plugin.Log.LogDebug($"Checking clearance for {gameObject.name} at {initialHit.point}");

                        // Get rotated transform since it is not yet calculated for this GameObject
                        Transform gameObjectTransform = gameObject.transform;
                        gameObjectTransform.rotation = Quaternion.LookRotation(initialHit.normal);

                        // Set pos a bit on top of object
                        Vector3 castPos = gameObject.transform.position + (gameObjectTransform.forward * 108.7f);

                        // Create box parameters with same size and oriantation as object
                        Vector3 halfExtents = gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;

                        // Cast Box ray 
                        if (Physics.BoxCast(castPos, halfExtents, -(gameObjectTransform.forward), out RaycastHit hit, gameObjectTransform.rotation, 200f, instance.whatIsTerrain))
                        {
                            Plugin.Log.LogDebug($"Clearance is {hit.distance}");

                            // If tent distance to ground is too short
                            if (hit.distance < 102.0f)
                            {
                                Plugin.Log.LogDebug($"House distance to ground is too short! Attempting to spawn again...");

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
