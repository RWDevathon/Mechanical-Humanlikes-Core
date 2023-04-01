using Verse;

namespace MechHumanlikes
{
    public abstract class MaintenanceWorker
    {
        public HediffDef def;

        public MHC_MaintenanceEffectExtension effecter;

        // Method for identifying what maintenance effects are applicable to a race. CompMaintenanceNeed caches valid Hediffs for pawns based on this.
        public virtual bool CanEverApplyTo(RaceProperties race)
        {
            return true;
        }

        // Method for identifying whether the maintenance effect may be applied to a particular pawn right now.
        public virtual bool CanApplyTo(Pawn pawn)
        {
            return CanEverApplyTo(pawn.def.race);
        }

        // Method for identifying whether the maintenance effect may be applied to a particular pawn's part right now.
        public virtual bool CanApplyOnPart(Pawn pawn, BodyPartRecord part)
        {
            return true;
        }
        
        // Method for specifying special code to be done when the maintenance effect is first applied. It is called after the Hediff is applied. Part can be null if applied to the whole body.
        public virtual void OnApplied(Pawn pawn, BodyPartRecord part)
        {
        }
    }
}
