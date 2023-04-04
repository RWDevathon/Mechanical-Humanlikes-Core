using System.Collections.Generic;
using Verse;

namespace MechHumanlikes
{
    // Mod extension for HediffDefs to mark them as maintenance consequences with appropriate details to be picked up by CompMaintenanceNeed.
    public class MHC_MaintenanceEffectExtension : DefModExtension
    {
        // Maintenance stage-related Hediffs are simple hediffs that should be applied at the corresponding stage and removed when it is not that stage (uses requiredMHC_MaintenanceStageToOccur).
        public bool isMaintenangeStageEffect = false;

        // Maintenance workers have special C# code for adding conditions on when it is legal to add the effect to pawns, and to add additional actions upon application.
        public MaintenanceWorker maintenanceWorker;

        // Curve to set the mean time to happen (in days) before this effect occurs when it is valid.
        public SimpleCurve MeanDaysToOccur = new SimpleCurve
        {
            new CurvePoint(0f, 9999999f),
            new CurvePoint(3f, 48f),
            new CurvePoint(9f, 24f),
            new CurvePoint(21f, 12f)
        };

        // Offset before the effect can occur. Negative means it is a negative maintenance effect, with at least 3 days of poor maintenance, and vice versa for positive.
        public int daysBeforeCanOccur = -3;

        // Required maintenance stage in order to occur on a pawn. IE. a pawn with very poor average maintenance can not trigger an effect if they currently have high maintenance.
        public MHC_MaintenanceStage requiredMHC_MaintenanceStageToOccur = MHC_MaintenanceStage.Poor;

        // A whitelist of races for applying the Hediff to. If left null, it will rely on above tags and apply to all races.
        public List<ThingDef> racesToAffect;

        // The specific body parts or body part tags (if any) to apply the Hediff to. If both are left null, it will assume it should be applied to the whole body.
        public List<BodyPartDef> partsToAffect;
        public List<BodyPartTagDef> bodyPartTagsToAffect;

        // The depth that is legal for applying the Hediff to. If left null, it will rely on above tags. If both above and this are specified, parts that satisfy any constraint are chosen (union).
        public BodyPartDepth partDepthToAffect;

        public override IEnumerable<string> ConfigErrors()
        {
            return base.ConfigErrors();
        }
    }
}