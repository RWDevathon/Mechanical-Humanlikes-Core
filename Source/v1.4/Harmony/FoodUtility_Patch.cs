using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class FoodUtility_Patch
    {
        // Mechanical units can not be food poisoned, so set their chance to receive food poisoning to zero.
        [HarmonyPatch(typeof(FoodUtility), "GetFoodPoisonChanceFactor")]
        public class GetFoodPoisonChanceFactor_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ingester, ref float __result)
            {
                if (MHC_Utils.IsConsideredMechanical(ingester))
                {
                    __result = 0f;
                    return false;
                }
                return true;
            }
        }

        // Modify the estimated nutrition for a food by the pawn's NutritionalIntakeEfficiency
        [HarmonyPatch(typeof(FoodUtility), "GetNutrition")]
        public class GetNutrition_patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn eater, Thing foodSource, ThingDef foodDef, ref float __result)
            {
                __result *= eater.GetStatValue(MHC_StatDefOf.MHC_NutritionalIntakeEfficiency, cacheStaleAfterTicks: 1200);
            }
        }
    }
}