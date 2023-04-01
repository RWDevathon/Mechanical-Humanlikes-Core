using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MechHumanlikes
{
    // Mod extension for races to control some features. These attributes are only used for humanlikes, there is no reason to provide any to non-humanlikes.
    public class MHC_MechanicalPawnExtension : DefModExtension
    {
        // Bool for whether this race may be assigned as a sapient and drone, respectively. Disabling both would effectively mean this pawn is blacklisted from being mechanical.
        // However, disabling both will throw a config error, as having the extension will result in some other changes elsewhere, for details like corpses rotting and what category they reside in.
        public bool canBeSapient = true;
        public bool canBeDrone = true;

        // Bool for whether this race has the maintenance need added by this mod. Setting this to disabled may be preferred for other race mods with their own maintenance needs.
        public bool needsMaintenance = true;

        // Int for the stat levels of this race when set as a drone. This does nothing if the race is not considered drones.
        public int droneSkillLevel = 8;

        // Bool for whether members of this race are vulnerable to EMP attacks and whether hostiles will attempt to use EMP weapons against them.
        public bool vulnerableToEMP = true;

        // Controls for what name makers None gendered pawns of this race will use. Vanilla will default them to use Male names, whereas these settings will allow for custom name makers for sapients and drones.
        public bool useCustomNoneGenderNameMakers = false;
        public RulePackDef sapientNoneGenderNameMaker;
        public RulePackDef droneNoneGenderNameMaker;

        // Controls for what backstory the drones of this race will be set to. If the bool is true, this mod will trust the PawnKindDefs to provide correct backstories. This does nothing if the race is not considered drones.
        public bool letPawnKindHandleDroneBackstories = true; // This is likely desired to be false if a race can be either sapients or drones!
        public BackstoryDef droneChildhoodBackstoryDef;
        public BackstoryDef droneAdulthoodBackstoryDef;

        // Bool for whether drones can have traits.
        public bool dronesCanHaveTraits = false;

        // List of needs that all units of this ThingDef do not have. Note that food and rest are unnecessary here, as HAR has a needsRest tag to enable/disable rest and setting foodType to None disables needing food/energy.
        public List<NeedDef> blacklistedNeeds;

        // List of needs that specifically mechanical sapients do not have. Same note as above.
        public List<NeedDef> blacklistedSapientNeeds;


        public override IEnumerable<string> ConfigErrors()
        {
            if (!canBeSapient && !canBeDrone)
            {
                yield return "[MHC] A race has both canBeSapient and canBeDrone set to false! This means it can not be mechanical. This extension should be removed from the race and will cause problems.";
            }

            if (!canBeSapient && !letPawnKindHandleDroneBackstories && (droneChildhoodBackstoryDef == null || droneAdulthoodBackstoryDef == null))
            {
                yield return "[MHC] A race was set to only be drones, does not allow drone backstories to be given by pawn kinds, and did not specify backstories directly! This is illegal!";
            }

            if (useCustomNoneGenderNameMakers && (sapientNoneGenderNameMaker == null || droneNoneGenderNameMaker == null))
            {
                yield return "[MHC] A race was set to use custom name makers for none gendered pawns but did not specify what those custom name makers were! This will cause issues.";
            }
        }
    }
}