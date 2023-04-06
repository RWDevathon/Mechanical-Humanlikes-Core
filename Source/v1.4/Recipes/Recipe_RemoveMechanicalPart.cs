using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace MechHumanlikes
{
    public class Recipe_RemoveMechanicalPart : Recipe_SurgeryMechanical
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (!MHC_Utils.IsConsideredMechanical(pawn))
            {
                yield break;
            }

            IEnumerable<BodyPartRecord> notMissingParts = pawn.health.hediffSet.GetNotMissingParts();
            foreach (BodyPartRecord part in notMissingParts)
            {
                if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
                {
                    yield return part;
                }
                else if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
                {
                    yield return part;
                }
                else if (part != pawn.RaceProps.body.corePart && part.def.canSuggestAmputation && pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part))
                {
                    yield return part;
                }
                else if (part.def.forceAlwaysRemovable)
                {
                    yield return part;
                }
            }
        }

        // Remove the body part from the pawn after the surgery is complete. Also handle returning the part itself and restoring any body parts.
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                // Check if the surgery failed. If it did, exit early.
                if (CheckSurgeryFailMechanical(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }

                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);

                if (pawn.health.hediffSet.GetNotMissingParts().Contains(part))
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where((Hediff x) => x.Part == part))
                    {
                        if (hediff.def.spawnThingOnRemoved != null)
                        {
                            GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, billDoer.Position, billDoer.Map);
                        }
                    }
                }

                // If the removed part represented the entire body part, then removing it would normally leave the part destroyed. Instead, restore this part and all children to normal functionality.
                if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part).ToList())
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                    RestoreChildParts(pawn, part);
                }

                // If removing the part represents a violation of a foreign pawn's trust, then report it.
                if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
                {
                    ReportViolation(pawn, billDoer, pawn.HomeFaction, -40);
                }
            }
        }

        // Recursively restore all parts that were missing as a result of the added body part. IE. hands and fingers when an arm upgrade was removed.
        private void RestoreChildParts(Pawn pawn, BodyPartRecord part)
        { 
            if (part == null)
                return;
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part == part && !hediff.def.keepOnBodyPartRestoration && hediff.def.isBad).ToList())
            {
                pawn.health.RemoveHediff(hediff);
            }
            foreach (BodyPartRecord childPart in part.GetDirectChildParts())
            {
                RestoreChildParts(pawn, childPart);
            }
        }
    }
}