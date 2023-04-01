using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    internal class ThoughtWorker_NeedNeuralSupercharge_Patch
    {
        // Mechanical units do not care about neural supercharges as their neural networks are different than organics.
        [HarmonyPatch(typeof(ThoughtWorker_NeedNeuralSupercharge), "ShouldHaveThought")]
        public class CurrentStateInternal_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn p, ref ThoughtState __result)
            {
                if (!__result.Active)
                    return;

                if (Utils.IsConsideredMechanicalSapient(p))
                {
                    __result = ThoughtState.Inactive;
                }
            }
        }
    }
}