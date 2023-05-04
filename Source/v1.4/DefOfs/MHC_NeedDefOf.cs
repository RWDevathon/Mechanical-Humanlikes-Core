using RimWorld;

namespace MechHumanlikes
{
    [DefOf]
    public static class MHC_NeedDefOf
    {
        static MHC_NeedDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MHC_NeedDefOf));
        }
        public static NeedDef MHC_Coolant;

        public static NeedDef MHC_Lubrication;
    }
}