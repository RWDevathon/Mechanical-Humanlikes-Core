using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MechHumanlikes
{
    // Simple ThingComp that allows pawns to consume it directly via an order to fulfill a mech need.
    public class CompMechNeedFulfiller : ThingComp
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            base.CompFloatMenuOptions(selPawn);
            // No reason to force organics to consume an item in this way.
            if (!MHC_Utils.IsConsideredMechanical(selPawn))
            {
                yield break;
            }

            // Force consuming one of this item.
            yield return new FloatMenuOption("MHC_ForceConsumption".Translate(parent.LabelNoCount), delegate () {
                Job job = JobMaker.MakeJob(MHC_JobDefOf.MHC_IngestMechNeed, parent);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
            });

            // Force consuming multiple of this item - a dialog slider will appear to select how much to consume.
            yield return new FloatMenuOption("MHC_ForceConsumptionMultiple".Translate(parent.LabelNoCount), delegate ()
            {
                Dialog_Slider selectorWindow = new Dialog_Slider("MHC_ConsumeCount".Translate(parent.LabelNoCount, parent), 1, parent.stackCount, delegate (int count)
                {
                    Job job = JobMaker.MakeJob(MHC_JobDefOf.MHC_IngestMechNeed, parent);
                    job.count = count;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
                });
                Find.WindowStack.Add(selectorWindow);
            });
        }
    }
}