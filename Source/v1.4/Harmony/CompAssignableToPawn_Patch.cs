using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class CompAssignableToPawn_Patch
    {
        // Nonsapient units have no ideoligion, and are therefore never restricted by it.
        [HarmonyPatch(typeof(CompAssignableToPawn), "IdeoligionForbids")]
        public class IdeoligionForbids_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Pawn pawn)
            {
                if (MHC_Utils.IsConsideredNonHumanlike(pawn))
                {
                    __result = false;
                }
            }
        }
    }
}