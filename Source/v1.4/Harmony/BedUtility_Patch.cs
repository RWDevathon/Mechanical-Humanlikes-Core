using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class BedUtility_Patch
    {
        // Drones can share beds with anyone, even if it unsettles their bedmate.
        [HarmonyPatch(typeof(BedUtility), "WillingToShareBed")]
        public class WillingToShareBed_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn pawn1, Pawn pawn2)
            {
                __result = MHC_Utils.IsConsideredNonHumanlike(pawn1) || MHC_Utils.IsConsideredNonHumanlike(pawn2);
                return !__result;
            }
        }
    }
}