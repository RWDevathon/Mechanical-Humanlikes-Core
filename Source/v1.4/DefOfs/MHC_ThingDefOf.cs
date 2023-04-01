using Verse;
using RimWorld;

namespace MechHumanlikes
{
    [DefOf]
    public static class MHC_ThingDefOf
    {
        static MHC_ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MHC_ThingDefOf));
        }

        public static ThingDef MHC_BedsideChargerFacility;

        public static ThingDef MHC_MaintenanceSpot;
    }
}