using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class RitualObligationTrigger_MemberDied_Patch
    {
        // Non-humanlike intelligences do not trigger death related obligations.
        [HarmonyPatch(typeof(RitualObligationTrigger_MemberDied), "Notify_MemberDied")]
        public class Notify_MemberDied_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn p)
            {
                if (MHC_Utils.IsConsideredNonHumanlike(p))
                {
                    return false;
                }
                return true;
            }
        }

    }
}