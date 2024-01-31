using Verse;

namespace MechHumanlikes
{
    public class MHC_MapComponent : MapComponent
    {
        public int playerDronesSpawned = 0;

        public MHC_MapComponent(Map map) : base(map) 
        {
        }

        public override void MapComponentTick()
        {
            if (map.IsHashIntervalTick(GenTicks.TickLongInterval))
            {
                playerDronesSpawned = 0;
                for (int i = map.mapPawns.FreeColonistsAndPrisonersSpawnedCount - 1; i >= 0; i--)
                {
                    if (MHC_Utils.IsConsideredMechanicalDrone(map.mapPawns.FreeColonistsAndPrisonersSpawned[i]))
                    {
                        playerDronesSpawned++;
                    }
                }
            }
        }
    }
}
