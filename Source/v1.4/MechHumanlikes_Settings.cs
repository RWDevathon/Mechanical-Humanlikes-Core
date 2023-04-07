using UnityEngine;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MechHumanlikes
{
    public class MechHumanlikes_Settings : ModSettings
    {
        // GENERAL SETTINGS
            // Settings for Permissions
        public static HashSet<string> thingsAllowedAsRepairStims = new HashSet<string> { };
        public static HashSet<string> blacklistedMechanicalHediffs = new HashSet<string> { };
        public static HashSet<string> blacklistedMechanicalTraits = new HashSet<string> { };
        public static bool bedRestrictionDefaultsToAll;

            // Settings for what is considered mechanical
        public static bool isUsingCustomConsiderations;
        public static HashSet<string> mechanicalAnimals = new HashSet<string>();
        public static HashSet<string> mechanicalSapients = new HashSet<string>();
        public static HashSet<string> mechanicalDrones = new HashSet<string>();
        public static HashSet<string> mechanicalRaces = new HashSet<string>();

            // Settings for battery charge rate
        public static float batteryChargeRate;

        // HEALTH SETTINGS
            // Settings for Surgeries
        public static bool medicinesAreInterchangeable = false;
        public static bool showMechanicalSurgerySuccessChance = false;
        public static float maxChanceMechanicOperationSuccess = 1.0f;
        public static float chanceFailedOperationMinor = 0.75f;
        public static float chancePartSavedOnFailure = 0.75f;

            // Settings for Maintenance
        public static bool maintenanceNeedExists = true;
        public static float maintenanceFallRateFactor = 1.0f;
        public static float maintenanceGainRateFactor = 1.0f;

        // STATS SETTINGS

        // INTERNAL SETTINGS
            // Settings page
        public MHC_OptionsTab activeTab = MHC_OptionsTab.General;
        public MHC_SettingsPreset ActivePreset = MHC_SettingsPreset.None;
        public bool settingsEverOpened = false;

        public virtual void StartupChecks()
        {
            if (ActivePreset == MHC_SettingsPreset.None)
            {
                settingsEverOpened = false;
                ApplyPreset(MHC_SettingsPreset.Default);
            }
            if (!isUsingCustomConsiderations)
            {
                RebuildCaches();
            }
        }

        Vector2 scrollPosition = Vector2.zero;
        float cachedScrollHeight = 0;

        bool cachedExpandFirst = true;
        bool cachedExpandSecond = true;
        bool cachedExpandThird = true;

        void ResetCachedExpand()
        {
            cachedExpandFirst = true;
            cachedExpandSecond = true;
            cachedExpandThird = true;
        }

        public virtual void DoSettingsWindowContents(Rect inRect)
        {
            settingsEverOpened = true;
            bool hasChanged = false;
            void onChange() { hasChanged = true; }
            void onConsiderationChange()
            {
                onChange();
                isUsingCustomConsiderations = true;
            }

            Color colorSave = GUI.color;
            TextAnchor anchorSave = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;

            var headerRect = inRect.TopPartPixels(50);
            var restOfRect = new Rect(inRect);
            restOfRect.y += 50;
            restOfRect.height -= 50;

            Listing_Standard prelist = new Listing_Standard();
            prelist.Begin(headerRect);

            prelist.EnumSelector("MHC_SettingsTabTitle".Translate(), ref activeTab, "MHC_SettingsTabOption_", valueTooltipPostfix: null, onChange: ResetCachedExpand);
            prelist.GapLine();

            prelist.End();

            bool needToScroll = cachedScrollHeight > inRect.height;
            var viewRect = new Rect(restOfRect);
            if (needToScroll)
            {
                viewRect.width -= 20f;
                viewRect.height = cachedScrollHeight;
                Widgets.BeginScrollView(restOfRect, ref scrollPosition, viewRect);
            }

            Listing_Standard listingStandard = new Listing_Standard
            {
                maxOneColumn = true
            };
            listingStandard.Begin(viewRect);

            switch (activeTab)
            {
                case MHC_OptionsTab.General:
                {
                    // PRESET SETTINGS
                    if (listingStandard.ButtonText("MHC_ApplyPreset".Translate()))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (MHC_SettingsPreset s in Enum.GetValues(typeof(MHC_SettingsPreset)))
                        {
                            if (s == MHC_SettingsPreset.None) // Can not apply the None preset.
                            {
                                continue;
                            }
                            options.Add(new FloatMenuOption(("MHC_SettingsPreset" + s.ToString()).Translate(), () => ApplyPreset(s)));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                    listingStandard.GapLine();

                    // PERMISSION SETTINGS
                    listingStandard.CheckboxLabeled("MHC_bedRestrictionDefaultsToAll".Translate(), ref bedRestrictionDefaultsToAll, tooltip: "MHC_bedRestrictionDefaultsToAllDesc".Translate(), onChange: onChange);
                    listingStandard.GapLine();

                    // CONSIDERATION SETTINGS
                    listingStandard.Label("MHC_RestartRequiredSection".Translate());
                    if (listingStandard.ButtonTextLabeled("MHC_isUsingCustomConsiderations".Translate(isUsingCustomConsiderations.ToString()), "MHC_resetCustomConsiderations".Translate(), tooltip: "MHC_isUsingCustomConsiderationsDesc".Translate()))
                    {
                            RebuildCaches();
                            isUsingCustomConsiderations = false;
                    }

                    if (listingStandard.ButtonText("MHC_ExpandMenu".Translate()))
                    {
                            cachedExpandFirst = !cachedExpandFirst;
                    }
                    if (cachedExpandFirst)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Humanlike), mechanicalSapients, "MHC_SettingsConsideredMechanicalSapient".Translate(), "MHC_SettingsNotConsideredMechanical".Translate(), onConsiderationChange);

                    if (listingStandard.ButtonText("MHC_ExpandMenu".Translate()))
                    {
                        cachedExpandSecond = !cachedExpandSecond;
                    }
                    if (cachedExpandSecond)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Humanlike), mechanicalDrones, "MHC_SettingsConsideredMechanicalDrone".Translate(), "MHC_SettingsNotConsideredMechanical".Translate(), onConsiderationChange);

                    if (listingStandard.ButtonText("MHC_ExpandMenu".Translate()))
                    {
                        cachedExpandThird = !cachedExpandThird;
                    }
                    if (cachedExpandThird)
                        listingStandard.PawnSelector(FilteredGetters.FilterByIntelligence(FilteredGetters.GetValidPawns(), Intelligence.Animal), mechanicalAnimals, "MHC_SettingsConsideredMechanicalAnimal".Translate(), "MHC_SettingsNotConsideredMechanical".Translate(), onConsiderationChange);

                    listingStandard.GapLine();

                    listingStandard.SliderLabeled("MHC_batteryChargeRateFactor".Translate(), ref batteryChargeRate, 0.1f, 4f, onChange: onChange);

                    listingStandard.GapLine();
                        break;
                }
                case MHC_OptionsTab.Health:
                {
                    // MEDICAL
                    listingStandard.CheckboxLabeled("MHC_medicinesAreInterchangeable".Translate(), ref medicinesAreInterchangeable, onChange: onChange);
                    listingStandard.CheckboxLabeled("MHC_showMechanicalSurgerySuccessChance".Translate(), ref showMechanicalSurgerySuccessChance, onChange: onChange);
                    listingStandard.SliderLabeled("MHC_maxChanceMechanicOperationSuccess".Translate(), ref maxChanceMechanicOperationSuccess, 0.01f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    listingStandard.SliderLabeled("MHC_chanceFailedOperationMinor".Translate(), ref chanceFailedOperationMinor, 0.01f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    listingStandard.SliderLabeled("MHC_chancePartSavedOnFailure".Translate(), ref chancePartSavedOnFailure, 0.01f, 1f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    listingStandard.GapLine();

                    // MAINTENANCE
                    listingStandard.CheckboxLabeled("MHC_maintenanceNeedExists".Translate(), ref maintenanceNeedExists, onChange: onChange);
                    if (maintenanceNeedExists)
                    {
                        listingStandard.SliderLabeled("MHC_maintenanceFallRateFactor".Translate(), ref maintenanceFallRateFactor, 0.5f, 5f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                        listingStandard.SliderLabeled("MHC_maintenanceGainRateFactor".Translate(), ref maintenanceGainRateFactor, 0.5f, 5f, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    }
                    listingStandard.GapLine();

                    // HEDIFFS
                    listingStandard.Label("MHC_hediffBlacklistWarning".Translate());
                    if (listingStandard.ButtonText("MHC_ExpandMenu".Translate()))
                    {
                        cachedExpandFirst = !cachedExpandFirst;
                    }
                    if (!cachedExpandFirst)
                    {
                        listingStandard.DefSelector(DefDatabase<HediffDef>.AllDefsListForReading, ref blacklistedMechanicalHediffs, "MHC_settingsBlacklistedMechanicalHediffs".Translate(), "MHC_settingsAllowedMechanicalHediffs".Translate(), onChange);
                    }
                    listingStandard.GapLine();

                    break;
                }
                case MHC_OptionsTab.Stats:
                {
                    // Traits
                    listingStandard.Label("MHC_traitBlacklistWarning".Translate());
                    if (listingStandard.ButtonText("MHC_ExpandMenu".Translate()))
                    {
                        cachedExpandFirst = !cachedExpandFirst;
                    }
                    if (!cachedExpandFirst)
                    {
                        listingStandard.DefSelector(DefDatabase<TraitDef>.AllDefsListForReading, ref blacklistedMechanicalTraits, "MHC_settingsBlacklistedMechanicalTraits".Translate(), "MHC_settingsAllowedMechanicalTraits".Translate(), onChange);
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
            // Ending

            cachedScrollHeight = listingStandard.CurHeight;
            listingStandard.End();

            if (needToScroll)
            {
                Widgets.EndScrollView();
            }


            if (hasChanged)
                ApplyPreset(MHC_SettingsPreset.Custom);

            GUI.color = colorSave;
            Text.Anchor = anchorSave;
        }

        public void ApplyBaseSettings()
        {
            // Permissions
            thingsAllowedAsRepairStims = new HashSet<string> { };
            blacklistedMechanicalHediffs = new HashSet<string> { };
            blacklistedMechanicalTraits = new HashSet<string> { };
            bedRestrictionDefaultsToAll = false;

            // Considerations
            isUsingCustomConsiderations = false;

            // Power
            batteryChargeRate = 1f;

            // HEALTH SETTINGS
                // Medical
            medicinesAreInterchangeable = false;
            showMechanicalSurgerySuccessChance = false;
            maxChanceMechanicOperationSuccess = 1f;
            chanceFailedOperationMinor = 0.75f;
            chancePartSavedOnFailure = 0.75f;

            // Maintenance
            maintenanceNeedExists = true;
            maintenanceFallRateFactor = 1.0f;
            maintenanceGainRateFactor = 1.0f;

            RebuildCaches();
        }

        public void ApplyPreset(MHC_SettingsPreset preset)
        {
            if (preset == MHC_SettingsPreset.None)
                throw new InvalidOperationException("[MHC] Applying the None preset is illegal - were the mod options properly initialized?");

            ActivePreset = preset;
            if (preset == MHC_SettingsPreset.Custom) // Custom settings are inherently not a preset, so apply no new settings.
                return;

            ApplyBaseSettings();

            switch (preset)
            {
                case MHC_SettingsPreset.Default:
                    break;
                default:
                    throw new InvalidOperationException("Attempted to apply a nonexistent preset.");
            }

        }

        // Caches for ThingDefs must be rebuilt manually. Configuration uses the MHC_MechanicalPawnExtension by default and will capture all pawn thing defs with that modExtension.
        protected virtual void RebuildCaches()
        {
            IEnumerable<ThingDef> validPawns = FilteredGetters.GetValidPawns();

            HashSet<string> matchingSapients = new HashSet<string>();
            HashSet<string> matchingDrones = new HashSet<string>();
            HashSet<string> matchingMechanicals = new HashSet<string>();
            HashSet<string> matchingChargers = new HashSet<string>();
            foreach (ThingDef validHumanlike in FilteredGetters.FilterByIntelligence(validPawns, Intelligence.Humanlike).Where(thingDef => thingDef.HasModExtension<MHC_MechanicalPawnExtension>()))
            {
                MHC_MechanicalPawnExtension modExt = validHumanlike.GetModExtension<MHC_MechanicalPawnExtension>();
                // Mechanical sapients are humanlikes with global learning factor >= 0.5 that have the ModExtension. Or are simply marked as canBeSapient and not canBeDrone.
                if (modExt.canBeSapient && (validHumanlike.statBases?.GetStatValueFromList(StatDefOf.GlobalLearningFactor, 0.5f) >= 0.5f || !modExt.canBeDrone))
                {
                    matchingSapients.Add(validHumanlike.defName);

                    // All mechanical humanlikes may charge inherently.
                    matchingChargers.Add(validHumanlike.defName);
                    matchingMechanicals.Add(validHumanlike.defName);
                }
                // Mechanical Drones are humanlikes with global learning factor < 0.5 that have the ModExtension. Or are simply marked as canBeDrone and not canBeSapient.
                else if (modExt.canBeDrone && (validHumanlike.statBases?.GetStatValueFromList(StatDefOf.GlobalLearningFactor, 0.5f) < 0.5f || !modExt.canBeSapient))
                {
                    matchingDrones.Add(validHumanlike.defName);
                    // All mechanical humanlikes may charge inherently.
                    matchingChargers.Add(validHumanlike.defName);
                    matchingMechanicals.Add(validHumanlike.defName);
                }
                else
                {
                    Log.Warning("[MHC] A humanlike race " + validHumanlike + " with the MHC_MechanicalPawnExtension mod extension was unable to automatically select its categorization! This will leave it as being considered organic.");
                }
            }
            // Mechanical animals are animals that have the ModExtension
            HashSet<ThingDef> validAnimals = FilteredGetters.FilterByIntelligence(validPawns, Intelligence.Animal).Where(thingDef => thingDef.HasModExtension<MHC_MechanicalPawnExtension>()).ToHashSet();
            HashSet<string> matchingAnimals = new HashSet<string>();

            // Mechanical animals of advanced intelligence may charge.
            foreach (ThingDef validAnimal in validAnimals)
            {
                matchingAnimals.Add(validAnimal.defName);
                matchingMechanicals.Add(validAnimal.defName);
                // Advanced mechanical animals may charge.
                if (validAnimal.race.trainability == TrainabilityDefOf.Advanced)
                    matchingChargers.Add(validAnimal.defName);
            }

            mechanicalSapients = matchingSapients;
            mechanicalDrones = matchingDrones;
            mechanicalAnimals = matchingAnimals;
            mechanicalRaces = matchingMechanicals;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            /* == public === */
            Scribe_Values.Look(ref ActivePreset, "MHC_ActivePreset", MHC_SettingsPreset.None, true);

            /* === GENERAL === */

            // Permissions
            Scribe_Collections.Look(ref thingsAllowedAsRepairStims, "MHC_thingsAllowedAsRepairStims", LookMode.Value);
            Scribe_Collections.Look(ref blacklistedMechanicalHediffs, "MHC_blacklistedMechanicalHediffs", LookMode.Value);
            Scribe_Collections.Look(ref blacklistedMechanicalTraits, "MHC_blacklistedMechanicalTraits", LookMode.Value);
            Scribe_Values.Look(ref bedRestrictionDefaultsToAll, "MHC_bedRestrictionDefaultsToAll", false);

            // Considerations
            Scribe_Values.Look(ref isUsingCustomConsiderations, "MHC_isUsingCustomConsiderations", false);
            try
            {
                Scribe_Collections.Look(ref mechanicalAnimals, "MHC_isConsideredMechanicalAnimal", LookMode.Value);
                Scribe_Collections.Look(ref mechanicalSapients, "MHC_isConsideredMechanicalSapient", LookMode.Value);
                Scribe_Collections.Look(ref mechanicalDrones, "MHC_isConsideredMechanicalDrone", LookMode.Value);
                Scribe_Collections.Look(ref mechanicalRaces, "MHC_isConsideredMechanical", LookMode.Value);
            }
            catch (Exception ex)
            {
                Log.Warning("[MHC] Mod settings failed to load appropriately! Resetting to default to avoid further issues. " + ex.Message + " " + ex.StackTrace);
                RebuildCaches();
            }

            // Power
            Scribe_Values.Look(ref batteryChargeRate, "MHC_batteryChargeRate", 1f);

            /* === HEALTH === */
            // Medical
            Scribe_Values.Look(ref medicinesAreInterchangeable, "MHC_medicinesAreInterchangeable", false);
            Scribe_Values.Look(ref showMechanicalSurgerySuccessChance, "MHC_showMechanicalSurgerySuccessChance", false);
            Scribe_Values.Look(ref maxChanceMechanicOperationSuccess, "MHC_maxChanceMechanicOperationSuccess", 1f);
            Scribe_Values.Look(ref chanceFailedOperationMinor, "MHC_chanceFailedOperationMinor", 0.75f);
            Scribe_Values.Look(ref chancePartSavedOnFailure, "MHC_chancePartSavedOnFailure", 0.75f);

            // Maintenance
            Scribe_Values.Look(ref maintenanceNeedExists, "MHC_maintenanceNeedExists", true);
            Scribe_Values.Look(ref maintenanceFallRateFactor, "MHC_maintenanceFallRateFactor", 1.0f);
            Scribe_Values.Look(ref maintenanceGainRateFactor, "MHC_maintenanceGainRateFactor", 1.0f);
        }
    }

}