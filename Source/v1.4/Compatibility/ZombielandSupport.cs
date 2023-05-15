using MechHumanlikes;
using Verse;

public class ZombielandSupport
{
    // Mechanical pawns can not be zombies.
    public static bool CanBecomeZombie(Pawn pawn)
    {
        return !MHC_Utils.IsConsideredMechanical(pawn);
    }

    // Mechanical pawns do not attract the attention of zombies.
    public static bool AttractsZombies(Pawn pawn)
    {
        return !MHC_Utils.IsConsideredMechanical(pawn);
    }
}