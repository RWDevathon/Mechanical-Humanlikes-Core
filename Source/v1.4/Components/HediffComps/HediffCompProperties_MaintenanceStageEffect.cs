using Verse;

namespace MechHumanlikes
{
    public class HediffCompProperties_MaintenanceStageEffect : HediffCompProperties
    {
        public HediffCompProperties_MaintenanceStageEffect()
        {
            compClass = typeof(HediffComp_MaintenanceStageEffect);
        }

        public MaintenanceStage activeMaintenanceStage;
    }
}
