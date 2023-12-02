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
                    if (i < instructions.Count - 1 && instructions[i + 1].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Dup); // Load a copy of the Pawn onto the Stack
                    }

                    yield return instructions[i];

                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CalculateMechanicalPain_Patch), nameof(IsOrganic))); // Our function call
                    }
                }
            }

            private static bool IsOrganic(Pawn pawn, bool notMechanoid)
            {
                return notMechanoid && !MHC_Utils.IsConsideredMechanical(pawn);
            }
        }

        // Some mechanical units do not bleed.
        [HarmonyPatch(typeof(HediffSet), "CalculateBleedRate")]
        public class CalculateBleedRate_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsFlesh));

                for (int i = 0; i < instructions.Count; i++)
                {
                    if (i < instructions.Count - 1 && instructions[i + 1].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Dup); // Load a copy of the Pawn onto the Stack
                    }

                    yield return instructions[i];

                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CalculateBleedRate_Patch), nameof(CanBleed))); // Our function call
                    }
                }
            }

            // Pawns that have any HediffGiver_Bleeding can bleed. We only check our own mechanical pawns currently.
            private static bool CanBleed(Pawn pawn, bool notMechanoid)
            {
                return notMechanoid && (!MHC_Utils.IsConsideredMechanical(pawn) || MHC_Utils.cachedBleedingHediffGivers.ContainsKey(pawn.RaceProps));
            }
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