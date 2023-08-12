using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace MechHumanlikes
{
    public class IncidentWorker_DiseaseAnimal_Patch
    {
        // Mechanicals aren't valid candidates for diseases. Could possibly be transpiled, but difficult due to anonymous methods.
        [HarmonyPatch(typeof(IncidentWorker_DiseaseAnimal), "PotentialVictimCandidates")]
        public class PotentialVictimCandidates_Patch
        {
            [HarmonyPostfix]
            public static void Listener(IIncidentTarget target, ref IEnumerable<Pawn> __result)
            {
                if (__result == null)
                    return;

                __result = __result.Where(pawn => !MHC_Utils.IsConsideredMechanicalAnimal(pawn));
            }
        }
    }
}