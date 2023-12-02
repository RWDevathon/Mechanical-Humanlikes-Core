using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using System.Reflection.Emit;

namespace MechHumanlikes
{
    public class Hediff_Patch
    {
        [HarmonyPatch]
        public class BleedRate_Patch
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> TargetProperties()
            {
                foreach (System.Type subclass in typeof(Hediff).AllSubclasses())
                {
                    if (AccessTools.DeclaredPropertyGetter(subclass, "BleedRate") is MethodInfo method)
                    {
                        yield return method;
                    }
                }
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Dead));

                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (i < instructions.Count - 1 && instructions[i + 1].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Dup); // Load a copy of the Pawn onto the Stack
                    }
                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BleedRate_Patch), nameof(CannotBleed))); // Our function call
                    }
                }
            }

            // Pawns that have any HediffGiver_Bleeding can bleed. We only check our own mechanical pawns currently.
            private static bool CannotBleed(Pawn pawn, bool isDead)
            {
                return isDead || (MHC_Utils.IsConsideredMechanical(pawn) && !MHC_Utils.cachedBleedingHediffGivers.ContainsKey(pawn.RaceProps));
            }
        }

        [HarmonyPatch]
        public class PainOffset_Patch
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> TargetProperties()
            {
                yield return AccessTools.DeclaredPropertyGetter(typeof(Hediff), "PainOffset");
                foreach (System.Type subclass in typeof(Hediff).AllSubclasses())
                {
                    if (AccessTools.DeclaredPropertyGetter(subclass, "PainOffset") is MethodInfo method)
                    {
                        yield return method;
                    }
                }
            }

            [HarmonyPrefix]
            public static bool Prefix(ref float __result, Pawn ___pawn)
            {
                if (MHC_Utils.IsConsideredMechanical(___pawn))
                {
                    __result = 0f;
                    return false;
                }
                return true;
            }
        }
    }
}
