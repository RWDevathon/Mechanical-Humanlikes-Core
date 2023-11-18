using RimWorld;
using Verse;
using Verse.AI;

namespace MechHumanlikes
{
    // Mechanical units should recharge if they can't find anything else to do - this effectively replaces default vanilla's wandering behavior.
    public class JobGiver_RechargeIdle : ThinkNode_JobGiver
    {
        // Pawn ThinkTrees occasionally sort jobs to take on a priority. This is exceedingly low priority for idle charging.
        public override float GetPriority(Pawn pawn)
        {
            return 0.6f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Food foodNeed = pawn.needs.food;

            // If the pawn can not charge or has sufficient charge, don't try giving them the job.
            if (foodNeed == null || !pawn.Spawned || pawn.InAggroMentalState || foodNeed.CurLevelPercentage >= 0.8f || !MHC_Utils.CanUseBattery(pawn))
            {
                return null;
            }

            // Attempt to locate a viable charging bed for the pawn. This can suit comfort, rest, and room needs whereas the charging station can not.
            Building_Bed bed;
            bed = MHC_Utils.GetChargingBed(pawn, pawn);
            if (bed != null)
            {
                pawn.ownership.ClaimBedIfNonMedical(bed);
                return new Job(MHC_JobDefOf.MHC_GetRecharge, new LocalTargetInfo(bed));
            }

            // Downed pawns can not use charging stations.
            if (pawn.Downed)
            {
                return null;
            }

            // Attempt to locate a viable charging station.
            Building_ChargingStation station = (Building_ChargingStation)GenClosest.ClosestThingReachable(pawn.PositionHeld, pawn.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(pawn), validator: building => building is Building_ChargingStation chargeStation && building.Position.InAllowedArea(pawn) && building.TryGetComp<CompPowerTrader>()?.PowerOn == true && chargeStation.GetOpenRechargeSpot(pawn) != IntVec3.Invalid);
            if (station != null)
            {
                return new Job(MHC_JobDefOf.MHC_GetRecharge, new LocalTargetInfo(station.GetOpenRechargeSpot(pawn)), new LocalTargetInfo(station));
            }
            return null;
        }
    }
}
