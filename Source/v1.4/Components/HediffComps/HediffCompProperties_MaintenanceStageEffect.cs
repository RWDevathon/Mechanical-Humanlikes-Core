using Verse;

namespace MechHumanlikes
{
    public class HediffCompProperties_MHC_MaintenanceStageEffect : HediffCompProperties
    {
        public HediffCompProperties_MHC_MaintenanceStageEffect()
        {
            compClass = typeof(HediffComp_MHC_MaintenanceStageEffect);
        }

        public MHC_MaintenanceStage activeMHC_MaintenanceStage;
    }
}
