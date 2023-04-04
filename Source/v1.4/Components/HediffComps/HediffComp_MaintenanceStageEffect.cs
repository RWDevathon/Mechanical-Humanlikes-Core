using Verse;

namespace MechHumanlikes
{
    public class HediffComp_MHC_MaintenanceStageEffect : HediffComp
    {
        public HediffCompProperties_MHC_MaintenanceStageEffect Props => (HediffCompProperties_MHC_MaintenanceStageEffect)props;

        public override bool CompShouldRemove => Pawn.GetComp<CompMaintenanceNeed>()?.Stage != Props.activeMHC_MaintenanceStage;
    }
}
