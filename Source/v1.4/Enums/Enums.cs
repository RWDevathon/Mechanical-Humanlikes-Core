using System;

namespace MechHumanlikes
{
    public enum OptionsTab { General, Health, Stats }
    public enum SettingsPreset { None, Default, Custom } //, DigitalWarfare, NoConnections, MetalSuperiority, FleshSuperiority }

    [Flags]
    public enum PawnType
    {
        None = 0b_0000_0000,  // 0
        Drone = 0b_0000_0001,  // 1
        Sapient = 0b_0000_0010,  // 2
        Mechanical = Drone | Sapient, // 3
        Organic = 0b_0000_0100,  // 4
        NonAI = Drone | Organic, // 5
        Autonomous = Sapient | Organic, // 6
        All = Drone | Sapient | Organic // 7
    }

    public enum DefModExtensionRole
    {
        Illegal,
        ChargeCapabilityMarker,
        MaintenanceEffectMarker,
        MechanicalPawnMarker
    }

    public enum MaintenanceStage
    {
        Critical,
        Poor,
        Sufficient,
        Satisfactory
    }

    public enum ServerType
    {
        None,
        SkillServer,
        SecurityServer,
        HackingServer
    }
}
