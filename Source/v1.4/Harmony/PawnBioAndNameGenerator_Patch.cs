using HarmonyLib;
using RimWorld;
using Verse;

namespace MechHumanlikes
{
    public class PawnBioAndNameGenerator_Patch
    {
        // Override the generation of names for mechanical sapients and drones. Because mod settings allow for swapping these things around, xml doesn't do the trick. This is Run-Time information.
        [HarmonyPatch(typeof(PawnBioAndNameGenerator), "GeneratePawnName")]
        public class GeneratePawnName_Patch
        {
            [HarmonyPostfix]
            public static void Listener(Pawn pawn, ref Name __result, NameStyle style = NameStyle.Full, string forcedLastName = null)
            {
                // Only override non-numeric name generations.
                if (style == NameStyle.Numeric)
                    return;

                MHC_MechanicalPawnExtension modExtension = pawn.def.GetModExtension<MHC_MechanicalPawnExtension>();
                if (modExtension?.useCustomNoneGenderNameMakers != true)
                {
                    return;
                }

                // None gendered mechanical sapients will take the sapient custom name maker from the mod extension
                if (MHC_Utils.IsConsideredMechanicalSapient(pawn) && pawn.gender == Gender.None && pawn.RaceProps.hasGenders)
                {
                    __result = PawnBioAndNameGenerator.GenerateFullPawnName(pawn.def, modExtension.sapientNoneGenderNameMaker, pawn.story, null, modExtension.sapientNoneGenderNameMaker, pawn.Faction?.ideos?.PrimaryCulture, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName);
                }
                // Mechanical drones never have gender and so should always take the custom name maker if provided
                else if (MHC_Utils.IsConsideredMechanicalDrone(pawn) && pawn.def.GetModExtension<MHC_MechanicalPawnExtension>().letPawnKindHandleDroneBackstories == false)
                {
                    __result = PawnBioAndNameGenerator.GenerateFullPawnName(pawn.def, modExtension.droneNoneGenderNameMaker, pawn.story, null, null, pawn.Faction?.ideos?.PrimaryCulture, pawn.gender, pawn.RaceProps.nameCategory, forcedLastName);

                }
            }
        }
    }
}