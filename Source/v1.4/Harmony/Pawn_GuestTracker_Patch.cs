using HarmonyLib;
using RimWorld;
using Verse;

namespace MechHumanlikes
{
    // The first time a drone is captured, a letter should be sent to the player that since drones can't be interacted with, they have no prisoner tab and should be surgically worked on.
    [HarmonyPatch(typeof(Pawn_GuestTracker), "CapturedBy")]
    public static class Pawn_GuestTracker_Patch
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
}
