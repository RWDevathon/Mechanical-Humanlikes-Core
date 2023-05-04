using Verse;

namespace MechHumanlikes
{
    // ModExtension marking a NeedDef as belonging only to mechanical pawns, with appropriate details for handling what pawns are valid and how to operate the Need.
    public class MHC_MechanicalNeedExtension : DefModExtension
    {
        // Bools for if this need is specific to mechanical sapient pawns or drones.
        public bool droneOnly = false;
        public bool sapientOnly = false;

        // Floats for the thresholds at which this need should be considered critical to affect when the pawn should automatically try to satisfy it.
        public float criticalThreshold = 0.15f;

        // Optional hediff to apply when need hits 0, and how quickly it will rise while 0 and fall when not 0, as well as a statFactor that can modify it.
        // These fields are only useful if the NeedDef uses the Need_MechanicalNeed worker class.
        public HediffDef hediffToApplyOnEmpty;
        public float hediffRisePerDay = 1;
        public float hediffFallPerDay = 4;
    }
}