using Verse;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace MechHumanlikes
{
    public class ImmunityHandler_Patch
    {
        // Mechanical units are immune to contracting diseases.
        [HarmonyPatch(typeof(ImmunityHandler), "DiseaseContractChanceFactor")]
        [HarmonyPatch(new Type[] { typeof(HediffDef), typeof(HediffDef), typeof(BodyPartRecord) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
        public class DiseaseContractChanceFactor_Patch
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
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load the ImmunityHandler "this"
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmunityHandler), "pawn")); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DiseaseContractChanceFactor_Patch), nameof(IsVulnerable))); // Our function call
                    }
                }
            }

            private static bool IsVulnerable(bool organic, Pawn pawn)
            {
                return organic && !MHC_Utils.IsConsideredMechanical(pawn);
            }
        }
    }
}