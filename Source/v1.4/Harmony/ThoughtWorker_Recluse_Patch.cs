using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace MechHumanlikes
{
    public class ThoughtWorker_Recluse_Patch
    {
        // This transpiler stops pawns from trying to feed pawns that are currently charging.
        [HarmonyPatch(typeof(ThoughtWorker_Recluse), "CurrentStateInternal")]
        public class ThoughtWorker_Recluse_CurrentStateInternal_Patch
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts, ILGenerator generator)
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(insts);
                MethodInfo targetProperty = AccessTools.PropertyGetter(typeof(MapPawns), nameof(MapPawns.FreeColonistsAndPrisonersSpawnedCount));

                for (int i = 0; i < instructions.Count; i++)
                {
                    yield return instructions[i];
                    if (instructions[i].Calls(targetProperty))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // Load pawn onto the stack so we can grab the map.
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ThoughtWorker_Recluse_CurrentStateInternal_Patch), nameof(ReduceByPlayerDrones))); // Our function call
                    }
                }
            }

            // Steals the existing int representing the number of colonists/prisoners and reduces it by the number of spawned free/imprisoned drones.
            private static int ReduceByPlayerDrones(int colonistsAndPrisoners, Pawn pawn)
            {
                return colonistsAndPrisoners - pawn.Map.GetComponent<MHC_MapComponent>().playerDronesSpawned;
            }
        }
    }
}