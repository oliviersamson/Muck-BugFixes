﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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

                    // Get rotated transform since it is not yet calculated for this GameObject
                    Transform gameObjectTransform = gameObject.transform;
                    gameObjectTransform.rotation = Quaternion.LookRotation(initialHit.normal);

                    // Set pos a bit on top of object
                    Vector3 castPos = gameObject.transform.position + (gameObjectTransform.forward * 108.7f);

                    // Create box parameters with same size and oriantation as object
                    Vector3 halfExtents = gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents;

                    // Deactivate this game object so that CheckBox doesn't hit it
                    gameObject.SetActive(false);

                    // Verify around spawning point for object(s) that might be in the way
                    if (Physics.CheckBox(initialHit.point, halfExtents, gameObjectTransform.rotation, ~instance.whatIsTerrain.value))
                    {
                        Plugin.Log.LogDebug($"{gameObject.name} is colliding with something at {initialHit.point}! Trying again...");

                        // Destroy game object
                        GameObject.Destroy(gameObject);

                        // return immediately
                        return true;
                    }

                    // Reactivate game object
                    gameObject.SetActive(true);

                    if (initialHit.collider.name.Contains("Clone"))
                    {
                        Plugin.Log.LogDebug($"{gameObject.name} is colliding with something at {initialHit.point}! Trying again...");

                        GameObject.Destroy(gameObject);

                        return true;
                    }

                    bool distanceToGroundTooShort = false;

                    // If gameObject is a tent
                    if (gameObject.name.Contains("House"))
                    {
                        // Deactivate this game object so that the BoxCast doesn't hit it
                        gameObject.SetActive(false);

                        // Cast Box ray 
                        if (Physics.BoxCast(castPos, halfExtents, -(gameObjectTransform.forward), out RaycastHit hit, gameObjectTransform.rotation, 200f, instance.whatIsTerrain))
                        {
                            // If tent distance to ground is too short
                            if (hit.distance < 102.0f)
                            {
                                Plugin.Log.LogDebug($"{gameObject.name} distance to ground is too short! Trying again...");

                                // Destroy game object
                                GameObject.Destroy(gameObject);

                                distanceToGroundTooShort = true;
                            }
                        }

                        // Reactivate game object
                        gameObject.SetActive(true);
                    }

                    return distanceToGroundTooShort;
                }));

            // Jump back to beginning of loop if distanceToGroundTooShort is true
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, label));

            return codeMatcher.InstructionEnumeration();
        }
    }
}
