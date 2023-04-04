using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    // There is no reason to try and feed a pawn that is charging.
    public class FeedPatientUtility_Patch
    {
        [HarmonyPatch(typeof(FeedPatientUtility), "ShouldBeFed")]
        public class ShouldBeFed_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref bool __result)
            {
                if (__result && MHC_Utils.CanUseBattery(p) && p.CurJob.def == MHC_JobDefOf.MHC_GetRecharge)
                    __result = false;
            }
        }
    }
}