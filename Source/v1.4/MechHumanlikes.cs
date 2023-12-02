using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using RimWorld;
using System.Collections.Generic;

namespace MechHumanlikes
{
    public class MechHumanlikes : Mod
    {
        public static MechHumanlikes_Settings settings;
        public static MechHumanlikes ModSingleton { get; private set; }

        public MechHumanlikes(ModContentPack content) : base(content)
        {
            ModSingleton = this;
            new Harmony("MechHumanlikes").PatchAll(Assembly.GetExecutingAssembly());
        }

        // Handles the localization for the mod's name in the list of mods in the mod settings page.
        public override string SettingsCategory()
        {
            return "MHC_ModTitle".Translate();
        }

        // Handles actually displaying this mod's settings.
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }

    [StaticConstructorOnStartup]
    public static class MechHumanlikes_PostInit
    {
        static MechHumanlikes_PostInit()
        {
            MechHumanlikes.settings = MechHumanlikes.ModSingleton.GetSettings<MechHumanlikes_Settings>();
            MechHumanlikes.settings.StartupChecks();

            // Acquire Defs for mechanical butchering so that mechanical (non-mechanoid) units are placed in the correct categories.
            RecipeDef mechanicalDisassembly = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseMechanoid");
            RecipeDef mechanicalSmashing = DefDatabase<RecipeDef>.GetNamed("SmashCorpseMechanoid");
            RecipeDef butcherFlesh = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh");

            CompProperties_Facility bedsideChargerLinkables = MHC_ThingDefOf.MHC_BedsideChargerFacility.GetCompProperties<CompProperties_Facility>();

            // Some patches can't be run with the other harmony patches as Defs aren't loaded yet. So we patch them here.
            if (HealthCardUtility_Patch.DrawOverviewTab_Patch.Prepare())
            {
                new Harmony("MechHumanlikes").CreateClassProcessor(typeof(HealthCardUtility_Patch.DrawOverviewTab_Patch)).Patch();
            }

            // Must dynamically modify some ThingDefs based on certain qualifications.
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                // Check race to see if the thingDef is for a Pawn.
                if (thingDef.race != null)
                {
                    // Mechanical pawns do not need rest or get butchered like organics do. Mechanical pawns get the maintenance need if the extension exists and says they should have it.
                    if (MHC_Utils.IsConsideredMechanical(thingDef))
                    {
                        ThingDef corpseDef = thingDef.race?.corpseDef;
                        if (corpseDef != null)
                        {
                            // Eliminate rottable and spawnerFilth comps from mechanical corpses.
                            corpseDef.comps.RemoveAll(compProperties => compProperties is CompProperties_Rottable || compProperties is CompProperties_SpawnerFilth);

                            // Put mechanical disassembly in the machining table and crafting spot (smashing) and remove from the butcher table.
                            mechanicalDisassembly.fixedIngredientFilter.SetAllow(corpseDef, true);
                            mechanicalSmashing.fixedIngredientFilter.SetAllow(corpseDef, true);
                            butcherFlesh.fixedIngredientFilter.SetAllow(corpseDef, false);

                            // Make mechanical corpses not edible.
                            IngestibleProperties ingestibleProps = corpseDef.ingestible;
                            if (ingestibleProps != null)
                            {
                                ingestibleProps.preferability = FoodPreferability.Undefined;
                            }
                        }

                        // Cache all bleeding hediffs associated with this race so that they are accounted for in health calculations.
                        List<HediffGiverSetDef> hediffGiverSetDefs = thingDef.race.hediffGiverSets;
                        List<KeyValuePair<HediffGiver_MechBleeding, float>> targetHediffPairs = new List<KeyValuePair<HediffGiver_MechBleeding, float>>();

                        if (hediffGiverSetDefs != null)
                        {
                            foreach (HediffGiverSetDef hediffGiverSetDef in hediffGiverSetDefs)
                            {
                                foreach (HediffGiver hediffGiver in hediffGiverSetDef.hediffGivers)
                                {
                                    //if (typeof(HediffGiver_MechBleeding).IsAssignableFrom(hediffGiver.GetType()))
                                    if (hediffGiver is HediffGiver_MechBleeding mechBleedingGiver)
                                    {
                                        float criticalThreshold = -1;
                                        foreach (HediffStage hediffStage in hediffGiver.hediff.stages)
                                        {
                                            if (hediffStage.lifeThreatening)
                                            {
                                                criticalThreshold = hediffStage.minSeverity;
                                                break;
                                            }
                                        }
                                        if (criticalThreshold < 0 && hediffGiver.hediff.lethalSeverity >= 0)
                                        {
                                            criticalThreshold = hediffGiver.hediff.lethalSeverity;
                                        }
                                        else if (criticalThreshold < 0)
                                        {
                                            criticalThreshold = hediffGiver.hediff.maxSeverity;
                                        }
                                        targetHediffPairs.Add(new KeyValuePair<HediffGiver_MechBleeding, float>(mechBleedingGiver, criticalThreshold));
                                    }
                                }
                            }
                        }
                        if (targetHediffPairs.Count > 0)
                        {
                            MHC_Utils.cachedBleedingHediffGivers[thingDef.race] = targetHediffPairs;
                        }

                        // Ensure all mechanical pawns have a mechanical pawn extension, with default values.
                        if (!thingDef.HasModExtension<MHC_MechanicalPawnExtension>())
                        {
                            thingDef.modExtensions.Add(new MHC_MechanicalPawnExtension());
                        }

                        if (thingDef.GetModExtension<MHC_MechanicalPawnExtension>().needsMaintenance == true && MechHumanlikes_Settings.maintenanceNeedExists)
                        {
                            CompProperties cp = new CompProperties
                            {
                                compClass = typeof(CompMaintenanceNeed)
                            };
                            thingDef.comps.Add(cp);
                        }

                        // Drones do not have learning factors.
                        if (MHC_Utils.IsConsideredMechanicalDrone(thingDef))
                        {
                            StatModifier learningModifier = thingDef.statBases.Find(modifier => modifier.stat.defName == "GlobalLearningFactor");
                            if (learningModifier != null)
                            {
                                learningModifier.value = 0;
                            }
                            else
                            {
                                thingDef.statBases.Add(new StatModifier() { stat = StatDefOf.GlobalLearningFactor, value = 0 });
                            }

                            // Check if drones of this race may have traits. If it may, cache it for easy access elsewhere.
                            if (thingDef.GetModExtension<MHC_MechanicalPawnExtension>()?.dronesCanHaveTraits == true)
                            {
                                MHC_Utils.cachedDronesWithTraits.Add(thingDef);
                            }
                        }
                    }

                }
                // All beds should have the Restrictable comp to restrict what pawn type may use it.
                if (thingDef.IsBed)
                {
                    CompProperties cp = new CompProperties
                    {
                        compClass = typeof(CompMHC_PawnTypeRestrictable)
                    };
                    thingDef.comps.Add(cp);

                    // Non-charging beds also should have the bedside charger as a linkable building.
                    CompProperties_AffectedByFacilities linkable = thingDef.GetCompProperties<CompProperties_AffectedByFacilities>();
                    if (linkable != null && !typeof(Building_ChargingBed).IsAssignableFrom(thingDef.thingClass))
                    {
                        linkable.linkableFacilities.Add(MHC_ThingDefOf.MHC_BedsideChargerFacility);
                        bedsideChargerLinkables.linkableBuildings.Add(thingDef);
                    }
                }
                // Things that fulfill mech needs should be marked appropriately.
                if (thingDef.HasModExtension<MHC_NeedFulfillerExtension>())
                {
                    MHC_NeedFulfillerExtension ingestibleExtension = thingDef.GetModExtension<MHC_NeedFulfillerExtension>();
                    if (ingestibleExtension != null && ingestibleExtension.consumableByMechanicals && ingestibleExtension.needOffsetRelations != null)
                    {
                        foreach (NeedDef needDef in ingestibleExtension.needOffsetRelations.Keys)
                        {
                            if (ingestibleExtension.needOffsetRelations[needDef] < 0)
                            {
                                continue;
                            }

                            if (MHC_Utils.cachedMechNeeds.ContainsKey(needDef))
                            {
                                MHC_Utils.cachedMechNeeds[needDef].Add(thingDef);
                            }
                            else
                            {
                                MHC_Utils.cachedMechNeeds[needDef] = new List<ThingDef> { thingDef };
                            }
                        }
                    }

                }
            }

            // Maintenance Effect workers must have their def and extension references manually defined here as they are created via a def mod extension which is def-blind and does not self-initialize.
            foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefsListForReading)
            {
                if (hediffDef.GetModExtension<MHC_MaintenanceEffectExtension>() is MHC_MaintenanceEffectExtension effectExtension)
                {
                    if (effectExtension.MaintenanceWorkers is List<MaintenanceWorker> maintenanceWorkers)
                    {
                        foreach (MaintenanceWorker maintenanceWorker in maintenanceWorkers)
                        {
                            maintenanceWorker.def = hediffDef;
                        }
                    }
                }
            }
        }
    }
}