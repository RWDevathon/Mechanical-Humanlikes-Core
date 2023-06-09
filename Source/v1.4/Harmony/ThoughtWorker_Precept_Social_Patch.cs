﻿using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class ThoughtWorker_Precept_Social_Patch
    {
        // Mechanical drones don't have precepts. Other pawns don't judge them for this.
        [HarmonyPatch(typeof(ThoughtWorker_Precept_Social), "CurrentSocialStateInternal")]
        public class CurrentSocialStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, Pawn otherPawn, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (MHC_Utils.IsConsideredMechanicalDrone(p) || MHC_Utils.IsConsideredMechanicalDrone(otherPawn))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}