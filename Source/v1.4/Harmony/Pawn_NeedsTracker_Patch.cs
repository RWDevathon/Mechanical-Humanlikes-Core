using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class Pawn_NeedsTracker_Patch
    {
        // Ensure mechanical units only have applicable needs as determined by their extension.
        [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
        public class ShouldHaveNeed_Patch
        {
            [HarmonyPostfix]
            public static void Listener(NeedDef nd, ref bool __result, Pawn ___pawn)
            {
                // If the result is already false or the pawn is null for some reason, do nothing.
                if (!__result || ___pawn == null)
                    return;

                // Get the pawn's mechanical extension and the need's mechanical extension (if they exist).
                // Mechanical pawns have access to a blacklist, while need extensions indicate only mechanical pawns may have it.
                MHC_MechanicalPawnExtension pawnExtension = ___pawn.def.GetModExtension<MHC_MechanicalPawnExtension>();
                MHC_MechanicalNeedExtension needExtension = nd.GetModExtension<MHC_MechanicalNeedExtension>();

                // If there is no pawn extension, then this isn't a mechanical pawn.
                if (pawnExtension == null)
                {
                    // If there is a need extension, then it is only for mechanical pawns. This pawn should not have it.
                    if (needExtension != null)
                    {
                        __result = false;
                        return;
                    }
                    
                    // Without an extension, there is nothing else to check. The pawn may have this need.
                    return;
                }

                // Race-wide blacklisted needs.
                if (pawnExtension.blacklistedNeeds?.Contains(nd) ?? false)
                {
                    __result = false;
                    return;
                }

                // Sapient blacklisted needs.
                if (MHC_Utils.IsConsideredMechanicalSapient(___pawn) && (needExtension?.droneOnly ?? false || (pawnExtension.blacklistedSapientNeeds?.Contains(nd) ?? false)))
                {
                    __result = false;
                    return;
                }

                // Drone blacklisted needs.
                if (MHC_Utils.IsConsideredMechanicalDrone(___pawn) && (needExtension?.sapientOnly ?? false || MHC_Utils.ReservedBlacklistedDroneNeeds.Contains(nd.defName)))
                {
                    __result = false;
                    return;
                }

                // Mechanical needs (marked by the defModExtension) are whitelist only.
                if (needExtension != null && (pawnExtension.mechanicalNeeds.NullOrEmpty() || !pawnExtension.mechanicalNeeds.ContainsKey(nd)) && !___pawn.health.hediffSet.hediffs.Any(hediff => hediff.def.causesNeed == nd))
                {
                    __result = false;
                }
            }
        }
    }
}