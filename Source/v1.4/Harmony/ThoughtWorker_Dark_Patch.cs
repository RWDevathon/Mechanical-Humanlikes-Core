using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class ThoughtWorker_Dark_Patch
    {
        // Mechanical units aren't bothered by darkness.
        [HarmonyPatch(typeof(ThoughtWorker_Dark), "CurrentStateInternal")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (MHC_Utils.IsConsideredMechanicalSapient(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}