using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class InteractionUtility_Patch
    {
        // Mechanical drones don't start social interactions.
        [HarmonyPatch(typeof(InteractionUtility), "CanInitiateInteraction")]
        public class CanInitiateInteraction_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref bool __result, InteractionDef interactionDef = null)
            {
                if (MHC_Utils.IsConsideredMechanicalDrone(pawn))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        // Mechanical drones don't receive social interactions.
        [HarmonyPatch(typeof(InteractionUtility), "CanReceiveInteraction")]
        public class CanReceiveInteraction_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref bool __result, InteractionDef interactionDef = null)
            {
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