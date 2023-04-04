using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class Thing_Patch
    {
        // Player charge-capable pawns have modified nutritional intake values.
        [HarmonyPatch(typeof(Thing), "IngestedCalculateAmounts")]
        public class IngestedCalculateAmountsModifiedByBiogenEfficiency_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn ingester, ref float nutritionIngested)
            {
                // No patching is done on ingesting loss
                if (nutritionIngested <= 0f)
                    return;

                // Player charge-capable pawns (to avoid issues with foreign pawns not bringing enough food) have their ingested nutrition modified.
                if (MHC_Utils.CanUseBattery(ingester) && ingester.Faction == Faction.OfPlayer)
                {
                    nutritionIngested *= ingester.GetStatValue(MHC_StatDefOf.MHC_NutritionalIntakeEfficiency);
                }
            }
        }
    }
}