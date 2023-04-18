using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MechHumanlikes
{
    [StaticConstructorOnStartup]
    public class CompMaintenanceNeed : ThingComp
    {
        Pawn Pawn => (Pawn)parent;

        private static float ThresholdCritical => 0.1f;

        private static float ThresholdPoor => 0.3f;

        private static float ThresholdSatisfactory => 0.7f;

        public static readonly List<float> MaintenanceThresholdBandPercentages = new List<float> {0f, ThresholdCritical, ThresholdPoor, ThresholdSatisfactory, 1f};

        private static readonly float TicksPerDay = 60000;

        private static readonly float TicksPerLong = 2000;

        public MHC_MaintenanceStage Stage
        {
            get
            {
                if (maintenanceLevel < ThresholdCritical)
                    return MHC_MaintenanceStage.Critical;
                else if (maintenanceLevel < ThresholdPoor)
                    return MHC_MaintenanceStage.Poor;
                else if (maintenanceLevel < ThresholdSatisfactory)
                    return MHC_MaintenanceStage.Sufficient;
                return MHC_MaintenanceStage.Satisfactory;
            }
        }

        public float MaintenanceLevel
        {
            get
            {
                return maintenanceLevel;
            }
        }

        public float TargetMaintenanceLevel
        {
            get
            {
                return targetLevel;
            }
            set
            {
                targetLevel = Mathf.Clamp(value, 0f, 1f);
            }
        }

        private float DailyFallPerStage(MHC_MaintenanceStage stage)
        {
            switch (stage)
            {
                case MHC_MaintenanceStage.Critical:
                    return 0.05f; // 5% per day base (10 -> 0 should take 2 day with 1 efficiency)
                case MHC_MaintenanceStage.Poor:
                    return 0.10f; // 10% per day base (30 -> 10 should take 2 days with 1 efficiency)
                case MHC_MaintenanceStage.Sufficient:
                    return 0.20f; // 20% per day base (70 -> 30 should take 2 days with 1 efficiency)
                default:
                    return 0.60f; // 60% per day base (100 -> 70 should take 0.5 day with 1 efficiency)
            }
        }

        public float MaintenanceFallPerDay()
        {
            return Mathf.Clamp(DailyFallPerStage(Stage) * MechHumanlikes_Settings.maintenanceFallRateFactor / Pawn.GetStatValue(MHC_StatDefOf.MHC_MaintenanceRetention, cacheStaleAfterTicks: 2000), 0.005f, 2f);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (maintenanceLevel < 0)
            {
                maintenanceLevel = 0.6f;
            }
            if (targetLevel < 0)
            {
                targetLevel = 0.5f;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref maintenanceLevel, "MHC_maintenanceLevel", -1);
            Scribe_Values.Look(ref targetLevel, "MHC_targetLevel", -1);
            Scribe_Values.Look(ref cachedFallRatePerDay, "MHC_cachedFallRatePerDay", -1);
            Scribe_Values.Look(ref maintenanceEffectTicks, "MHC_maintenanceEffectTicks", TicksPerDay);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!Pawn.Spawned || !MechHumanlikes_Settings.maintenanceNeedExists || Find.TickManager.TicksGame % 2000 != 0)
            {
                return;
            }

            // Recache fall rate every in game day.
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                cachedFallRatePerDay = MaintenanceFallPerDay();
            }

            TryMaintenanceCheck();

            ChangeMaintenanceEffectTicks();
            ChangeMaintenanceLevel(-cachedFallRatePerDay * TicksPerLong / TicksPerDay);
        }

        // Alter the maintenance level by the provided amount (decreases are assumed to be negative). Ensure the level never falls outside 0 - 1 range and handle stage changes appropriately.
        public void ChangeMaintenanceLevel(float baseChange)
        {
            MHC_MaintenanceStage currentStage = Stage;
            maintenanceLevel = Mathf.Clamp(maintenanceLevel + baseChange, 0f, 1f);

            // If we changed stages, make sure we initialize an appropriate stage effect hediff if there is one. They remove themselves automatically when appropriate.
            if (Stage != currentStage)
            {
                // Check all maintenance stage effects for this race and apply them if the new stage matches their required stage.
                foreach (HediffDef hediffDef in MHC_Utils.GetMaintenanceEffectsForRace(Pawn.RaceProps))
                {
                    MHC_MaintenanceEffectExtension effectExtension = hediffDef.GetModExtension<MHC_MaintenanceEffectExtension>();
                    if (effectExtension.isMaintenangeStageEffect && effectExtension.requiredMaintenanceStageToOccur == Stage)
                    {
                        Pawn.health.AddHediff(hediffDef);
                    }
                }
            }
        }

        /// <summary>
        /// Alter the maintenance effect ticks based on the provided tick rate. Actual effect is changed based on the pawn's maintenance level.
        /// If less than 0.3 (poor maintenance), effect trends negatively, and if higher than 0.7, effect trends positively.
        /// Between 0.3 and 0.7, effect trends toward 0 days, and does not change if between -60000 and 60000 already.
        /// </summary>
        public void ChangeMaintenanceEffectTicks()
        {
            if (maintenanceLevel < 0.3f)
            {
                maintenanceEffectTicks -= TicksPerLong;

                if (maintenanceEffectTicks > 0)
                {
                    maintenanceEffectTicks -= maintenanceEffectTicks * 0.001f;
                }
            }
            else if (maintenanceLevel > 0.7f)
            {
                maintenanceEffectTicks += TicksPerLong;

                if (maintenanceEffectTicks < 0)
                {
                    maintenanceEffectTicks -= maintenanceEffectTicks * 0.001f;
                }
            }
            else
            {
                maintenanceEffectTicks = Mathf.MoveTowards(maintenanceEffectTicks, 0, (Mathf.Log((Mathf.Abs(maintenanceEffectTicks) / TicksPerDay) + 2, 2) - 1) * TicksPerLong);
            }
            // Prevent the ticks from going outside a 60 day value positively or negatively.
            maintenanceEffectTicks = Mathf.Clamp(maintenanceEffectTicks, -3600000, 3600000);
        }

        // Maintenance need has associated gizmos for displaying and controlling the maintenance level of pawns.
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!MechHumanlikes_Settings.maintenanceNeedExists)
            {
                yield break;
            }

            if (Find.Selector.SingleSelectedThing == parent)
            {
                Gizmo_MaintenanceStatus maintenanceStatusGizmo = new Gizmo_MaintenanceStatus
                {
                    maintenanceNeed = this
                };
                yield return maintenanceStatusGizmo;

                Gizmo_MaintenanceEffect maintenanceEffectGizmo = new Gizmo_MaintenanceEffect
                {
                    maintenanceNeed = this
                };
                yield return maintenanceEffectGizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action subtract20PercentMaintenance = new Command_Action
                {
                    defaultLabel = "DEV: Maintenance -20%",
                    action = delegate
                    {
                        ChangeMaintenanceLevel(-0.2f);
                    }
                };
                yield return subtract20PercentMaintenance;
                Command_Action add20PercentMaintenance = new Command_Action
                {
                    defaultLabel = "DEV: Maintenance +20%",
                    action = delegate
                    {
                        ChangeMaintenanceLevel(0.2f);
                    }
                };
                yield return add20PercentMaintenance;
                Command_Action substract1DayMaintenanceEffect = new Command_Action
                {
                    defaultLabel = "DEV: Maintenance Effect -1 day",
                    action = delegate
                    {
                        maintenanceEffectTicks -= TicksPerDay;
                    }
                };
                yield return substract1DayMaintenanceEffect;
                Command_Action add1DayMaintenanceEffect = new Command_Action
                {
                    defaultLabel = "DEV: Maintenance Effect +1 day",
                    action = delegate
                    {
                        maintenanceEffectTicks += TicksPerDay;
                    }
                };
                yield return add1DayMaintenanceEffect;
            }
            yield break;
        }

        public string MaintenanceTipString()
        {
            if (maintenanceLevelInfoCached == null)
            {
                for (int stageInt = 0; stageInt < MaintenanceThresholdBandPercentages.Count - 1; stageInt++)
                {
                    maintenanceLevelInfoCached += "MHC_MaintenanceLevelInfoRange".Translate((MaintenanceThresholdBandPercentages[stageInt] * 100f).ToStringDecimalIfSmall(), (MaintenanceThresholdBandPercentages[stageInt + 1] * 100f).ToStringDecimalIfSmall()) + ": " + "MHC_MaintenanceLevelInfoFallRate".Translate(DailyFallPerStage((MHC_MaintenanceStage)stageInt).ToStringPercent()) + "\n";
                }
            }
            return (("MHC_MaintenanceGizmoLabel".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + MaintenanceLevel.ToStringPercent("0.#") + "\n" + "MHC_MaintenanceTargetLabel".Translate() + ": " + TargetMaintenanceLevel.ToStringPercent("0.#") + "\n\n" + "MHC_MaintenanceTargetLabelDesc".Translate() + "\n\n" + "MHC_MaintenanceDesc".Translate() + ":\n\n" + maintenanceLevelInfoCached).Resolve();
        }

        // Randomly applies maintenance effects based on random chances from the ticksSincePoorMaintenance level.
        public void TryMaintenanceCheck()
        {
            // Check all valid maintenance effects for this race and test whether they are applied now.
            foreach (HediffDef hediffDef in MHC_Utils.GetMaintenanceEffectsForRace(Pawn.RaceProps))
            {
                MHC_MaintenanceEffectExtension effectExtension = hediffDef.GetModExtension<MHC_MaintenanceEffectExtension>();
                // Maintenance stage effects are applied/removed automatically on stage changes.
                if (effectExtension.isMaintenangeStageEffect)
                {
                    continue;
                }

                // If there is a custom maintenance worker that blocks this effect on the pawn, it can not occur.
                if (!effectExtension.MaintenanceWorkers.NullOrEmpty() && effectExtension.MaintenanceWorkers.Any(maintenanceWorker => !maintenanceWorker.CanApplyTo(Pawn)))
                {
                    continue;
                }

                // Effects requiring negative maintenance
                if (effectExtension.daysBeforeCanOccur < 0)
                {
                    // Stage is too high to occur now.
                    if (Stage > effectExtension.requiredMaintenanceStageToOccur)
                    {
                        continue;
                    }

                    // If maintenance effect ticks is higher than days before can occur, it can not occur (both are negative here).
                    if (maintenanceEffectTicks > effectExtension.daysBeforeCanOccur * 60000)
                    {
                        continue;
                    }

                    TryMaintenanceEffect(effectExtension, maintenanceEffectTicks, Pawn, hediffDef);
                }
                // Effects requiring positive maintenance
                else
                {
                    // Stage is too low to occur now.
                    if (Stage < effectExtension.requiredMaintenanceStageToOccur)
                    {
                        continue;
                    }

                    // If maintenance effect ticks is lower than days before can occur, it can not occur (both are positive here).
                    if (maintenanceEffectTicks < effectExtension.daysBeforeCanOccur * 60000)
                    {
                        continue;
                    }

                    TryMaintenanceEffect(effectExtension, maintenanceEffectTicks, Pawn, hediffDef);
                }
            }
        }

        public static void TryMaintenanceEffect(MHC_MaintenanceEffectExtension effectExtension, float effectTicks, Pawn pawn, HediffDef hediffDef)
        {
            // Check the chance to occur based on the extensions curve, with 0 corresponding to the days before it can occur. If it should, apply the appropriate effects.
            // For example, if 4 average maintenance days must pass before an effect is applied, and 5 days have passed, the curve will evaluate at 1.
            if (Rand.MTBEventOccurs(effectExtension.meanDaysToOccur.Evaluate(Math.Abs(effectTicks) - Math.Abs(effectExtension.daysBeforeCanOccur)), TicksPerDay, 60f))
            {
                HashSet<BodyPartRecord> validParts = ValidBodyPartsForEffect(effectExtension, pawn);
                // If there are no legal parts identified, this effect can not occur.
                if (validParts == null)
                {
                    return;
                }

                // If the HashSet is empty, that means the effect should be applied to the whole body.
                if (validParts.Count == 0)
                {
                    pawn.health.AddHediff(hediffDef);
                    Hediff chosenHediff = HediffMaker.MakeHediff(hediffDef, pawn);
                    if (effectExtension.MaintenanceWorkers.NullOrEmpty())
                    {
                        foreach (MaintenanceWorker worker in effectExtension.MaintenanceWorkers)
                        {
                            worker.OnApplied(pawn, null);
                        }
                    }
                    SendMaintenanceEffectLetter(pawn, chosenHediff);
                }
                else
                {
                    BodyPartRecord chosenPart = validParts.RandomElement();
                    Hediff chosenHediff = HediffMaker.MakeHediff(hediffDef, pawn, chosenPart);
                    pawn.health.AddHediff(chosenHediff, chosenPart);
                    if (!effectExtension.MaintenanceWorkers.NullOrEmpty())
                    {
                        foreach (MaintenanceWorker worker in effectExtension.MaintenanceWorkers)
                        {
                            worker.OnApplied(pawn, chosenPart);
                        }
                    }
                    SendMaintenanceEffectLetter(pawn, chosenHediff);
                }
            }
        }

        public static HashSet<BodyPartRecord> ValidBodyPartsForEffect(MHC_MaintenanceEffectExtension effectExtension, Pawn pawn)
        {
            // If parts to affect, tags to affect, and depth are all unspecified and thus no parts have been identified yet, the effect may be applied to the whole body, signified by an empty hash set.
            if (effectExtension.partsToAffect == null && effectExtension.bodyPartTagsToAffect == null && effectExtension.partDepthToAffect == BodyPartDepth.Undefined)
            {
                return new HashSet<BodyPartRecord>();
            }

            HashSet<BodyPartRecord> validParts = new HashSet<BodyPartRecord>();
            List<BodyPartRecord> pawnParts = pawn.RaceProps.body.AllParts;
            foreach (BodyPartRecord part in pawnParts)
            {
                // Missing parts are skipped entirely and are always illegal.
                if (pawn.health.hediffSet.PartIsMissing(part))
                {
                    continue;
                }

                // If the maintenance worker indicates this part is illegal now, skip it.
                if (!effectExtension.MaintenanceWorkers.NullOrEmpty() && effectExtension.MaintenanceWorkers.Any(maintenanceWorker => maintenanceWorker.CanApplyOnPart(pawn, part) == false))
                {
                    continue;
                }

                // If this specific part is legal, add it.
                if (effectExtension.partsToAffect?.Contains(part.def) == true)
                {
                    validParts.Add(part);
                    continue;
                }

                // If this part has the correct tag, add it.
                if (effectExtension.bodyPartTagsToAffect != null && part.def.tags != null && effectExtension.bodyPartTagsToAffect.Any(tag => part.def.tags.Contains(tag)))
                {
                    validParts.Add(part);
                    continue;
                }

                // If this part has the correct depth, add it.
                if (effectExtension.partDepthToAffect == part.depth)
                {
                    validParts.Add(part);
                }
            }
            if (validParts.Count > 0)
            {
                return validParts;
            }
            // If no parts were identified and the effect can't be applied to the whole body, return null to signify there are no valid ways to use this effect.
            return null;
        }

        // Send a letter to the player about an effect being applied to the pawn if appropriate.
        public static void SendMaintenanceEffectLetter(Pawn pawn, Hediff cause)
        {
            if (PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Find.LetterStack.ReceiveLetter("MHC_MaintenanceEffectOccurredLetterLabel".Translate(pawn.LabelShort, cause.LabelCap, pawn.Named("PAWN")).CapitalizeFirst(), "MHC_MaintenanceEffectOccurredLetter".Translate(pawn.LabelShortCap, cause.LabelCap, pawn.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn);
            }
        }

        private float maintenanceLevel = -1;
        public float targetLevel = -1;
        private float cachedFallRatePerDay = 0.1f;
        public float maintenanceEffectTicks = TicksPerDay;
        public static string maintenanceLevelInfoCached;
    }
}