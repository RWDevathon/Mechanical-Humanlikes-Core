using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace MechHumanlikes
{
    internal class ThingSetMaker_RefugeePod_Patch
    {
        // Mechanical "refugees" in pods reboot as they feel no pain and thus may not be immobilized otherwise.
        [HarmonyPatch(typeof(ThingSetMaker_RefugeePod), "Generate")]
        public class Generate_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ThingSetMakerParams parms, ref List<Thing> outThings)
            {
                for (int i = outThings.Count - 1; i >= 0; i--)
                {
                    Thing thing = outThings[i];
                    if (thing is Pawn pawn)
                    {
                        if (Utils.IsConsideredMechanical(pawn))
                        {
                            pawn.health.AddHediff(MHC_HediffDefOf.MHC_Restarting);
                        }
                    }
                }
            }
        }
    }
}