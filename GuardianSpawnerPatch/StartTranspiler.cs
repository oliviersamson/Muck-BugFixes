using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BugFixes.GuardianSpawnerPatch
{
    [HarmonyPatch(typeof(GuardianSpawner), "Start")]
    class StartTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new(instructions);

            // Match beginning of loop
            codeMatcher = codeMatcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Callvirt));

            // Add label at current position
            Label label = generator.DefineLabel();
            codeMatcher.SetInstructionAndAdvance(codeMatcher.InstructionAt(0).WithLabels(label));          

            // Match instruction right after raycast
            codeMatcher = codeMatcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Call));

            // Load RaycastHit raycastHit (local variable at index 7)
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 7));

            // Emit call to delegate, consuming RaycastHit raycastHit and returning false if hit something else than ground mesh
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<RaycastHit, bool>>(
                (raycastHit) =>
                {

                    if (!raycastHit.collider.name.Contains("Mesh"))
                    {
                        Plugin.Log.LogDebug($"Guardian spawner is not on ground at {raycastHit.point}! Trying again...");
                        return false;
                    }

                    return true;
                }));

            // Jump if above return is false
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label));

            return codeMatcher.InstructionEnumeration();
        }
    }
}
