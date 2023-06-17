using System;
using Verse;
using HarmonyLib;
using RimWorld;

namespace MechHumanlikes
{
    public class CompUseEffect_InstallImplantMechlink_Patch
    {
        // Non-sapient mechanical units may not have mechlinks installed - it can possibly generate errors!
        [HarmonyPatch(typeof(CompUseEffect_InstallImplantMechlink), "CanBeUsedBy")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(string) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out })]
        public class CanBeUsedBy_Patch
        {
            [HarmonyPostfix]
            public static void Listener(ref bool __result, Pawn p, ref string failReason)
            {
                if (!__result)
                {
                    return;
                }

                if (MHC_Utils.IsConsideredNonHumanlike(p))
                {
                    __result = false;
                    failReason = "MHC_PawnTypeAutonomousTooltip".Translate();
                    return;
                }
            }
        }
    }
}