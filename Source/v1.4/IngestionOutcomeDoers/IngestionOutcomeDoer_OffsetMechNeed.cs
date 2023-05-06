using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MechHumanlikes
{
    public class IngestionOutcomeDoer_OffsetMechNeed : IngestionOutcomeDoer_OffsetNeed
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (pawn.needs == null)
            {
                return;
            }
            Need need = pawn.needs.TryGetNeed(this.need);
            if (need != null)
            {
                float effect = offset;
                if (perIngested)
                {
                    effect *= ingested.stackCount;
                }
                need.CurLevel += effect;
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawnImportant, need.LabelCap, (offset >= 0f) ? "+" : string.Empty + Mathf.RoundToInt(offset * 100f), need.description, need.listPriority);
        }
    }
}
