using System;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace MechHumanlikes
{
    public class Corpse_Patch
    {
        // This transpiler prevents mechanical units from triggering corpse thoughts. Only organic humanlikes trigger it normally.
        [HarmonyPatch(typeof(Corpse), "GiveObservedHistoryEvent")]
        public class GiveObservedHistoryEvent_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetMethod = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike));

                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (instructions[i].Calls(targetMethod))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Corpse
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GiveObservedHistoryEvent_Patch), nameof(IsOrganicHumanlike))); // Our function call
                    }
                }
            }

            private static bool IsOrganicHumanlike (bool humanlike, Corpse corpse)
            {
                return humanlike && !MHC_Utils.IsConsideredMechanical(corpse.InnerPawn);
            }
        }

        // This transpiler ensures that butchering non-humanlike intelligences does not create a butcher thought or history event.
        [HarmonyPatch]
        public class ButcherProducts_Patch
        {
            [HarmonyPatch]
            static MethodInfo TargetMethod()
            {
                return typeof(Corpse).GetNestedTypes(AccessTools.all).First(ty => ty.Name.Contains("<ButcherProducts>")).GetMethods(AccessTools.all).First(m => PatchProcessor.GetOriginalInstructions(m).Any(inst => inst.Calls(AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike)))));
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                var type = typeof(Corpse).GetNestedTypes(AccessTools.all).First(ty => ty.Name.Contains("<ButcherProducts>"));
                var fieldInfo = type.GetFields(AccessTools.all).First(field => field.FieldType == typeof(Corpse));

                foreach (CodeInstruction inst in insts)
                {
                    if (inst.Calls(AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike))))
                    {

                        yield return inst;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, fieldInfo);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Corpse), nameof(Corpse.InnerPawn)));
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MHC_Utils), nameof(MHC_Utils.IsConsideredNonHumanlike), new Type[] { typeof(Pawn) }));
                        yield return new CodeInstruction(OpCodes.Not);
                        yield return new CodeInstruction(OpCodes.And);

                        continue;
                    }
                    yield return inst;
                }
            }
        }
    }
}