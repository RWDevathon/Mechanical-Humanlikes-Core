using System;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace MechHumanlikes
{
    public class AttackTargetFinder_Patch
    {
        // This transpiler alters a validator for AttackTargetFinder to ensure that enemy AI's with EMP weapons will consider mechanical units as vulnerable targets for attacking.
        // Otherwise, enemies with EMP weapons will ignore units entirely, preferring to melee or simply outright ignoring.
        [HarmonyPatch]
        public class BestAttackTarget_innerValidator_Patch
        {
            [HarmonyPatch]
            static MethodInfo TargetMethod()
            {
                return typeof(AttackTargetFinder).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(target => target.ReturnType == typeof(bool) && target.GetParameters().First().ParameterType == typeof(IAttackTarget));
            }

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetMethod = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsFlesh));

                // Yield the actual instructions, adding in our additional instructions where necessary.
                for (int i = 0; i < instructions.Count; i++)
                {
                    // Operation target hit, yield instruction and insert new instructions.
                    if (instructions[i].Calls(targetMethod))
                    {
                        yield return instructions[i]; // Use the IsFlesh instruction, which will be passed to our method below
                        yield return new CodeInstruction(OpCodes.Ldloc_1); // Load Pawn
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BestAttackTarget_innerValidator_Patch), nameof(Invulnerable))); // Our function call
                    }
                    // Not a target, return instruction as normal.
                    else
                    {
                        yield return instructions[i];
                    }
                }
            }

            private static bool Invulnerable(bool vulnerableSoFar, Pawn pawn)
            {
                return vulnerableSoFar && pawn.def.GetModExtension<MHC_MechanicalPawnExtension>()?.vulnerableToEMP != true;
            }
        }
    }
}