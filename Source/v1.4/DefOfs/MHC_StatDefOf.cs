using RimWorld;

namespace MechHumanlikes
{
    [DefOf]
    public static class MHC_StatDefOf
    {
        static MHC_StatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MHC_StatDefOf));
        }

        public static StatDef MHC_MechanicalSurgerySuccessChance;

        public static StatDef MHC_MechanicalTendQuality;

        public static StatDef MHC_MechanicalTendSpeed;

        public static StatDef MHC_MechanicalTendQualityOffset;

        public static StatDef MHC_MechanicalSurgerySuccessChanceFactor;

        public static StatDef MHC_MaintenanceRetention;

        public static StatDef MHC_NutritionalIntakeEfficiency;

        public static StatDef MHC_ChargingSpeed;
    }
}