using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BugFixes.DetectInteractablesPatch
{
    [HarmonyPatch(typeof(DetectInteractables), "Update")]
    class UpdateTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new(instructions);

            // Match first instruction after get_position call
            codeMatcher = codeMatcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4));

            // Emit call to delegate, consuming Vector3 position and return the new position
            codeMatcher = codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate<Func<Vector3, Vector3>>(
                (pos) => {

                    Vector3 forward = ((Transform)AccessTools.Field(typeof(DetectInteractables), "playerCam").GetValue(DetectInteractables.Instance)).forward;

                    Vector3 newPos = pos - (0.7f * forward);

                    return newPos;
                }));

            // Match instruction loading the maxDistance value
            codeMatcher = codeMatcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Ldc_R4));

            // Set the maxDistance to add the added offset into account
            codeMatcher.Instruction.operand = 4.7f;

            return codeMatcher.InstructionEnumeration();
        }
    }
}
