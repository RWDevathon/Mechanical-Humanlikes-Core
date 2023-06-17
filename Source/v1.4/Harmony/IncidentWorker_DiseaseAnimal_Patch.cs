using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using static MechHumanlikes.ImmunityHandler_Patch;
using System.Reflection.Emit;
using System.Reflection;

namespace MechHumanlikes
{
    public class IncidentWorker_DiseaseAnimal_Patch
    {
        // Remove all mechanical animals from the candidate list for animal diseases.
        [HarmonyPatch(typeof(IncidentWorker_DiseaseAnimal), "PotentialVictimCandidates")]
        public class PotentialVictims_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike));

                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DiseaseContractChanceFactor_Patch), nameof(IsOrganicAnimal))); // Our function call
                    }
                }
            }

            private static bool IsOrganicAnimal(bool humanlike, Pawn pawn)
            {
                return !humanlike && !MHC_Utils.IsConsideredMechanicalAnimal(pawn);
            }
        }
    }
}