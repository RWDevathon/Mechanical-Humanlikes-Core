using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using System.Text;

namespace MechHumanlikes
{
    public class HealthCardUtility_Patch
    {
        // Vanilla Self-tending can not be enabled for player pawns if the doctor work type is completely disabled. This transpiler makes it so that mechanical units check against Mechanic instead.
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

        // The hediff listing for mechanical units will incorrectly detail bleeding as if they are humans. This transpiler replaces the bleeding summary for that panel.
        [HarmonyPatch(typeof(HealthCardUtility), "DrawHediffListing")]
        public class DrawHediffListing_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo bleedRateProperty = AccessTools.PropertyGetter(typeof(HediffSet), nameof(HediffSet.BleedRateTotal));
                bool bleedRateFound = false;
                Label? labelBeyondBleedCode = null;
                bool complete = false;

                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (complete)
                    {
                        yield return instructions[i];
                        continue;
                    }

                    // If we have the property and the label to skip to, then our code should be inserted here.
                    if (bleedRateFound && labelBeyondBleedCode != null)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load Rect
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // Load pawn
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 4); // Load bleedRateTotal
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawHediffListing_Patch), nameof(GetBleedingSummary))); // Call our method
                        yield return new CodeInstruction(OpCodes.Brtrue, labelBeyondBleedCode); // If the method is true (it was a mechanical pawn) then branch out and skip the organic path.
                        complete = true;
                    }
                    yield return instructions[i];
                    // We are attempting to override behavior that happens after the bleed rate is found.
                    if (instructions[i].Calls(bleedRateProperty))
                    {
                        bleedRateFound = true;
                    }
                    else if (bleedRateFound && instructions[i].Branches(out labelBeyondBleedCode))
                    {
                    }
                }
            }

            // Method containing the logic for printing the bleeding coolant summary. Returns true if the pawn is mechanical and is handled.
            private static bool GetBleedingSummary(Rect rect, Pawn pawn, float bleedRateTotal)
            {
                if (MHC_Utils.IsConsideredMechanical(pawn))
                {
                    List<KeyValuePair<HediffGiver_MechBleeding, float>> bleedingHediffGivers = MHC_Utils.cachedBleedingHediffGivers[pawn.RaceProps];
                    StringBuilder summaryText = new StringBuilder();
                    int lines = 0;

                    foreach (KeyValuePair<HediffGiver_MechBleeding, float> hediffPair in bleedingHediffGivers)
                    {
                        string lineText = hediffPair.Key.hediff.LabelCap + ": " + (bleedRateTotal * hediffPair.Key.riseRatePerDay).ToStringPercent() + "/" + "LetterDay".Translate();
                        
                        int ticksToDanger = MHC_Utils.TicksUntilCriticalFailure(pawn, hediffPair);
                        if (ticksToDanger == 0)
                        {
                            lineText += " (" + "MHC_CriticalReached".Translate() + ")";
                        }
                        else if (ticksToDanger <= GenDate.TicksPerDay)
                        {
                            lineText += " (" + "MHC_HoursToFailure".Translate(ticksToDanger.ToStringTicksToPeriod()) + ")";
                        }
                        else
                        {
                            lineText += " (" + "WontBleedOutSoon".Translate() + ")";
                        }
                        summaryText.AppendLine(lineText);
                        lines++;
                    }

                    if (lines > 0)
                    {
                        Rect summaryRect = new Rect(0f, rect.height - (Text.LineHeight * lines), rect.width, Text.LineHeight * lines);
                        Widgets.Label(summaryRect, summaryText.ToString());
                    }
                    return true;
                }
                return false;
            }
        }

        // We want to replace the bleeding icon for particular units. This transpiler replaces the vanilla BleedingIcon with a method that accounts for our purposes.
        [HarmonyPatch(typeof(HealthCardUtility), "DrawHediffRow")]
        public class DrawHediffRow_Patch
        {

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo bleedingProperty = AccessTools.PropertyGetter(typeof(Hediff), nameof(Hediff.Bleeding));
                bool replaceNextLoadField = false;
                bool complete = false;

                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    // We are attempting to override behavior that happens after we have confirmed this hediff involves bleeding.
                    if (!complete && instructions[i].Calls(bleedingProperty))
                    {
                        replaceNextLoadField = true;
                    }
                    
                    if (replaceNextLoadField && instructions[i].opcode == OpCodes.Ldsfld)
                    {
                        replaceNextLoadField = false;
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // Load pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawHediffRow_Patch), nameof(GetBleedingIcon))); // Call our method
                        complete = true;
                    }
                    else
                    {
                        yield return instructions[i];
                    }
                }
            }

            // Method containing the logic for getting the bleedingIcon for mechanical pawns. Returns true if the pawn is mechanical and is handled.
            // The bleedingIcon initially is null, and can exit the method null safely. The first HediffGiver_MechBleeding wins.
            private static Texture2D GetBleedingIcon(Pawn pawn)
            {
                if (MHC_Utils.IsConsideredMechanical(pawn))
                {
                    if (MHC_Utils.cachedBleedingHediffGivers.TryGetValue(pawn.RaceProps, out List<KeyValuePair<HediffGiver_MechBleeding, float>> bleedingHediffGivers))
                    {
                        if (bleedingHediffGivers.NullOrEmpty())
                        {
                            return null;
                        }
                        else
                        {
                            return bleedingHediffGivers[0].Key.Icon;
                        }
                    }
                    return null;
                }
                else
                {
                    return MHC_Textures.VanillaBleedIcon;
                }
            }
        }
    }
}