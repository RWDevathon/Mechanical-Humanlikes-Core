using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class ThoughtWorker_Precept_HasProsthetic_Patch
    {
        // Mechanical units always act as if they have a prosthetic.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_HasProsthetic), "HasProsthetic")]
        public class TW_Precept_HasProsthetic_HasProsthetic
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref bool __result)
            {
                if (__result)
                    return;

                if (MHC_Utils.IsConsideredMechanicalSapient(p))
                {
                    __result = true;
                }
            }
        }
    }
}