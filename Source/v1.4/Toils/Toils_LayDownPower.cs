using Verse;
using Verse.AI;
using RimWorld;

namespace MechHumanlikes
{
    public static class Toils_LayDownPower
    {
        private const int TicksBetweenFlecks = 200;

        private static readonly FleckDef fullChargeFleck = DefDatabase<FleckDef>.GetNamed("MHC_FullChargeFleck");
        private static readonly FleckDef halfChargeFleck = DefDatabase<FleckDef>.GetNamed("MHC_HalfChargeFleck");
        private static readonly FleckDef emptyChargeFleck = DefDatabase<FleckDef>.GetNamed("MHC_EmptyChargeFleck");

        public static Toil LayDown(TargetIndex chargingBuilding, bool hasBed, bool lookForOtherJobs = true, bool canSleep = true)
        {
            Toil layDown = new Toil();

            Pawn actor = null;

            Job curJob = null;
            JobDriver curDriver = null;
            Building_Bed bed = null;
            Building_ChargingStation station = null;

            Need_Food foodNeed = null;
            Need_Rest restNeed = null;

            Thing targetBuilding = null;
            CompPowerTrader powerSource = null;
            CompPawnCharger compPawnCharger = null;

            float restEffectiveness = -1;
            float chargeRate = -1;

            // Responsible for handling information at the very start of the toil.
            layDown.initAction = delegate
            {
                actor = layDown.actor;
                actor.pather.StopDead();
                curDriver = actor.jobs.curDriver;

                // Ensure that the pawn is in a bed if it was informed it was. Set the pawn posture and inform them of the bed they are now sleeping in.
                if (hasBed)
                {
                    bed = (Building_Bed)actor.CurJob.GetTarget(chargingBuilding).Thing;
                    if (!bed.OccupiedRect().Contains(actor.Position))
                    {
                        Log.Error("[MHC] Can't start LayDown toil because pawn is not in the bed. pawn=" + actor);
                        actor.jobs.EndCurrentJob(JobCondition.Errored, true);
                        return;
                    }
                    actor.jobs.posture = PawnPosture.LayingInBed;
                    actor.mindState.lastBedDefSleptIn = bed.def;
                }
                else
                {
                    actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
                    actor.mindState.lastBedDefSleptIn = null;
                }

                // Identify the charging device responsible for this toil and notify it that this pawn is using it.
                targetBuilding = actor.CurJob.GetTarget(chargingBuilding).Thing;

                // The target building itself may have the CompPawnCharger we need.
                compPawnCharger = targetBuilding.TryGetComp<CompPawnCharger>();
                // If the power source isn't the building itself, we need to grab a linkable charger connected to this building.
                if (compPawnCharger == null)
                {
                    compPawnCharger = targetBuilding.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading?.Find(thing => thing.TryGetComp<CompPawnCharger>() != null)?.TryGetComp<CompPawnCharger>();
                }
                compPawnCharger?.Notify_ConsumerAdded(actor);

                // Initialize the sleeping tracking information, so that the pawn will receive mood buff/debuff if they sleep long enough.
                curDriver.asleep = true;
                if (actor.mindState.applyBedThoughtsTick == 0)
                {
                    actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 5000);
                    actor.mindState.applyBedThoughtsOnLeave = false;
                }

                // If the pawn does not own the bed, immediately remove positive thoughts.
                if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed)
                {
                    ThoughtUtility.RemovePositiveBedroomThoughts(actor);
                }
            };

