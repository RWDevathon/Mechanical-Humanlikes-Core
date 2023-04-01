using Verse;
using RimWorld;

namespace MechHumanlikes
{
    [DefOf]
    public static class MHC_HediffDefOf
    {
        static MHC_HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MHC_HediffDefOf));
        }

        // Surgery effect

        public static HediffDef MHC_Restarting;

    }
}
