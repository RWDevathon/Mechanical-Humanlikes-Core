using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MechHumanlikes
{
    // Override AffectQuality to use MHC_MechanicalSurgerySuccessChance instead of SurgerySuccessChance
    public class SurgeryOutcomeComp_MechanicSuccessChance : SurgeryOutcomeComp_SurgeonSuccessChance
    {
        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            quality *= surgeon.GetStatValue(MHC_StatDefOf.MHC_MechanicalSurgerySuccessChance);
        }
    }
}