            // Responsible for handling information done per tick of the action, including gaining energy and checking if the job fails for a particular reason.
            layDown.tickAction = delegate
            {
                // Recache values if necessary.
                if (actor == null)
                {
                    actor = layDown.actor;
                }
                if (curJob == null)
                {
                    curJob = actor.CurJob;
                }
                if (curDriver == null)
                {
                    curDriver = actor.jobs.curDriver;
                }
                if (foodNeed == null)
                {
                    foodNeed = actor.needs.food;
                }
                if (restNeed == null)
                {
                    restNeed = actor.needs.rest;
                }
                if (hasBed)
                {
                    if (bed == null)
                    {
                        bed = (Building_Bed)curJob.GetTarget(chargingBuilding).Thing;
                    }
                }
                else
                {
                    if (station == null)
                    {
                        station = (Building_ChargingStation)curJob.GetTarget(chargingBuilding).Thing;
                    }
                }
                if (targetBuilding == null)
                {
                    targetBuilding = actor.CurJob.GetTarget(chargingBuilding).Thing;
                }
                if (powerSource == null)
                {
                    if (targetBuilding.TryGetComp<CompPawnCharger>() != null)
                    {
                        powerSource = targetBuilding.TryGetComp<CompPowerTrader>();
                    }
                    // If it's not a charging bed or a charging station, we need to grab the linkable bedside charger that this bed is attached to.
                    else
                    {
                        powerSource = targetBuilding.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading?.Find(thing => thing.TryGetComp<CompPawnCharger>() != null).TryGetComp<CompPowerTrader>();
                    }
                }

                // If the pawn is not asleep, check to see if they should sleep.
                if (!curDriver.asleep && canSleep)
                {
                    // Pawn falls asleep if their rest need or food need is low enough.
                    if ((restNeed != null && RestUtility.CanFallAsleep(actor)) || (foodNeed != null && foodNeed.CurLevelPercentage < 0.9f))
                    {
                        curDriver.asleep = true;
                    }
                }

                // If the power source is offline for some reason, then abort the job. Downed pawns do not have a choice of exiting the job in this way.
                // Pawns that are using this bed for some other purpose (like maintenance) will not abort either, as charging is not their primary job here.
                if (!actor.Downed && powerSource?.PowerOn != true && lookForOtherJobs && !HealthAIUtility.ShouldSeekMedicalRest(actor) && !HealthAIUtility.ShouldHaveSurgeryDoneNow(actor))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }

