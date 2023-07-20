using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MechHumanlikes
{
    public class Recipe_RebootUnit : Recipe_SurgeryMechanical
    {
        // This recipe always targets the core part as something easy to target that all pawns are guaranteed to have.
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            yield return pawn.RaceProps.body.corePart;
        }

        // This is valid for all mechanical pawns at all times.
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return MHC_Utils.IsConsideredMechanical(thing.def);
        }

        // On completion, apply the desired hediff to the whole body.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }
            pawn.health.AddHediff(recipe.addsHediff);
        }
    }
}
