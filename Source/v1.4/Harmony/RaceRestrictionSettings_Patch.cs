using Verse;
using System;
using HarmonyLib;
using AlienRace;
using RimWorld;

namespace MechHumanlikes
{
    // NOTE: This is for harmony patches on Humanoid Alien Race's Assembly, not Core!
    // Piggyback off HAR's code to make sure pawns do not end up with blacklisted traits.
    public class RaceRestrictionSettings_Patch
    {
        [HarmonyPatch(typeof(RaceRestrictionSettings), "CanGetTrait")]
        [HarmonyPatch(new Type[] { typeof(TraitDef), typeof(Pawn), typeof(int)}, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        public class CanGetTrait_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(TraitDef trait, Pawn pawn, int degree, ref bool __result)
            {
                // Drones that do not have dronesCanHaveTraits enabled can not have traits.
                if (MHC_Utils.IsConsideredMechanicalDrone(pawn) && pawn.def.GetModExtension<MHC_MechanicalPawnExtension>()?.dronesCanHaveTraits != true)
                {
                    __result = false;
                    return false;
                }

                // If HAR's race settings whitelists this trait, yield to that setting.
                RaceRestrictionSettings raceRestrictionSettings = (pawn.def as ThingDef_AlienRace)?.alienRace?.raceRestriction;
                if (raceRestrictionSettings?.whiteTraitList.Contains(trait) == true)
                {
                    return true;
                }

                // If the pawn is a sapient and this trait is blacklisted, it can not have it.
                if (MHC_Utils.IsConsideredMechanicalSapient(pawn) && MechHumanlikes_Settings.blacklistedMechanicalTraits.Contains(trait.defName))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}