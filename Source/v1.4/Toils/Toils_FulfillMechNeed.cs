using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace MechHumanlikes
{
    public static class Toils_FulfillMechNeed
    {
        public const int MaxPawnReservations = 10;

        public static Toil PickupConsumable(TargetIndex ind, Pawn consumer)
        {
            Toil toil = ToilMaker.MakeToil("PickupConsumable");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(ind).Thing;
                if (curJob.count <= 0)
                {
                    Log.Error("[MHC] Tried to do PickupConsumable toil with job.count = " + curJob.count);
                    actor.jobs.EndCurrentJob(JobCondition.Errored);
                }
                else
                {
                    int count = Mathf.Min(thing.stackCount, curJob.count);
                    actor.carryTracker.TryStartCarry(thing, count);
                    if (thing != actor.carryTracker.CarriedThing && actor.Map.reservationManager.ReservedBy(thing, actor, curJob))
                    {
                        actor.Map.reservationManager.Release(thing, actor, curJob);
                    }
                    actor.jobs.curJob.targetA = actor.carryTracker.CarriedThing;
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        // Generate and return the action necessary to consume the item, with a timer for how long it will take and the appropriate failure checks and reservation.
        public static Toil ConsumeItem(Pawn consumer, TargetIndex consumableInd)
        {
            Toil toil = ToilMaker.MakeToil("ConsumeMechNeedFulfiller");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Thing item = actor.CurJob.GetTarget(consumableInd).Thing;
                toil.actor.pather.StopDead();
                actor.jobs.curDriver.ticksLeftThisToil = 300;
                if (item.Spawned)
                {
                    item.Map.physicalInteractionReservationManager.Reserve(consumer, actor.CurJob, item);
                }
            };
            toil.tickAction = delegate
            {
                Thing item = toil.actor.CurJob.GetTarget(consumableInd).Thing;
                if (item != null && item.Spawned)
                {
                    toil.actor.rotationTracker.FaceCell(item.Position);
                }
                toil.actor.GainComfortFromCellIfPossible();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(consumableInd);
            toil.AddFinishAction(delegate
            {
                if (consumer != null && consumer.CurJob != null)
                {
                    Thing item = consumer.CurJob.GetTarget(consumableInd).Thing;
                    if (item != null && consumer.Map.physicalInteractionReservationManager.IsReservedBy(consumer, item))
                    {
                        consumer.Map.physicalInteractionReservationManager.Release(consumer, toil.actor.CurJob, item);
                    }
                }
            });
            toil.handlingFacing = true;
            return toil;
        }

        // Actually consume the items by reducing its stack count or destroying the item if all is consumed, and apply the appropriate need changes.
        public static Toil FinalizeConsumption(Pawn consumer, TargetIndex consumableInd)
        {
            Toil toil = ToilMaker.MakeToil("FinalizeConsumption");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(consumableInd).Thing;

                // Consume item.
                if (curJob.count >= thing.stackCount)
                {
                    thing.Destroy();
                }
                else
                {
                    thing.stackCount -= curJob.count;
                    if (thing.Spawned)
                    {
                        thing.Map.listerMergeables.Notify_ThingStackChanged(thing);
                    }
                }

                // Fulfill mech needs.
                Dictionary<NeedDef, float> needFulfillment = thing.def.GetModExtension<MHC_NeedFulfillerExtension>().needOffsetRelations;
                foreach (NeedDef needDef in needFulfillment.Keys)
                {
                    Need need = actor.needs.TryGetNeed(needDef);
                    if (need == null)
                    {
                        continue;
                    }
                    need.CurLevel += needFulfillment[needDef] * curJob.count;
                }

                // Fulfill nutrition (if it should be).
                float nutritionGained = thing.GetStatValue(StatDefOf.Nutrition) * curJob.count;
                if (!consumer.Dead)
                {
                    consumer.needs.food.CurLevel += nutritionGained;
                }
                consumer.records.AddTo(RecordDefOf.NutritionEaten, nutritionGained);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}