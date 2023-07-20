using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MechHumanlikes
{
    public class Recipe_PaintMechanicalUnit : Recipe_SurgeryMechanical
    {
        // This recipe always targets the core part, and is always applicable to "alien" mechanical races, as it would throw errors if not using AlienRace.
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            yield return pawn.RaceProps.body.corePart;
        }

        // This is valid only for mechanical humanlike pawns.
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return MHC_Utils.IsConsideredMechanicalSapient(thing.def) || MHC_Utils.IsConsideredMechanicalDrone(thing.def);
        }

        // On completion, open a dialog menu to select the new paint color.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Find.WindowStack.Add(new Dialog_Repaint(pawn));
        }
    }
}
