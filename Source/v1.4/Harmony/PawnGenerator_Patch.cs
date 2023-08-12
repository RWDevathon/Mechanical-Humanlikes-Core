using Verse;
using HarmonyLib;
using System;

namespace MechHumanlikes
{
    public class PawnGenerator_Patch
    {
        // Prefix pawn generation for drones so they have the appropriate gender. This will allow vanilla pawn gen to handle various details like name and body type automatically.
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn")]
        [HarmonyPatch(new Type[] { typeof(PawnGenerationRequest) }, new ArgumentType[] { ArgumentType.Normal })]
        public class GeneratePawn_Prefix
        {
            [HarmonyPrefix]
            public static bool Prefix(ref PawnGenerationRequest request)
            {
                ThingDef thingDef = request.KindDef?.race;
                if (thingDef != null && MHC_Utils.IsConsideredMechanicalDrone(thingDef))
                {
                    request.FixedGender = Gender.None;
                }
                // This prefix will always allow vanilla pawn gen to continue
                return true;
            }
        }

        // Patch pawn generation for mechanical drones so they have appropriate related mechanics at generation.
        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn")]
        [HarmonyPatch(new Type[] { typeof(PawnGenerationRequest) }, new ArgumentType[] { ArgumentType.Normal })]
        public class GeneratePawn_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref Pawn __result)
            {
                try
                {
                    // Drones have some special mechanics that need to be specifically handled.
                    if (MHC_Utils.IsConsideredMechanicalDrone(__result))
                    {
                        MHC_Utils.ReconfigureDrone(__result);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("[MHC] PawnGenerator.GeneratePawn " + ex.Message + " " + ex.StackTrace);
                }
            }
        }
    }
}