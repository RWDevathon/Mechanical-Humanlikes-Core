using RimWorld;
using Verse;

namespace MechHumanlikes
{
    // Simple StatWorker that will only display stats that use this worker if the pawn needs maintenance.
    public class StatWorker_Maintenance : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!base.ShouldShowFor(req))
            {
                return false;
            }
            if (req.Thing != null && req.Thing is Pawn pawn)
            {
                return pawn.def.GetModExtension<MHC_MechanicalPawnExtension>()?.needsMaintenance == true && MechHumanlikes_Settings.maintenanceNeedExists;
            }
            return false;
        }
    }
}
