using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MechHumanlikes
{
    public class HistoryAutoRecorderWorker_ColonistMood_Patch
    {
        // This method assumes that if the player has a colonist, they have at least one colonist with mood. This transpiler corrects that assumption.
        [HarmonyPatch(typeof(HistoryAutoRecorderWorker_ColonistMood), "PullRecord")]
        public class PullRecord_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                bool insertionComplete = false;

                for (int i = 0; i < instructions.Count; i++)
                {
                    if (!insertionComplete && instructions[i].opcode == OpCodes.Call && i > 0 && instructions[i - 1].opcode == OpCodes.Ldloc_0)
                    {
                        insertionComplete = true;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PullRecord_Patch), nameof(HasColonistWithMood))); // Our function call 
                    }
                    else
                    {
                        yield return instructions[i];
                    }
                }
            }

            private static bool HasColonistWithMood(List<Pawn> colonists)
            {
                // There is a colonist with mood if it is not true that all of the given pawns are moodless. More performant than .Any apparently!
                return colonists.Any(pawn => pawn.needs.mood != null);
            }
        }
    }
}