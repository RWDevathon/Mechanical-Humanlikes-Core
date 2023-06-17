using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace MechHumanlikes
{
    public class HediffSet_Patch
    {
        // Mechanical units feel no pain.
        [HarmonyPatch(typeof(HediffSet), "CalculatePain")]
        public class CalculateMechanicalPain_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsFlesh));

                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CalculateMechanicalPain_Patch), nameof(IsMechanical))); // Our function call
                    }
                }
            }

            private static bool IsMechanical(bool organic, Pawn pawn)
            {
                return !organic || MHC_Utils.IsConsideredMechanical(pawn);
            }

            //[HarmonyPrefix]
            //public static bool Prefix(ref HediffSet __instance, ref float __result)
            //{
            //    if (MHC_Utils.IsConsideredMechanical(__instance.pawn))
            //    {
            //        __result = 0f;
            //        return false;
            //    }
            //    return true;
            //}
        }

        // Mechanical units may check against different hediffs than organics do for temperature hediff concerns.
        [HarmonyPatch(typeof(HediffSet), "HasTemperatureInjury")]
        public class HasTemperatureInjury_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(HediffSet __instance, ref bool __result, TemperatureInjuryStage minStage)
            {
                if (!MHC_Utils.IsConsideredMechanical(__instance.pawn))
                {
                    return true;
                }

                // The targetHediffs cached are all hediffs which are temperature related and need to be checked against the hediffs present on this pawn.
                HashSet<HediffDef> targetHediffDefs = MHC_Utils.GetTemperatureHediffDefsForRace(__instance.pawn.RaceProps);
                if (targetHediffDefs.Count > 0)
                {
                    foreach (Hediff hediff in __instance.hediffs)
                    {
                        if (targetHediffDefs.Contains(hediff.def) && hediff.CurStageIndex > (int)minStage)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                __result = false;
                return false;
            }
        }
    }
}