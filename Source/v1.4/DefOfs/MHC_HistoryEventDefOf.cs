using RimWorld;

namespace MechHumanlikes
{
    [DefOf]
    public static class MHC_HistoryEventDefOf
    {
        static MHC_HistoryEventDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MHC_HistoryEventDefOf));
        }
        public static HistoryEventDef MHC_ExtractedCoolantPack;
    }
}