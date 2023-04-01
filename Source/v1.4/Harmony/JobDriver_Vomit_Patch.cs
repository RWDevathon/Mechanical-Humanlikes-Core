using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    internal class JobDriver_Vomit_Patch
    {
        // Mechanical pawns do not vomit.
        [HarmonyPatch(typeof(JobDriver_Vomit), "TryMakePreToilReservations")]
        public class TryMakePreToilReservations_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref Pawn ___pawn, ref bool __result)
            {
                __result = __result && !Utils.IsConsideredMechanical(___pawn);
            }
        }
    }
}