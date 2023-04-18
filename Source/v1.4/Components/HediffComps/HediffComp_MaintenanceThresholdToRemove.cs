using Verse;

namespace MechHumanlikes
{
    public class HediffComp_MaintenanceThresholdToRemove : HediffComp
    {
        public HediffCompProperties_MaintenanceThresholdToRemove Props => (HediffCompProperties_MaintenanceThresholdToRemove)props;

        public override bool CompShouldRemove => Props.shouldBeHigherThanToRemove ? CompMaintenanceNeed.maintenanceEffectTicks > Props.maintenanceThresholdDays * 60000 : CompMaintenanceNeed.maintenanceEffectTicks < Props.maintenanceThresholdDays * 60000;

        public CompMaintenanceNeed CompMaintenanceNeed
        {
            get
            {
                if (compMaintenanceNeed == null)
                {
                    compMaintenanceNeed = Pawn.GetComp<CompMaintenanceNeed>();
                }
                return compMaintenanceNeed;
            }
        }

        private CompMaintenanceNeed compMaintenanceNeed;
    }
}
