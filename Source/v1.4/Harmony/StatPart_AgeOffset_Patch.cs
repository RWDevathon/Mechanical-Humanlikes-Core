using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class StatPart_AgeOffset_Patch
    {
        // Age as a StatPart is not used for mechanical units. This resolves issues around "baby" mechanical units with reduced work speed, and other related issues.
        [HarmonyPatch(typeof(StatPart_AgeOffset), "ActiveFor")]
        public class ActiveFor_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref bool __result)
            {
                if (MHC_Utils.IsConsideredMechanical(pawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}