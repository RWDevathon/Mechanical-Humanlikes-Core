using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    internal class PrisonBreakUtility_Patch
    {
        // Non-humanlike intelligences can not participate in prison breaks.
        [HarmonyPatch(typeof(PrisonBreakUtility), "CanParticipateInPrisonBreak")]
        public class CanParticipateInPrisonBreak_Patch
        {
            [HarmonyPostfix]
            public static void Listener( ref bool __result, Pawn pawn)
            {
                __result = __result && !Utils.IsConsideredNonHumanlike(pawn);
            }
        }
    }
}