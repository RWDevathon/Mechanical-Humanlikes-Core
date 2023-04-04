using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    // Pawns that are considered non-humanlike intelligence are not valid targets for attempted Ideological conversions.
    public class CompAbilityEffect_Convert_Patch
    {
        [HarmonyPatch(typeof(CompAbilityEffect_Convert), "Valid")]
        public class CompAbilityEffect_Convert_Valid_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(LocalTargetInfo target, ref bool __result)
            {
                if (target != null && target.Pawn != null && MHC_Utils.IsConsideredMechanicalDrone(target.Pawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}