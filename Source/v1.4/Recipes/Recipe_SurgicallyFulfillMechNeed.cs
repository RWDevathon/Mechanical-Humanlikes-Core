using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MechHumanlikes
{
    public class Recipe_SurgicallyFulfillMechNeed : Recipe_Surgery
    {
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
    }
}