using RimWorld;
using Verse;
using Verse.AI;

namespace MechHumanlikes
{
    // Mechanical units should attempt maintenance if in poor maintenance and allowed to gain enough to escape it before doing other work.
    public class JobGiver_DoMaintenanceUrgent : ThinkNode_JobGiver
    {
        // Pawn ThinkTrees occasionally sort jobs to take on a priority. This is a high priority job, and should almost always be done ahead of work.
        public override float GetPriority(Pawn pawn)
        {
            TimeAssignmentDef timeAssignmentDef = (pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment;
            if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
            {
                return 9.25f;
            }
            else if (timeAssignmentDef == TimeAssignmentDefOf.Work)
            {
                return 8f;
            }
            else if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
            {
                return 9.25f;
            }
            else if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
            {
                return 8f;
            }
            else if (timeAssignmentDef == TimeAssignmentDefOf.Meditate)
            {
                return 11f;
            }
            return 0.5f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompMaintenanceNeed compMaintenanceNeed = pawn.GetComp<CompMaintenanceNeed>();

            if (compMaintenanceNeed == null || pawn.InAggroMentalState || !pawn.Spawned || pawn.Downed)
            {
                return null;
            }

            // Urgent maintenance is not done if the target level is below the poor maintenance threshold.
            if (compMaintenanceNeed.TargetMaintenanceLevel <= 0.3)
            {
                return null;
            }

            // If the pawn is currently doing urgent maintenance and is not at their target level, continue doing maintenance.
            if (pawn.CurJobDef == MHC_JobDefOf.MHC_DoMaintenanceUrgent && compMaintenanceNeed.TargetMaintenanceLevel > compMaintenanceNeed.MaintenanceLevel)
            {
                return JobMaker.MakeJob(MHC_JobDefOf.MHC_DoMaintenanceUrgent, pawn.Position, pawn.InBed() ? ((LocalTargetInfo)pawn.CurrentBed()) : new LocalTargetInfo(pawn.Position));
            }

            // Urgent maintenance is otherwise skipped if it is less than 30% away from the target level and above Poor.
            if (compMaintenanceNeed.MaintenanceLevel / compMaintenanceNeed.TargetMaintenanceLevel > 0.7f && compMaintenanceNeed.Stage > MHC_MaintenanceStage.Poor)
            {
                return null;
            }

            // FindMeditationSpot will find a place that is valid and will allow this job to continue. If it is invalid, then there is nowhere to do maintenance and no job is given.
            LocalTargetInfo maintenanceSpot = MaintenanceUtility.FindMaintenanceSpot(pawn);
            if (maintenanceSpot.IsValid)
            {
                return JobMaker.MakeJob(MHC_JobDefOf.MHC_DoMaintenanceUrgent, maintenanceSpot.Cell, pawn.InBed() ? ((LocalTargetInfo)pawn.CurrentBed()) : new LocalTargetInfo(maintenanceSpot.Cell));
            }
            return null;
        }
    }
}