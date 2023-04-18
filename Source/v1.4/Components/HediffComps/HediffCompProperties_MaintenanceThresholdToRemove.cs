using Verse;

namespace MechHumanlikes
{
    public class HediffCompProperties_MaintenanceThresholdToRemove : HediffCompProperties
    {
        public HediffCompProperties_MaintenanceThresholdToRemove()
        {
            compClass = typeof(HediffComp_MaintenanceThresholdToRemove);
        }

        public bool shouldBeHigherThanToRemove = true;

        public float maintenanceThresholdDays;
    }
}