                // if the pawn has their charge/rest need met (both if both exist, otherwise which ever one they have), then the job can be complete.
                if (!actor.Downed && (foodNeed == null || foodNeed.CurLevelPercentage >= 1.0f || powerSource?.PowerOn != true) && (bed == null || restNeed == null || restNeed.CurLevelPercentage >= 1.0f))
                {
                    // If the job is complete by measures of needs, terminate the job if they are uninjured and not doing this toil as part of a non-charging job.
                    if (!HealthAIUtility.ShouldSeekMedicalRest(actor) && lookForOtherJobs)
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                        return;
                    }
                    // If the pawn is injured but ready for other tasks (and isn't forced to do this one), then check if there is another preferred job.
                    else if (!actor.jobs.curJob.restUntilHealed && lookForOtherJobs && actor.IsHashIntervalTick(211))
                    {
                        actor.jobs.CheckForJobOverride();
                    }
                }

                // Increment comfort from where the pawn is resting, if applicable.
                actor.GainComfortFromCellIfPossible();

                // If the pawn can gain rest, then gain rest.
                if (restNeed != null)
                {
                    if (actor.IsHashIntervalTick(180) || restEffectiveness < 0)
                    {
                        restEffectiveness = (bed == null || !bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness)) ? StatDefOf.BedRestEffectiveness.valueIfMissing : bed.GetStatValue(StatDefOf.BedRestEffectiveness);
                    }
                    restNeed.TickResting(restEffectiveness);
                }

                // If the pawn is asleep and can charge (food not null), then charge.
                if (foodNeed != null && powerSource?.PowerOn == true)
                {
                    // Recache this value relatively frequently or calculate it if it is illegal.
                    if (actor.IsHashIntervalTick(120) || chargeRate < 0)
                    {
                        chargeRate = 0.0001f * actor.GetStatValue(MHC_StatDefOf.MHC_ChargingSpeed, cacheStaleAfterTicks: 120) * MechHumanlikes_Settings.batteryChargeRate;
                        if (chargeRate > 0)
                        {
                            // Beds get charging effectiveness from their BedRestEffectiveness stat.
                            if (bed != null)
                            {
                                chargeRate *= !bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness) ? StatDefOf.BedRestEffectiveness.valueIfMissing : bed.GetStatValue(StatDefOf.BedRestEffectiveness);
                            }
                            else
                            {
                                chargeRate *= !station.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness) ? StatDefOf.BedRestEffectiveness.valueIfMissing : station.GetStatValue(StatDefOf.BedRestEffectiveness);
                            }
                        }
                    }
                    foodNeed.CurLevel += chargeRate;
                }

                // If the apply bed thought timer is up, set applyBedThoughtsOnLeave to true so that it will apply when done with the job.
                if (actor.mindState.applyBedThoughtsTick != 0 && actor.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
                {
                    actor.mindState.applyBedThoughtsTick += 20000;
                    actor.mindState.applyBedThoughtsOnLeave = true;
                }

                // Throw the appropriate flecks when appropriate. Charging flecks when charging (energy < rest), ZZZ's when resting (rest < energy), healing when injured and can naturally heal.
                if (actor.IsHashIntervalTick(TicksBetweenFlecks) && !actor.Position.Fogged(actor.Map))
                {
                    if (restNeed != null)
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.SleepZ);
                    }
                    if (foodNeed != null && powerSource?.PowerOn == true)
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, GetChargeFleckDef(foodNeed));
                    }
                    if (actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any())
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.HealingCross);
                    }
                }

                // If the pawn is downed and this is not a valid spot to stay in, then boot them to the closest walkable spot.
                if (actor.ownership != null && bed != null && !bed.Medical && !bed.OwnersForReading.Contains(actor))
                {
                    if (actor.Downed)
                    {
                        actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1, null);
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }
            };
            layDown.defaultCompleteMode = ToilCompleteMode.Never;
            if (hasBed)
            {
                layDown.FailOnBedNoLongerUsable(chargingBuilding);
            }

            // When the job is complete (failed or succeeded), handle some necessary details.
            layDown.AddFinishAction(delegate
            {
                if (actor == null)
                {
                    actor = layDown.actor;
                }
                if (curDriver == null)
                {
                    curDriver = actor.jobs.curDriver;
                }
                curDriver.asleep = false;

                // If the pawn was asleep long enough to have thoughts applied, apply them.
                if (actor.mindState.applyBedThoughtsOnLeave)
                {
                    ApplyBedThoughts(actor);
                }

                // Notify the charger that it is no longer being used by this pawn.
                if (targetBuilding == null)
                {
                    targetBuilding = actor.CurJob.GetTarget(chargingBuilding).Thing;
                }
                if (compPawnCharger == null)
                {
                    compPawnCharger = targetBuilding.TryGetComp<CompPawnCharger>();
                    // If it's not a charging bed or a charging station, we need to grab the linkable bedside charger that this bed is attached to.
                    if (compPawnCharger == null)
                    {
                        compPawnCharger = targetBuilding.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading?.Find(thing => thing.TryGetComp<CompPawnCharger>() != null)?.TryGetComp<CompPawnCharger>();
                    }
                }
                compPawnCharger?.Notify_ConsumerRemoved(actor);
            });
            return layDown;
        }

        // Return a particular charge fleck depending on the charge of the food need tracker.
        private static FleckDef GetChargeFleckDef(Need_Food energyNeed)
        {
            if (energyNeed.CurLevelPercentage >= 0.80f) { return fullChargeFleck; }
            else if (energyNeed.CurLevelPercentage >= 0.40f) { return halfChargeFleck; }
            return emptyChargeFleck;
        }

        // Apply a sleep thought to pawns once they awaken (if they slept long enough to warrant one). This is functionally identical to Core's function in Toils_LayDown.ApplyBedThoughts.
        private static void ApplyBedThoughts(Pawn actor)
        {
            if (actor.needs.mood == null)
            {
                return;
            }

            MemoryThoughtHandler memThoughtHandler = actor.needs.mood.thoughts.memories;

            Building_Bed building_Bed = actor.CurrentBed();
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
            memThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
            if (actor.GetRoom().PsychologicallyOutdoors)
            {
                memThoughtHandler.TryGainMemory(ThoughtDefOf.SleptOutside);
            }
            if (actor.AmbientTemperature < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
            {
                memThoughtHandler.TryGainMemory(ThoughtDefOf.SleptInCold);
            }
            if (actor.AmbientTemperature > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax))
            {
                memThoughtHandler.TryGainMemory(ThoughtDefOf.SleptInHeat);
            }
            if (building_Bed != null && building_Bed == actor.ownership.OwnedBed && !building_Bed.ForPrisoners && !actor.story.traits.HasTrait(TraitDefOf.Ascetic))
            {
                ThoughtDef thoughtDef = null;
                if (building_Bed.GetRoom().Role == RoomRoleDefOf.Bedroom)
                {
                    thoughtDef = ThoughtDefOf.SleptInBedroom;
                }
                else if (building_Bed.GetRoom().Role == RoomRoleDefOf.Barracks)
                {
                    thoughtDef = ThoughtDefOf.SleptInBarracks;
                }
                if (thoughtDef != null)
                {
                    int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
                    if (thoughtDef.stages[scoreStageIndex] != null)
                    {
                        memThoughtHandler.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
                    }
                }
            }
            actor.Notify_AddBedThoughts();
        }
    }
}