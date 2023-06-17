using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace MechHumanlikes
{
    public class Alert_ColonistLeftUnburied_Patch
    {
        // Pawns that are not considered humanlike don't get unburied colonist notifications.
        [HarmonyPatch(typeof(Alert_ColonistLeftUnburied), "IsCorpseOfColonist")]
        public class IsCorpseOfColonist_Patch
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
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Corpse
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IsCorpseOfColonist_Patch), nameof(SapientHumanlike))); // Our function call
                    }
                }
            }

            private static bool SapientHumanlike(bool humanlike, Corpse corpse)
            {
                return humanlike && !MHC_Utils.IsConsideredNonHumanlike(corpse.InnerPawn);
            }
        }
    }
}