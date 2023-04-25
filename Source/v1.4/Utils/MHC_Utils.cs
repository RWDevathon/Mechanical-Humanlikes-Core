using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace MechHumanlikes
{
    public static class MHC_Utils
    {
        // GENERAL UTILITIES

        public static bool IsConsideredMechanical(Pawn pawn)
        {
            return MechHumanlikes_Settings.mechanicalRaces.Contains(pawn.def.defName);
        }

        public static bool IsConsideredMechanical(ThingDef thingDef)
        {
            return MechHumanlikes_Settings.mechanicalRaces.Contains(thingDef.defName);
        }

        public static bool IsConsideredMechanicalAnimal(Pawn pawn)
        {
            return MechHumanlikes_Settings.mechanicalAnimals.Contains(pawn.def.defName);
        }

        public static bool IsConsideredMechanicalAnimal(ThingDef thingDef)
        {
            return MechHumanlikes_Settings.mechanicalAnimals.Contains(thingDef.defName);
        }

        public static bool IsConsideredMechanicalSapient(Pawn pawn)
        {
            return MechHumanlikes_Settings.mechanicalSapients.Contains(pawn.def.defName);
        }

        public static bool IsConsideredMechanicalSapient(ThingDef thingDef)
        {
            return MechHumanlikes_Settings.mechanicalSapients.Contains(thingDef.defName);
        }

        public static bool IsConsideredMechanicalDrone(Pawn pawn)
        {
            return MechHumanlikes_Settings.mechanicalDrones.Contains(pawn.def.defName);
        }

        public static bool IsConsideredMechanicalDrone(ThingDef thingDef)
        {
            return MechHumanlikes_Settings.mechanicalDrones.Contains(thingDef.defName);
        }

        // Some pawns may be treated as non-humanlikes even if they are one, such as drones. Other mods may wish to hook into this method to add extra qualifications.
        public static bool IsConsideredNonHumanlike(Pawn pawn)
        {
            return IsConsideredMechanicalDrone(pawn);
        }

        public static MHC_PawnType GetMHC_PawnType(Pawn pawn)
        {
            if (IsConsideredMechanicalSapient(pawn))
            {
                return MHC_PawnType.Sapient;
            }
            else if (IsConsideredMechanicalDrone(pawn))
            {
                return MHC_PawnType.Drone;
            }
            else if (IsConsideredMechanicalAnimal(pawn))
            {
                return MHC_PawnType.Mechanical;
            }
            else
            {
                return MHC_PawnType.Organic;
            }
        }

        /* === POWER UTILITIES === */
        public static bool CanUseBattery(Pawn pawn)
        {
            return pawn.GetStatValue(MHC_StatDefOf.MHC_ChargingSpeed, cacheStaleAfterTicks: 10000) > 0;
        }

        // Locate the nearest available charging bed for the given pawn user, as carried by the given pawn carrier. Pawns may carry themselves here, if they are not downed.
        public static Building_Bed GetChargingBed(Pawn user, Pawn carrier)
        {
            if (user.Map == null)
                return null;

            // Downed pawns in beds may count their current bed as a charging bed if it is charge-capable.
            if (user.Downed && user == carrier && user.CurrentBed() is Building_Bed bed)
            {
                if (RestUtility.IsValidBedFor(bed, user, carrier, true) && ((bed.GetComp<CompPawnCharger>() != null && (bed.GetComp<CompPowerTrader>()?.PowerOn ?? false)) || (bed.GetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading.Any(thing => thing.TryGetComp<CompPawnCharger>() != null && (thing.TryGetComp<CompPowerTrader>()?.PowerOn ?? false)) ?? false)))
                {
                    return bed;
                }
            }
            else if (user.Downed && user == carrier)
            {
                return null;
            }

            return (Building_Bed)GenClosest.ClosestThingReachable(user.PositionHeld, user.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.Bed), PathEndMode.OnCell, TraverseParms.For(carrier), 9999f, (Thing b) => b.def.IsBed && (int)b.Position.GetDangerFor(user, user.Map) <= (int)Danger.Deadly && RestUtility.IsValidBedFor(b, user, carrier, true) && ((b.TryGetComp<CompPawnCharger>() != null && (b.TryGetComp<CompPowerTrader>()?.PowerOn ?? false)) || (b.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading.Any(thing => thing.TryGetComp<CompPawnCharger>() != null && (thing.TryGetComp<CompPowerTrader>()?.PowerOn ?? false)) ?? false)));
        }

        /* === HEALTH UTILITIES === */

        // Returns true if the provided thing is in the reserved list of repair stims or is recognized in the settings.
        public static bool IsMechanicalRepairStim(ThingDef thing)
        {
            return ReservedRepairStims.Contains(thing.defName) || MechHumanlikes_Settings.thingsAllowedAsRepairStims.Contains(thing.defName);
        }

        // RESERVED UTILITIES, INTERNAL USE ONLY
        public static HashSet<string> ReservedRepairStims = new HashSet<string> { "MHC_RepairStimSimple", "MHC_RepairStimIntermediate", "MHC_RepairStimAdvanced" };

        public static HashSet<string> ReservedBlacklistedDroneNeeds = new HashSet<string> { "Mood", "Joy", "Beauty", "Outdoors", "Indoors", "Comfort" };

        // Handle various parts of resetting drones to default status.
        public static void ReconfigureDrone(Pawn pawn)
        {
            MHC_MechanicalPawnExtension pawnExtension = pawn.def.GetModExtension<MHC_MechanicalPawnExtension>();
            if (pawnExtension?.dronesCanHaveTraits == false)
            {
                foreach (Trait trait in pawn.story.traits.allTraits.ToList())
                {
                    pawn.story.traits.RemoveTrait(trait);
                }
            }

            // Drones don't have ideos.
            pawn.ideo = null;

            // Drones always take their last name (ID #) as their nickname.
            if (pawn.Name is NameTriple name)
            {
                pawn.Name = new NameTriple(name.First, name.Last, name.Last);
            }

            // Drones have a set skill, which is taken from their mod extension if it exists. If not, it defaults to 8 (which is the default value for the extension).
            // Since drones are incapable of learning, their passions and xp does not matter. Set it to 0 for consistency's sake.
            int skillLevel = pawnExtension?.droneSkillLevel ?? 8;
            foreach (SkillRecord skillRecord in pawn.skills.skills)
            {
                skillRecord.passion = 0;
                skillRecord.Level = skillLevel;
                skillRecord.xpSinceLastLevel = 0;
            }

            // If pawn kind backstories should be overwritten, then try to take it from the mod extension.
            if (pawnExtension?.letPawnKindHandleDroneBackstories == false)
            {
                pawn.story.Childhood = pawnExtension.droneChildhoodBackstoryDef;
                pawn.story.Adulthood = pawnExtension.droneAdulthoodBackstoryDef;
                pawn.workSettings.Notify_DisabledWorkTypesChanged();
                pawn.skills.Notify_SkillDisablesChanged();
            }
        }

        // Caches and returns the HediffDefs contained in the HediffGiver of a given RaceProperties.
        public static HashSet<HediffDef> GetTemperatureHediffDefsForRace(RaceProperties raceProperties)
        {
            if (raceProperties == null)
            {
                Log.Error("[MHC] A pawn with null raceProperties tried to have those properties checked!");
                return new HashSet<HediffDef>();
            }

            try
            {
                if (!cachedTemperatureHediffs.ContainsKey(raceProperties))
                {
                    List<HediffGiverSetDef> hediffGiverSetDefs = raceProperties.hediffGiverSets;
                    HashSet<HediffDef> targetHediffs = new HashSet<HediffDef>();

                    if (hediffGiverSetDefs == null)
                    {
                        cachedTemperatureHediffs[raceProperties] = targetHediffs;
                        return targetHediffs;
                    }

                    foreach (HediffGiverSetDef hediffGiverSetDef in hediffGiverSetDefs)
                    {
                        foreach (HediffGiver hediffGiver in hediffGiverSetDef.hediffGivers)
                        {
                            if (typeof(HediffGiver_Heat).IsAssignableFrom(hediffGiver.GetType()) || typeof(HediffGiver_Hypothermia).IsAssignableFrom(hediffGiver.GetType()))
                            {
                                targetHediffs.Add(hediffGiver.hediff);
                            }
                        }
                    }
                    cachedTemperatureHediffs[raceProperties] = targetHediffs;
                }
                return cachedTemperatureHediffs[raceProperties];
            }
            catch (Exception ex)
            {
                Log.Warning("[MHC] Encountered an error while trying to get temperature HediffDefs for a specific race. Returning empty set." + ex.Message + ex.StackTrace);
                return new HashSet<HediffDef>();
            }
        }

        // Get all maintenance effects for the given race from the appropriate dictionary as a HashSet, generating the HashSet as needed.
        public static HashSet<HediffDef> GetMaintenanceEffectsForRace(RaceProperties race)
        {
            if (race == null)
            {
                Log.Error("[MHC] A pawn with null RaceProperties tried to have those properties checked!");
                return new HashSet<HediffDef>();
            }

            // Acquire the enumerable of all maintenance effect hediffs if it hasn't been initialized yet.
            if (allMaintenanceHediffs == null)
            {
                allMaintenanceHediffs = DefDatabase<HediffDef>.AllDefsListForReading.Where(hediffDef => hediffDef.HasModExtension<MHC_MaintenanceEffectExtension>());
            }

            try
            {
                // Generate the HashSet if one does not exist already.
                if (!cachedMaintenanceHediffs.ContainsKey(race))
                {
                    HashSet<HediffDef> validHediffs = new HashSet<HediffDef>();
                    // Scan all HediffDefs and add them to the HashSet if they are legal.
                    foreach (HediffDef hediffDef in allMaintenanceHediffs)
                    {
                        // The HediffDef is guaranteed to have the mod extension as that is a pre-requisite condition for being in the enumerable.
                        MHC_MaintenanceEffectExtension effectExtension = hediffDef.GetModExtension<MHC_MaintenanceEffectExtension>();

                        // If the maintenance worker specifies this race is invalid for this Hediff, move on.
                        if (!effectExtension.MaintenanceWorkers.NullOrEmpty() && effectExtension.MaintenanceWorkers.Any(maintenanceWorker => !maintenanceWorker.CanEverApplyTo(race)))
                        {
                            continue;
                        }

                        // If the extension has its whitelist defined, ensure the race is included in that list.
                        if (effectExtension.racesToAffect != null && !effectExtension.racesToAffect.Any(thingDef => thingDef.race == race))
                        {
                            continue;
                        }

                        // If the extension is stage-related and isn't race-restricted/worker-restricted, allow it.
                        if (effectExtension.isMaintenangeStageEffect)
                        {
                            validHediffs.Add(hediffDef);
                            continue;
                        }

                        // If the extension defines specific parts to affect and the race has any of those parts, it may be applied.
                        if (effectExtension.partsToAffect != null)
                        {
                            bool locatedViablePart = false;
                            foreach (BodyPartDef partDef in effectExtension.partsToAffect)
                            {
                                if (race.body.GetPartsWithDef(partDef).Count > 0)
                                {
                                    validHediffs.Add(hediffDef);
                                    locatedViablePart = true;
                                    break;
                                }
                            }
                            // If this hediff is valid here, no need to run other checks.
                            if (locatedViablePart)
                            {
                                continue;
                            }
                        }

                        // If the extension defines specific part tags to affect and the race has any of that tagged part, it may be applied.
                        if (effectExtension.bodyPartTagsToAffect != null)
                        {
                            bool locatedViableTag = false;
                            foreach (BodyPartTagDef tagDef in effectExtension.bodyPartTagsToAffect)
                            {
                                if (race.body.HasPartWithTag(tagDef))
                                {
                                    validHediffs.Add(hediffDef);
                                    locatedViableTag = true;
                                    break;
                                }
                            }
                            // If this hediff is valid here, no need to run other checks.
                            if (locatedViableTag)
                            {
                                continue;
                            }
                        }

                        // If the extension defines a depth to affect and the race has any part with that depth, it may be applied.
                        if (effectExtension.partDepthToAffect != BodyPartDepth.Undefined)
                        {
                            if (race.body.AllParts.Any(partRecord => partRecord.depth == effectExtension.partDepthToAffect))
                            {
                                validHediffs.Add(hediffDef);
                            }
                        }
                    }
                    cachedMaintenanceHediffs[race] = validHediffs;
                }
                return cachedMaintenanceHediffs[race];
            }
            catch (Exception ex)
            {
                Log.Error("[MHC] Encountered an error while trying to generate and return maintenance-related HediffDefs for a race. Additional errors may occur! " + ex.Message + ex.StackTrace);
                return new HashSet<HediffDef>();
            }
        }

        // Cached Hediffs for a particular pawn's race that count as temperature hediffs to avoid recalculation, cached when needed.
        private static Dictionary<RaceProperties, HashSet<HediffDef>> cachedTemperatureHediffs = new Dictionary<RaceProperties, HashSet<HediffDef>>();

        // Cached Hediff Set for all maintenance effect Hediffs and a dictionary matching RaceProperties to the valid maintenance effects for that race.
        private static IEnumerable<HediffDef> allMaintenanceHediffs;
        private static Dictionary<RaceProperties, HashSet<HediffDef>> cachedMaintenanceHediffs = new Dictionary<RaceProperties, HashSet<HediffDef>>();
    }
}
