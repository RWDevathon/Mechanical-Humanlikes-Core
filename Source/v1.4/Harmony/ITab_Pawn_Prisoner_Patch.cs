using RimWorld;
using HarmonyLib;
using Verse;

namespace MechHumanlikes
{
    public static class ITab_Pawn_Prisoner_Patch
    {
        // Drones are incapable of receiving social interactions, and are thus invalid for all prisoner interactions. Prevent players from accessing the tab.
        [HarmonyPatch(typeof(ITab_Pawn_Prisoner), "get_IsVisible")]
        public class ITab_Pawn_Prisoner_get_IsVisible_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ITab_Pawn_Prisoner __instance, ref bool __result)
            {
                // Get the currently selected pawn. If it's null for some reason, let the default behavior continue.
                if (!(Find.Selector.SingleSelectedThing is Pawn pawn))
                    return true;

                // Drones do not get the prison tab.
                if (MHC_Utils.IsConsideredMechanicalDrone(pawn))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}