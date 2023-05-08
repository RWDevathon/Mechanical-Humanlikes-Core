using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MechHumanlikes
{
    // Mod extension for food and drugs for controls over which pawns may consume it.
    public class MHC_NeedFulfillerExtension : DefModExtension
    {
        // Bool for allowing or disallowing organics to consume this substance.
        public bool consumableByOrganics = true;
        public bool consumableByMechanicals = true;

        // Dictionary linking NeedDefs to how much that need will be offset when this is consumed.
        public Dictionary<NeedDef, float> needOffsetRelations;

        // Conditions for whether a particular race can consume a particular substance is an HAR feature of race restrictions and not necessary here.

        public override IEnumerable<string> ConfigErrors()
        {
            if (!consumableByOrganics && !consumableByMechanicals)
            {
                yield return "[MHC] A Thing has been marked as disallowed for both organics and mechanicals to consume. This will prevent it from ever being ingested by anyone at all!";
            }

            if (!consumableByMechanicals && needOffsetRelations != null)
            {
                yield return "[MHC] A Thing has been marked as disallowed for mechanical units to consume but set to satisfy mechanical needs. This doesn't make sense.";
            }
        }
    }
}