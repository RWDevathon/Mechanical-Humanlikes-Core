using Verse;
using RimWorld;

namespace MechHumanlikes
{
    [DefOf]
    public static class MHC_WorkTypeDefOf
    {
        static MHC_WorkTypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MHC_WorkTypeDefOf));
        }

        public static WorkTypeDef MHC_Mechanic;
    }
}