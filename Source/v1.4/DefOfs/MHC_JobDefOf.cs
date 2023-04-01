using Verse;
using RimWorld;

namespace MechHumanlikes
{
    [DefOf]
    public static class MHC_JobDefOf
    {
        static MHC_JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MHC_JobDefOf));
        }
        public static JobDef MHC_GetRecharge;

        public static JobDef MHC_TendMechanical;

        public static JobDef MHC_DoMaintenanceUrgent;

        public static JobDef MHC_DoMaintenanceIdle;
    }
}