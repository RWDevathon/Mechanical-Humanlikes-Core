using HarmonyLib;
using RimWorld;
using Verse;

namespace MechHumanlikes
{
    // When a drone is captured, a letter should be sent to the player that since drones can't be interacted with, they have no prisoner tab and should be surgically worked on.
    [HarmonyPatch(typeof(Pawn_GuestTracker), "CapturedBy")]
    public static class Pawn_GuestTracker_CapturedBy_Patch
    {
        [HarmonyPostfix]
        public static void Listener(Faction by, Pawn byPawn, Pawn ___pawn)
        {
            if (by == Faction.OfPlayer && MHC_Utils.IsConsideredMechanicalDrone(___pawn))
            {
                Messages.Message("MHC_DroneCaptured".Translate(___pawn), MessageTypeDefOf.CautionInput, false);
            }
        }
    }

    // Pawns with no food need cannot be brought food.
    [HarmonyPatch(typeof(Pawn_GuestTracker), "get_CanBeBroughtFood")]
    public static class Pawn_GuestTracker_get_CanBeBroughtFood_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn, ref bool __result)
        {
            if (___pawn.needs.food == null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
