using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class WorkGiver_Tend_Patch
    {
        // Patch the medical tend WorkGiver to not give doctoring jobs on mechanicals. WorkGiver_MechTend handles mechanical tending.
        [HarmonyPatch(typeof(WorkGiver_Tend), "HasJobOnThing")]
        public class PotentialWorkThingsGlobal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, Thing t, bool forced, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                __result = !MHC_Utils.IsConsideredMechanical(t.def);
            }
        }
    }
}