using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    internal class Pawn_NeedsTracker_Patch
    {
        // Ensure mechanical units only have applicable needs as determined by their extension.
        [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
        public class ShouldHaveNeed_Patch
        {
            [HarmonyPostfix]
            public static void Listener(NeedDef nd, ref bool __result, Pawn ___pawn)
            {
                // Patch only applies to mechanical units.
                if (!__result || ___pawn == null || !Utils.IsConsideredMechanical(___pawn))
                    return;

                // Get the pawn's mechanical extension and check specific values. Mechanical sapients check only the extension, while drones have an extra set of blacklisted needs.
                MHC_MechanicalPawnExtension pawnExtension = ___pawn.def.GetModExtension<MHC_MechanicalPawnExtension>();
                if (pawnExtension == null)
                {
                    return;
                }

                // Race-wide blacklisted needs.
                if (pawnExtension.blacklistedNeeds.Contains(nd))
                {
                    __result = false;
                }

                // Sapient blacklisted needs.
                if (Utils.IsConsideredMechanicalSapient(___pawn) && pawnExtension.blacklistedSapientNeeds.Contains(nd))
                {
                    __result = false;
                }

                // Drone blacklisted needs.
                if (Utils.IsConsideredMechanicalDrone(___pawn) && Utils.ReservedBlacklistedDroneNeeds.Contains(nd.defName))
                {
                    __result = false;
                }
            }
        }
    }
}