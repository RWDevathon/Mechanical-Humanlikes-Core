using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MechHumanlikes
{
    public class Recipe_ExtractCoolant : Recipe_Surgery
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Pawn pawn && pawn.needs != null && pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant) == null)
            {
                return false;
            }
            return base.AvailableOnNow(thing, part);
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Pawn pawn && pawn.needs != null && pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant) == null)
            {
                return "MHC_NoCoolant".Translate();
            }
            return base.AvailableReport(thing, part);
        }

        public override bool CompletableEver(Pawn surgeryTarget)
        {
            if (base.CompletableEver(surgeryTarget))
            {
                return PawnHasEnoughReserveCoolantForExtraction(surgeryTarget);
            }
            return false;
        }

        public override void CheckForWarnings(Pawn medPawn)
        {
            base.CheckForWarnings(medPawn);
            if (!PawnHasEnoughReserveCoolantForExtraction(medPawn))
            {
                Messages.Message("MHC_CannotStartCoolantExtraction".Translate(medPawn.Named("PAWN")), medPawn, MessageTypeDefOf.NeutralEvent, historical: false);
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!PawnHasEnoughReserveCoolantForExtraction(pawn))
            {
                Messages.Message("MHC_PawnHadInsufficientCoolantToExtractCoolantPack".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
                return;
            }
            OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);
            if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            {
                ReportViolation(pawn, billDoer, pawn.HomeFaction, -1, MHC_HistoryEventDefOf.MHC_ExtractedCoolantPack);
            }
            HealthUtility.AdjustSeverity(pawn, MHC_HediffDefOf.MHC_CoolantShortage, 1);
            pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant).CurLevelPercentage = 0;
        }

        protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Thing coolantPack = ThingMaker.MakeThing(MHC_ThingDefOf.MHC_CoolantPack);
            coolantPack.stackCount = Mathf.FloorToInt(pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant).CurLevel / 0.5f);
            if (!GenPlace.TryPlaceThing(coolantPack, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near))
            {
                Log.Error("[MHC] Could not drop coolant pack near " + pawn.PositionHeld);
            }
        }

        private bool PawnHasEnoughReserveCoolantForExtraction(Pawn pawn)
        {
            Need coolantNeed = pawn.needs.TryGetNeed(MHC_NeedDefOf.MHC_Coolant);
            if (coolantNeed == null) 
            { 
                return false;
            }

            if (coolantNeed.CurInstantLevel < 0.5)
            {
                return false;
            }

            return true;
        }
    }
}