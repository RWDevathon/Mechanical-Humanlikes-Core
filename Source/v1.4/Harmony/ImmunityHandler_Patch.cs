using Verse;
using HarmonyLib;
using System;

namespace MechHumanlikes
{
    internal class ImmunityHandler_Patch
    {
        // Mechanical units are immune to contracting diseases.
        [HarmonyPatch(typeof(ImmunityHandler), "DiseaseContractChanceFactor")]
        [HarmonyPatch(new Type[] { typeof(HediffDef), typeof(BodyPartRecord) })]
        public class DiseaseContractChanceFactor_Patch
        {
            [HarmonyPostfix]
            public static void Listener(HediffDef diseaseDef, BodyPartRecord part, ref float __result, Pawn ___pawn)
            {
                if (__result != 0f && Utils.IsConsideredMechanical(___pawn))
                {
                    __result = 0;
                }
            }
        }
    }
}