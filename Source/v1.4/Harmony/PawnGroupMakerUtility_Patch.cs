using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace MechHumanlikes
{
    public class MHC_PawnGroupMakerUtility_Patch
    {
        // Give mechanical pawns in faction groups (raids, caravans, etc) random maintenance levels between -5 to 5 days.
        [HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns")]
        public class GeneratePawns_Patch
        {
            [HarmonyPostfix]
            public static void Listener(PawnGroupMakerParms parms, bool warnOnZeroResults, ref IEnumerable<Pawn> __result)
            {
                List<Pawn> modifiedResults = __result.ToList();
                foreach (Pawn member in modifiedResults)
                {
                    if (MHC_Utils.IsConsideredMechanical(member))
                    {
                        if (member.GetComp<CompMaintenanceNeed>() is CompMaintenanceNeed maintenanceNeed)
                        {
                            maintenanceNeed.maintenanceEffectTicks = Rand.Range(-300000f, 300000f);
                        }
                    }
                }
                __result = modifiedResults.AsEnumerable();
            }
        }
    }
}