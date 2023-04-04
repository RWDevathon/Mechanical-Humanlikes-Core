using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    // Mechanical units do not need doctors.
    public class Alert_NeedDoctor_Patch
    {
        [HarmonyPatch(typeof(Alert_NeedDoctor), "get_Patients")]
        public class get_Patients_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref List<Pawn> __result)
            {
                __result.RemoveAll(pawn => MHC_Utils.IsConsideredMechanical(pawn));
            }
        }
    }
}