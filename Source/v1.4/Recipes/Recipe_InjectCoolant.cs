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

        // Explicitly do nothing in this method as it gets handled in ApplyOnPawn instead.
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            foreach (Thing ingredient in ingredients)
            {
                // Fulfill mech needs.
                Dictionary<NeedDef, float> needFulfillment = ingredient.def.GetModExtension<MHC_NeedFulfillerExtension>().needOffsetRelations;
                foreach (NeedDef needDef in needFulfillment.Keys)
                {
                    Need need = pawn.needs.TryGetNeed(needDef);
                    if (need == null)
                    {
                        continue;
                    }
                    need.CurLevel += needFulfillment[needDef] * ingredient.stackCount;
                }
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                ingredients[i].Destroy();
            }
        }

        public override float GetIngredientCount(IngredientCount ing, Bill bill)
        {
            if (bill.billStack?.billGiver is Pawn pawn)
            {
                return Mathf.Min(bill.Map.listerThings.ThingsOfDef(MHC_ThingDefOf.MHC_CoolantPack).Sum((Thing x) => x.stackCount), Mathf.FloorToInt(((Need_MechanicalNeed) pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant)).CoolantDesired / 0.5f));
            }
            return base.GetIngredientCount(ing, bill);
        }
    }
}