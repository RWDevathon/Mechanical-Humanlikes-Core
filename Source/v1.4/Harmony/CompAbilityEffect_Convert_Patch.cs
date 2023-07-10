using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class CompAbilityEffect_Convert_Patch
    {
        // Pawns that are considered non-humanlike intelligence are not valid targets for attempted Ideological conversions.
        [HarmonyPatch(typeof(CompAbilityEffect_Convert), "Valid")]
        public class Valid_Patch
        {
            [HarmonyPostfix]
            public static void Listener(LocalTargetInfo target, ref bool __result)
            {
                __result = __result && !MHC_Utils.IsConsideredMechanicalDrone(target.Pawn);
            }
        }
    }
}