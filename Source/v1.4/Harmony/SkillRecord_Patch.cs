using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    // Non-humanlike intelligences don't forget skills.
    [HarmonyPatch(typeof(SkillRecord), "Interval")]
    public static class Interval_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Pawn ___pawn)
        {
            if (MHC_Utils.IsConsideredNonHumanlike(___pawn))
            {
                return false;
            }
            return true;
        }
    }
}