using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BugFixes.SpawnZoneGeneratorPatch
{
    [HarmonyPatch(typeof(SpawnZoneGenerator<Type>), "Start")]
    class Start
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new(instructions);

            // Match beginning of loop
            codeMatcher = codeMatcher.MatchForward(true, 
                new CodeMatch(OpCodes.Br),
                new CodeMatch(OpCodes.Ldarg_0));

            // Add label at current position
            Label label = generator.DefineLabel();
            codeMatcher.SetInstructionAndAdvance(codeMatcher.InstructionAt(0).WithLabels(label));
         
            // Match instruction saving GameObject gameObject as local variable at index 8
            codeMatcher = codeMatcher.MatchForward(true, 
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Stloc_S));

            // Advance after Stloc_S instruction
            codeMatcher = codeMatcher.Advance(1);

            // Load RaycastHit raycastHit (local variable at index 7)
            codeMatcher = codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 7));

            // Emit call to delegate, consuming RaycastHit raycastHit and returning false if hit something else than ground mesh
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<RaycastHit, bool>>(
                (raycastHit) => {

                    if (!raycastHit.collider.name.Contains("Mesh"))
                    {
                        Plugin.Log.LogDebug($"Spawn zone is not on ground at {raycastHit.point}! Trying again...");
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