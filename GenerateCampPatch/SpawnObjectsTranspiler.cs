using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BugFixes.GenerateCampPatch
{
    [HarmonyPatch(typeof(GenerateCamp), "SpawnObjects", new Type[] { typeof(GameObject), typeof(int), typeof(ConsistentRandom) })]
    class SpawnObjects_GameObject
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new(instructions);

            // Match beginning of loop
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Br));

            // Add label at current position
            Label label = generator.DefineLabel();
            codeMatcher.SetInstructionAndAdvance(codeMatcher.InstructionAt(0).WithLabels(label));

            // Goto FindPos call
            codeMatcher = codeMatcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Call));

            // Load the GameObject obj (argument at index 1) into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1));

            // Emit call to delagate, consuming GameObject obj
            // (Patch to get a sphere radius matching the object's size)
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<GameObject, float>>(
                (gameObject) => {

                    // Get the max extent in x,z for sphere radius
                    return Math.Max(gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x, gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z);
                }));

            // Emit call to delagate, consuming current GenerateCamp instance, ConsistentRandom rand (argument at index 3) and float radius (pushed into stact by previous delegate)
            // (Patch to get a RaycastHit with the given sphereRadius)
            codeMatcher = codeMatcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<GenerateCamp, ConsistentRandom, float, RaycastHit>>(
                (instance, rand, sphereRadius) => {

                    // Call new GenerateCamp.FindPos() method with new argument radius
                    return instance.FindPos(rand, sphereRadius);
                }));

            // Match the instruction jumping if raycastHit.collider != null
            // Somehow it's OpCodes.Brtrue instead of OpCodes.Brtrue_S... OpCodes.Brtrue_S makes the mod crash
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Brtrue));

            // Load the current GenerateCamp instance into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));

            // Emit new call to delegate, consuming bool resulting from raycastHit.collider != null and current GenerateCamp instance
            // (Patch to retry house generation if the resulting object the raycast has hit something)
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, GenerateCamp, bool>>(
                (result, instance) =>
                {
                    if (result)
                    {
                        // Increase camp radius in order to not have any infinite loops due to always failing to spawn object
                        AccessTools.Field(typeof(GenerateCamp), "campRadius").SetValue(instance, (float)AccessTools.Field(typeof(GenerateCamp), "campRadius").GetValue(instance) + 1.0f);
                    }
                    return result;
                }));

            // Change jump to go back at begining of loop without incrementing the loop counter
            codeMatcher = codeMatcher.SetOperandAndAdvance(label);

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
            // (Patch to retry tent generation if the resulting object is too close to the ground)
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
                        if (Physics.BoxCast(castPos, halfExtents, -(gameObject.transform.up), out RaycastHit hit, orientation, 20f, instance.whatIsGround))
                        {
                            // If tent distance to ground is too short
                            if (hit.distance < 2.0f)
                            {
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
    class SpawnObjects_StructureSpawner
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new(instructions);

            // Match beginning of loop
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Br));

            // Add label at current position
            Label label = generator.DefineLabel();
            codeMatcher.SetInstructionAndAdvance(codeMatcher.InstructionAt(0).WithLabels(label));

            // Goto FindPos call
            codeMatcher = codeMatcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Call));

            // Load the GameObject original (local variable at index 2) into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2));

            // Emit call to delagate, consuming GameObject original
            // (Patch to get a sphere radius matching the object's size)
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<GameObject, float>>(
                (gameObject) => {

                    // Get the max extent in x,z for sphere radius
                    return Math.Max(gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x, gameObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z);
                }));

            // Emit call to delagate, consuming current GenerateCamp instance, ConsistentRandom rand (argument at index 3) and float radius (pushed into stact by previous delegate)
            // (Patch to get a RaycastHit with the given sphereRadius)
            codeMatcher = codeMatcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<GenerateCamp, ConsistentRandom, float, RaycastHit>>(
                (instance, rand, sphereRadius) => {

                    // Call new GenerateCamp.FindPos() method with new argument radius
                    return instance.FindPos(rand, sphereRadius);
                }));

            // Match the instruction jumping if raycastHit.collider != null
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Brtrue));

            // Load the current GenerateCamp instance into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));

            // Emit new call to delegate, consuming bool resulting from raycastHit.collider != null and current GenerateCamp instance
            // (Patch to retry house generation if the resulting object the raycast has hit something)
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, GenerateCamp, bool>>(
                (result, instance) =>
                {
                    if (result)
                    {
                        // Increase camp radius in order to not have any infinite loops due to always failing to spawn object
                        AccessTools.Field(typeof(GenerateCamp), "campRadius").SetValue(instance, (float)AccessTools.Field(typeof(GenerateCamp), "campRadius").GetValue(instance) + 1.0f);
                    }
                    return result;
                }));

            // Change jump to go back at begining of loop without incrementing the loop counter
            codeMatcher = codeMatcher.SetOperandAndAdvance(label);

            // Match the instruction loading gameObject on stack for GetComponent call
            codeMatcher = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S));

            // Load the current GenerateCamp instance and the GameObject gameObject (local variable at index 4) into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4));

            // Emit new call to delegate, consuming current GenerateCamp instance and GameObject obj
            // (Patch to retry house generation if the resulting object is too close to the ground)
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
                        if (Physics.BoxCast(castPos, halfExtents, -(gameObject.transform.forward), out RaycastHit hit, orientation, 20f, instance.whatIsGround))
                        {
                            // If tent distance to ground is too short
                            if (hit.distance < 2.0f)
                            {
                                //Plugin.Log.LogInfo($"House distance to ground is too short!");
                                //Plugin.Log.LogInfo($"Raycast hit at {hit.point}");
                                //Plugin.Log.LogInfo($"Distance = {hit.distance}");

                                // Destroy game object
                                GameObject.Destroy(gameObject);

                                // Increase camp radius in order to not have any infinite loops due to always failing to spawn object
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
}
