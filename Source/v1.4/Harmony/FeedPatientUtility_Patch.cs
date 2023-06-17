using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace MechHumanlikes
{
    public class FeedPatientUtility_Patch
    {
        // This transpiler stops pawns from trying to feed pawns that are currently charging.
        [HarmonyPatch(typeof(FeedPatientUtility), "ShouldBeFed")]
        public class ShouldBeFed_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.EatsFood));

                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShouldBeFed_Patch), nameof(NeedsFood))); // Our function call
                    }
                }
            }

            // Returns true if the pawn is not charge capable or is not currently charging.
            private static bool NeedsFood(bool eatsFood, Pawn pawn)
            {
                return eatsFood && (!MHC_Utils.CanUseBattery(pawn) || pawn.CurJob.def != MHC_JobDefOf.MHC_GetRecharge);
            }
        }
    }
}