using Verse;
using HarmonyLib;

namespace MechHumanlikes
{
    public class HediffUtility_Patch
    {
        // Mechanical units get a large number of bonus "implanted" parts when checked for number so transhumanists love being mechanical.
        [HarmonyPatch(typeof(HediffUtility), "CountAddedAndImplantedParts")]
        public class CountAddedParts_Patch
        {
            [HarmonyPostfix]
            public static void Listener(HediffSet hs, ref int __result)
            {
                if (MHC_Utils.IsConsideredMechanical(hs.pawn))
                {
                    __result += 20;
                }
            }
        }
    }
}