using System;
using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class JobGiver_GetFood_Patch
    {
        // Override job for getting food based on whether the pawn can charge instead, and on whether the pawn can effectively eat at all.
        [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob")]
        public class TryGiveJob_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref Job __result)
            {
                try
                {
                    // If this is an illegal state, let other methods take care of it.
                    if (pawn == null || pawn.Map == null)
                    {
                        return true;
                    }

                    float nutritionalEfficiency = pawn.GetStatValue(MHC_StatDefOf.MHC_NutritionalIntakeEfficiency);
                    float chargingEfficiency = pawn.GetStatValue(MHC_StatDefOf.MHC_ChargingSpeed);

                    // If the pawn can neither charge nor consume food, return no job and don't bother looking for food or a charging spot.
                    if (nutritionalEfficiency <= 0 && chargingEfficiency <= 0)
                    {
                        __result = null;
                        return false;
                    }

                    // Non-player pawns as well as pawns that can not charge do not need to search for a charging spot and can use normal behavior. Do not override non-spawned or drafted pawns.
                    if ((pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer) || chargingEfficiency <= 0 || !pawn.Spawned || pawn.Drafted)
                    {
                        return true;
                    }

                    // Attempt to locate a viable charging bed for the pawn. This can suit comfort, rest, and room needs whereas the charging station can not.
                    Building_Bed bed;
                    bed = MHC_Utils.GetChargingBed(pawn, pawn);
                    if (bed != null)
                    {
                        pawn.ownership.ClaimBedIfNonMedical(bed);
                        __result = new Job(MHC_JobDefOf.MHC_GetRecharge, new LocalTargetInfo(bed));
                        return false;
                    }

                    // Attempt to locate a viable charging station. Set the result to this if one is found.
                    Building_ChargingStation station = (Building_ChargingStation)GenClosest.ClosestThingReachable(pawn.PositionHeld, pawn.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(pawn), validator: building => building is Building_ChargingStation chargeStation && building.Position.InAllowedArea(pawn) && building.TryGetComp<CompPowerTrader>()?.PowerOn == true && chargeStation.GetOpenRechargeSpot(pawn) != IntVec3.Invalid);
                    if (station != null)
                    {
                        __result = new Job(MHC_JobDefOf.MHC_GetRecharge, new LocalTargetInfo(station.GetOpenRechargeSpot(pawn)), new LocalTargetInfo(station));
                        return false;
                    }

                    // If there is no viable charging bed or charging station and the pawn can not eat food, give no job.
                    if (nutritionalEfficiency <= 0)
                    {
                        __result = null;
                        return false;
                    }
                    // If the pawn can eat food, then allow normal behavior to continue.
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("[MHC] MechHumanlikes.JobGiver_GetFood_Patch Encountered an error while attempting to check pawn" + pawn + " for charging. Default vanilla behavior will proceed." + ex.Message + " " + ex.StackTrace);
                    return true;
                }
            }
        }
    }
}