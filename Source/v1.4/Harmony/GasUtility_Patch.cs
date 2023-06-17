using Verse;
using HarmonyLib;

namespace MechHumanlikes
{
    public class GasUtility_Patch
    {
        // Mechanical units are not affected by gas exposure.
        [HarmonyPatch(typeof(GasUtility), "IsEffectedByExposure")]
        public class IsEffectedByExposure_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn pawn)
            {
                __result = !MHC_Utils.IsConsideredMechanical(pawn);
                return __result;
            }
        }
    }
}