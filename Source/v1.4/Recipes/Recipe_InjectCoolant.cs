using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MechHumanlikes
{
    public class Recipe_InjectCoolant : Recipe_Surgery
    {
        public const float BloodlossHealedPerPack = 0.35f;

        public override bool CompletableEver(Pawn surgeryTarget)
        {
            Need coolantNeed = surgeryTarget.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant);
            if (coolantNeed == null)
            {
                return false;
            }
            return base.CompletableEver(surgeryTarget);
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (thing.MapHeld == null)
            {
                return false;
            }
            if (thing is Pawn pawn && pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant) == null)
            {
                return false;
            }
            return base.AvailableOnNow(thing, part);
        }
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            float gain = 0;
            foreach (Thing ingredient in ingredients)
            {
                float gainAmount = (ingredient.def.ingestible?.outcomeDoers.Find(outcomeDoer => outcomeDoer is IngestionOutcomeDoer_OffsetNeed needOffsetter && needOffsetter.need == MHC_NeedDefOf.MHC_Coolant) as IngestionOutcomeDoer_OffsetNeed).offset;
                gain += ingredient.stackCount * gainAmount;
            }

            pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant).CurLevel += gain;
            for (int i = 0; i < ingredients.Count; i++)
            {
                ingredients[i].Destroy();
            }
        }

        public override float GetIngredientCount(IngredientCount ing, Bill bill)
        {
            if (bill.billStack?.billGiver is Pawn pawn)
            {
                return Mathf.Min(bill.Map.listerThings.ThingsOfDef(MHC_ThingDefOf.MHC_CoolantPack).Sum((Thing x) => x.stackCount), ((Need_MechanicalNeed) pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant)).CoolantDesired / 0.35f);
            }
            return base.GetIngredientCount(ing, bill);
        }
    }
}