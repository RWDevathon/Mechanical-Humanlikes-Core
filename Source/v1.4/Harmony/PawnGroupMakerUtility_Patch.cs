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
            public static IEnumerable<Pawn> Listener(IEnumerable<Pawn> __result, PawnGroupMakerParms parms, bool warnOnZeroResults)
            {
                foreach (Pawn member in __result)
                {
                    if (MHC_Utils.IsConsideredMechanical(member))
                    {
                        if (member.GetComp<CompMaintenanceNeed>() is CompMaintenanceNeed maintenanceNeed)
                        {
                            maintenanceNeed.maintenanceEffectTicks = Rand.Range(-300000f, 300000f);
                        }
                    }
                    yield return member;
                }
            }
        }
    }
}