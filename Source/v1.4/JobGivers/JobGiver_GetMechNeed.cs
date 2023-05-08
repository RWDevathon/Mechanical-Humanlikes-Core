using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MechHumanlikes
{
    public class JobGiver_GetMechNeed : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            if (!MHC_Utils.IsConsideredMechanical(pawn))
            {
                return 0f;
            }
            return 9.5f;
        }

        // Try to find a mechanical need that needs to be satisfied, and return a job to satisfy it if it can find an item to ingest for it.
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!MHC_Utils.IsConsideredMechanical(pawn))
            {
                return null;
            }

            foreach (NeedDef needDef in MHC_Utils.cachedMechNeeds.Keys)
            {
                Need need = pawn.needs.TryGetNeed(needDef);
                if (need == null)
                {
                    continue;
                }

                // Only seek to fulfill the need if you are under the critical threshold.
                if (need.CurLevelPercentage > need.def.GetModExtension<MHC_MechanicalNeedExtension>().criticalThreshold)
                {
                    continue;
                }

                Thing item = GetNeedSatisfyingItem(pawn, needDef);
                if (item != null)
                {
                    MHC_NeedFulfillerExtension needFulfiller = item.def.GetModExtension<MHC_NeedFulfillerExtension>();
                    if (needFulfiller == null)
                    {
                        continue;
                    }
                    int desiredCount = Mathf.Max(1, Mathf.FloorToInt((need.MaxLevel - need.CurLevel) / needFulfiller.needOffsetRelations[needDef]));
                    Job job = JobMaker.MakeJob(MHC_JobDefOf.MHC_IngestMechNeed, item);
                    job.count = Mathf.Min(item.stackCount, desiredCount);
                    return job;
                }
            }
            return null;
        }

        public static Thing GetNeedSatisfyingItem(Pawn pawn, NeedDef need)
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing != null && MHC_Utils.cachedMechNeeds[need].NotNullAndContains(carriedThing.def))
            {
                return carriedThing;
            }
            for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
            {
                if (MHC_Utils.cachedMechNeeds[need].NotNullAndContains(pawn.inventory.innerContainer[i].def))
                {
                    return pawn.inventory.innerContainer[i];
                }
            }
            List<Thing> searchList = new List<Thing>();
            foreach (Thing thing in pawn.Map.listerThings.AllThings)
            {
                if (MHC_Utils.cachedMechNeeds[need].NotNullAndContains(thing.def))
                {
                    searchList.Add(thing);
                }
            }
            return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, searchList, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => pawn.CanReserve(t) && !t.IsForbidden(pawn));
        }
    }
}
