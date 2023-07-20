using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MechHumanlikes
{
    public class Recipe_CommandDrone : Recipe_SurgeryMechanical
    {
        // This recipe is specifically targetting the brain of a mechanical unit, so we only need to check if the brain is available (a slight optimization over checking fixed body parts).
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {

            BodyPartRecord targetBodyPart = pawn.health.hediffSet.GetBrain();
            if (targetBodyPart != null)
            {
                yield return targetBodyPart;
            }
            yield break;
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Pawn pawn && (MHC_Utils.IsConsideredMechanicalDrone(pawn) || MHC_Utils.IsConsideredMechanicalAnimal(pawn)) && pawn.Faction != Faction.OfPlayer)
            {
                return true;
            }
            return false;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                pawn.health.AddHediff(recipe.addsHediff, part, null);
                // Handle success state
                if (!CheckSurgeryFailMechanical(billDoer, pawn, ingredients, part, null))
                {
                    TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                    {
                        billDoer,
                        pawn
                    });
                    pawn.SetFaction(Faction.OfPlayer, null);
                    Find.LetterStack.ReceiveLetter("MHC_ReprogramSuccess".Translate(), "MHC_ReprogramSuccessDesc".Translate(pawn.Name.ToStringShort), LetterDefOf.PositiveEvent, pawn, null);
                    return;
                }
                Find.LetterStack.ReceiveLetter("MHC_ReprogramFailed".Translate(), "MHC_ReprogramFailedDesc".Translate(pawn.Name.ToStringShort), LetterDefOf.NegativeEvent, pawn);
            }
        }
    }
}