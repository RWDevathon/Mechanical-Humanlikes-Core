using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace MechHumanlikes
{
    public class Thought_Situational_Recluse_Patch
    {
        // This transpiler stops pawns from trying to feed pawns that are currently charging.
        [HarmonyPatch(typeof(Thought_Situational_Recluse), "MoodOffset")]
        public class Thought_Situational_Recluse_MoodOffset_Patch
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
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // Load the Thought onto the stack so we can grab the map from its pawn.
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Thought_Situational_Recluse_MoodOffset_Patch), nameof(ReduceByPlayerDrones))); // Our function call
                    }
                }
            }

            // Steals the existing int representing the number of colonists/prisoners and reduces it by the number of spawned free/imprisoned drones.
            private static int ReduceByPlayerDrones(int colonistsAndPrisoners, Thought thought)
            {
                return colonistsAndPrisoners - thought.pawn.Map.GetComponent<MHC_MapComponent>().playerDronesSpawned;
            }
        }
    }
}