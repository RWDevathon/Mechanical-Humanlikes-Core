using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class JobGiver_GetHemogen_Patch
    {
        // Mechanical units are invalid targets for blood feeding.
        [HarmonyPatch(typeof(JobGiver_GetHemogen), "CanFeedOnPrisoner")]
        public class CanFeedOnPrisoner_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref AcceptanceReport __result, Pawn prisoner)
            {
                if (MHC_Utils.IsConsideredMechanical(prisoner))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}