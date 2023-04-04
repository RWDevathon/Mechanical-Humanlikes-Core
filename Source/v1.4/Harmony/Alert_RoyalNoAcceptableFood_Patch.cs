using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace MechHumanlikes
{
    // Do not give an alert about no food for charge-capable royals.
    public class Alert_RoyalNoAcceptableFood_Patch
    {
        [HarmonyPatch(typeof(Alert_RoyalNoAcceptableFood), "get_Targets")]
        public class Alert_RoyalNoAcceptableFood_get_Targets_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref List<Pawn> __result)
            {
                __result.RemoveAll(pawn => MHC_Utils.CanUseBattery(pawn));
            }
        }
    }
}