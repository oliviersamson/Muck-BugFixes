using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace BugFixes.SpawnZonePatch
{
    [HarmonyPatch(typeof(SpawnZone), "FindRandomPos")]
    class FindRandomPosTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new(instructions);

            // Goto first instruction of method
            codeMatcher = codeMatcher.MatchForward(false, new CodeMatch(OpCodes.Call));

            // Add label at current position
            Label label = generator.DefineLabel();
            codeMatcher.SetInstructionAndAdvance(codeMatcher.InstructionAt(0).WithLabels(label));

            // Match instructions before return with raycastHit.point
            codeMatcher = codeMatcher.MatchForward(false, 
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Ret));

            // Load current SpawnZone instance RaycastHit raycastHit (local variable at index 2) into top of stack
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2));

            // Emit delegate, consuming the SpawnZone instance, RaycastHit raycastHit and returning false if hit something else than ground mesh
            // Gives up on trying to find a new position after 10 tries
            int failCount = 0;
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<SpawnZone, RaycastHit, bool>>(
                (instance, raycastHit) => {
                    if (!raycastHit.collider.name.Contains("Mesh") && failCount < 10)
                    {
                        Plugin.Log.LogDebug($"Entity is not on ground at {raycastHit.point}! Trying again...");
                        failCount++;

                        return false;
                    }

                    failCount = 0;
                    return true;
                }));

            // Jump if above return is false
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label));

            return codeMatcher.InstructionEnumeration();
        }
    }
}
