using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    // Pawns that are not considered humanlike don't get unburied colonist notifications.
    public class Alert_ColonistLeftUnburied_Patch
    {
        [HarmonyPatch(typeof(Alert_ColonistLeftUnburied), "IsCorpseOfColonist")]
        public class Alert_ColonistLeftUnburied_IsCorpseOfColonist_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Corpse corpse, ref bool __result)
            {
                if (!__result)
                    return;

                Pawn p = corpse.InnerPawn;
                if (p != null && MHC_Utils.IsConsideredNonHumanlike(p))
                    __result = false;
 
            }
        }
    }
}