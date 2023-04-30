using System;
using System.Collections.Generic;
using Verse;

namespace MechHumanlikes
{
    // Mod extension for HediffDefs to mark them as maintenance consequences with appropriate details to be picked up by CompMaintenanceNeed.
    public class MHC_MaintenanceEffectExtension : DefModExtension
    {
        // Maintenance stage-related Hediffs are simple hediffs that should be applied at the corresponding stage and removed when it is not that stage (uses requiredMaintenanceStageToOccur).
        public bool isMaintenanceStageEffect = false;

        // Maintenance workers have special C# code for adding conditions on when it is legal to add the effect to pawns, and to add additional actions upon application.
        // A HediffDef may have multiple maintenance workers. Conditionals are always AND, meaning all workers must permit the effect for it to be legal.
        public List<Type> maintenanceWorkers;

        // Curve to set the mean time to happen (in days) before this effect occurs when it is valid.
        public SimpleCurve meanDaysToOccur = new SimpleCurve
        {
            new CurvePoint(0f, 9999999f),
            new CurvePoint(3f, 48f),
            new CurvePoint(9f, 24f),
            new CurvePoint(21f, 12f)
        };

        // Offset before the effect can occur. Negative means it is a negative maintenance effect, with at least 3 days of poor maintenance, and vice versa for positive.
        public int daysBeforeCanOccur = -3;

        // Required maintenance stage in order to occur on a pawn. IE. a pawn with very poor average maintenance can not trigger an effect if they currently have high maintenance.
        public MHC_MaintenanceStage requiredMaintenanceStageToOccur = MHC_MaintenanceStage.Poor;

        // A whitelist of races for applying the Hediff to. If left null, it will apply to all races if they match any of the below tags. If all are null, it will apply to all races.
        public List<ThingDef> racesToAffect;

        // The specific body parts or body part tags (if any) to apply the Hediff to. If both are left null, it will assume it should be applied to the whole body.
        public List<BodyPartDef> partsToAffect;
        public List<BodyPartTagDef> bodyPartTagsToAffect;

        // The depth that is legal for applying the Hediff to. If left null, it will rely on above tags. If both above and this are specified, parts that satisfy any constraint are chosen (union).
        public BodyPartDepth partDepthToAffect;

        // XML parsing of workers is done as Types, and must be converted to MaintenanceWorkers prior to use. This saves the casted class for reuse.
        private List<MaintenanceWorker> maintenanceWorkerInts;

        public List<MaintenanceWorker> MaintenanceWorkers
        {
            get
            {
                if (maintenanceWorkerInts == null)
                {
                    maintenanceWorkerInts = new List<MaintenanceWorker>();
                    if (maintenanceWorkers != null)
                    {
                        foreach (Type worker in maintenanceWorkers)
                        {
                            MaintenanceWorker workerInt = (MaintenanceWorker)Activator.CreateInstance(worker);
                            workerInt.effecter = this;
                            maintenanceWorkerInts.Add(workerInt);
                        }
                    }
                }
                return maintenanceWorkerInts;
            }
        }
    }
}