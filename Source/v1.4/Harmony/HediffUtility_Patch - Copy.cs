using Verse;
using HarmonyLib;

namespace MechHumanlikes
{
    public class HediffUtility_Patch
    {
        // Mechanical units are effectively all implants (bionics for organics usually), so don't even bother calculating for them.
        [HarmonyPatch(typeof(HediffUtility), "CountAddedAndImplantedParts")]
        public class CountAddedParts_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(HediffSet hs, ref int __result)
            {
                if (MHC_Utils.IsConsideredMechanical(hs.pawn))
                {
                    __result = 50;
                    return false;
                }
                return true;
            }
        }
    }
}