using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace MechHumanlikes
{
    // Vanilla Self-tending can not be enabled for player pawns if the doctor work type is completely disabled. This transpiler makes it so that mechanical units check against Mechanic instead.
    public class HealthCardUtility_Patch
    {
        [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
        public class DrawOverviewTab_Patch
        {
            [HarmonyPrepare]
            public static bool Prepare() => DefDatabase<WorkTypeDef>.AllDefsListForReading.Count > 0;

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodBase disableTargetMethod = AccessTools.Method(typeof(Pawn), nameof(Pawn.WorkTypeIsDisabled));
                MethodBase warningTargetMethod = AccessTools.Method(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.GetPriority));

                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    // Operation target hit, yield contained instructions and add null-check branch.
                    if (instructions[i].operand as MethodBase == disableTargetMethod)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawOverviewTab_Patch), nameof(CanEverSelfTend)));
                    }
                    else if (instructions[i].operand as MethodBase == warningTargetMethod)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawOverviewTab_Patch), nameof(HasSelfTendWorkTypeSet)));
                    }
                }
            }

            // Organic pawns that are not doctors have self-tend disabled. Mechanical pawns that are not mechanics have self-tend disabled.
            // Skip the original condition if mechanical to avoid double-sending warning messages.
            private static bool CanEverSelfTend (bool notDoctor, Pawn pawn)
            {
                if (!MHC_Utils.IsConsideredMechanical(pawn))
                {
                    return notDoctor;
                }
                else if (pawn.WorkTypeIsDisabled(MHC_WorkTypeDefOf.MHC_Mechanic))
                {
                    pawn.playerSettings.selfTend = false;
                }
                return false;
            }

            // If the pawn is organic and is a doctor, then no warning message will appear.
            // If the pawn is mechanical and not a mechanic, send a warning message but return true so that the organic doctor warning is still skipped.
            private static bool HasSelfTendWorkTypeSet (bool isDoctor, Pawn pawn)
            {
                if (!MHC_Utils.IsConsideredMechanical(pawn))
                {
                    return isDoctor;
                }
                else if (pawn.workSettings.GetPriority(MHC_WorkTypeDefOf.MHC_Mechanic) == 0)
                {
                    Messages.Message("MHC_MessageSelfRepairUnsatisfied".Translate(pawn.LabelShort, pawn), MessageTypeDefOf.CautionInput, historical: false);
                }
                return true;
            }
        }
    }
}