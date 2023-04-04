using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;

namespace MechHumanlikes
{
    public class WorkGiver_DoBill_Patch
    {
        // Listen for doctors/mechanics doing a work bill, and make sure they select an appropriate medicine for their task.
        [HarmonyPatch(typeof(WorkGiver_DoBill), "AddEveryMedicineToRelevantThings")]
        public class AddEveryMedicineToRelevantThings_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, Thing billGiver, ref List<Thing> relevantThings, Predicate<Thing> baseValidator, Map map)
            {
                try
                {
                    // If all medicines may be used for any operation, then no reason to remove any medicines from any operation.
                    if (!MechHumanlikes_Settings.medicinesAreInterchangeable && billGiver is Pawn)
                    {
                        // If the patient is a mechanical unit, make sure to use a mechanical-compatible medicine (Reserved Repair Stims or additionally by settings)
                        if (MHC_Utils.IsConsideredMechanical(billGiver.def))
                        {
                            relevantThings.RemoveAll(thing => !MHC_Utils.IsMechanicalRepairStim(thing.def));
                        }
                        // If the patient is not mechanical, do not allow it to use Repair Stims. Other medicines will be handled by vanilla code.
                        else
                        {
                            relevantThings.RemoveAll(thing => MHC_Utils.IsMechanicalRepairStim(thing.def));
                        }
                    }
                }
                catch(Exception e)
                {
                    Log.Message("[MHC] WorkGiver_DoBill.AddEveryMedicineToRelevantThings " + e.Message + " " + e.StackTrace);
                }
            }
        }

        // Forbid doctors from working on mechanicals and mechanics from working on organics.
        [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
        public class JobOnThing_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, Thing thing, bool forced, ref Job __result, WorkGiver_DoBill __instance)
            {
                if (__result == null)
                {
                    return;
                }
                else if (__instance.def.workType == WorkTypeDefOf.Doctor && thing is Pawn patient && MHC_Utils.IsConsideredMechanical(patient))
                {
                    __result = null;
                }
                else if (__instance.def.workType == MHC_WorkTypeDefOf.MHC_Mechanic && thing is Pawn unit && !MHC_Utils.IsConsideredMechanical(unit))
                {
                    __result = null;
                }
            }
        }
    }
}