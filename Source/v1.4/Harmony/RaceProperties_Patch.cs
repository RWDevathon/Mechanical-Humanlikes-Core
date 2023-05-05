using Verse;
using HarmonyLib;
using AlienRace;
using System;

namespace MechHumanlikes
{
    public class RaceProperties_Patch
    {
        // Patch to prevent mechanical units from consuming things restricted to organics, and to prevent organics from consuming things restricted to mechanicals.
        [HarmonyPatch(typeof(RaceProperties), "CanEverEat")]
        [HarmonyPatch(new Type[] { typeof(ThingDef) })]
        public class CanEverEat_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ThingDef t, ref bool __result, ref RaceProperties __instance)
            {
                if (!__result)
                {
                    return;
                }

                // Get the food extension for this thing def. If there isn't one, then we leave the original result.
                MHC_FoodExtension foodExtension = t.GetModExtension<MHC_FoodExtension>();
                if (foodExtension == null)
                {
                    return;
                }

                // Get the thing def for the race from the race properties (using HAR's cache).
                ThingDef pawnDef = CachedData.GetRaceFromRaceProps(__instance);

                // If it is mechanical, ensure the food is compatible with mechanical units.
                if (MHC_Utils.IsConsideredMechanical(pawnDef) && !foodExtension.consumableByMechanicals)
                {
                    __result = false;
                }
                // If it is organic, ensure it is compatible with organic pawns.
                else if (!MHC_Utils.IsConsideredMechanical(pawnDef) && !foodExtension.consumableByOrganics)
                {
                    __result = false;
                }
            }
        }
    }
}