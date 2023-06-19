using Verse;
using HarmonyLib;
using System;
using RimWorld.Planet;

namespace MechHumanlikes
{
    public class Pawn_HealthTracker_Patch
    {
        // Ensure the hediff to be added is not forbidden on the given pawn (for mechanicals) before doing standard AddHediff checks - it would be wasted/junk calculations.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff")]
        [HarmonyPatch(new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
        public class AddHediff_Patch
        { 
            [HarmonyPrefix]
            public static bool Prefix(ref Pawn ___pawn, ref Hediff hediff, BodyPartRecord part)
            {
                // If this is a mechanical pawn and this particular hediff is forbidden for mechanicals to have, then abort trying to add it.
                if (MHC_Utils.IsConsideredMechanical(___pawn) && MechHumanlikes_Settings.blacklistedMechanicalHediffs.Contains(hediff.def.defName))
                {
                    return false;
                }

                return true;
            }
        }

        // Pawns with a non-humanlike intelligence should not send letters to the player about their deaths.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "NotifyPlayerOfKilled")]
        public class NotifyPlayerOfKilled_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref DamageInfo? dinfo, ref Hediff hediff, ref Caravan caravan, Pawn ___pawn)
            {
                // If the pawn is a surrogate and wasn't just turned into one, then abort.
                if (MHC_Utils.IsConsideredNonHumanlike(___pawn))
                {
                    return false;
                }

                return true;
            }
        }

        // Mechanical pawns do not die from the lethal damage threshold mechanic.
        [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDeadFromLethalDamageThreshold")]
        public class ShouldBeDeadFromLethalDamageThreshold_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn ___pawn)
            {
                if (MHC_Utils.IsConsideredMechanical(___pawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }

        }
    }
}