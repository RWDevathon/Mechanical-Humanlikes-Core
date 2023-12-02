using Verse;
using UnityEngine;

namespace MechHumanlikes
{
    [StaticConstructorOnStartup]
    public static class MHC_Textures
    {
        static MHC_Textures()
        {
        }
        public static readonly Texture2D VanillaBleedIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/Bleeding");

        // Settings
        public static readonly Texture2D DrawPocket = ContentFinder<Texture2D>.Get("UI/Icons/Settings/DrawPocket");

        // Medicine
        public static readonly Texture2D NoCare = ContentFinder<Texture2D>.Get("UI/Icons/Tabs/NoMechanicCare");
        public static readonly Texture2D NoMed = ContentFinder<Texture2D>.Get("UI/Icons/Tabs/NoRepairStims");
        public static readonly Texture2D RepairStimSimple = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/MHC_RepairStimSimple/MHC_RepairStimSimple_a");
        public static readonly Texture2D RepairStimIntermediate = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/MHC_RepairStimIntermediate/MHC_RepairStimIntermediate_a");
        public static readonly Texture2D RepairStimAdvanced = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/MHC_RepairStimAdvanced/MHC_RepairStimAdvanced_a");

        // Gizmos
        public static readonly Texture2D RestrictionGizmoIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/MHC_RestrictionGizmo");

        // Race Exemplars
        public static readonly Texture2D MechDroneExemplar = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MHC_MechDroneExemplar");
        public static readonly Texture2D MechSapientExemplar = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MHC_MechSapientExemplar");
        public static readonly Texture2D BasicHumanExemplar = ContentFinder<Texture2D>.Get("UI/Commands/ForColonists");
        public static readonly Texture2D MechDroneMHC_PawnTypeRestricted = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MHC_MechDronePawnTypeRestricted");
        public static readonly Texture2D MechSapientMHC_PawnTypeRestricted = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MHC_MechSapientPawnTypeRestricted");
        public static readonly Texture2D OrganicMHC_PawnTypeRestricted = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MHC_OrganicPawnTypeRestricted");
    }
}