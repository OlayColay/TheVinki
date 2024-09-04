using System.Collections.Generic;

namespace Vinki;
public static class GraffitiCreatureSpawner
{
    public readonly struct GraffitiDenSpawnData(CreatureTemplate.Type[] creatures, int remainInDenCounter)
    {
        public readonly CreatureTemplate.Type[] creatures = creatures;
        public readonly int remainInDenCounter = remainInDenCounter;
    }

    public static Dictionary<string, Dictionary<int, GraffitiDenSpawnData>> roomCreatures = [];

    public static void PopulateDictionary()
    {
        roomCreatures.Clear();
        roomCreatures["GW_TOWER05"] = new Dictionary<int, GraffitiDenSpawnData>
		{
            { 4, new GraffitiDenSpawnData([MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.MirosVulture], 100) },
			{ 6, new GraffitiDenSpawnData([MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TrainLizard], 500) }
        };
    }

	public static void TriggerSpawns(AbstractRoom room)
	{
		Plugin.VLogger.LogInfo("Spawning Graffiti Creatures in " + room.name);
		if (roomCreatures.ContainsKey(room.name))
		{
			foreach (KeyValuePair<int, GraffitiDenSpawnData> den in roomCreatures[room.name])
			{
				foreach (CreatureTemplate.Type type in den.Value.creatures)
				{
					AbstractCreature creature = new(room.world, StaticWorld.GetCreatureTemplate(type), null, new(room.index, -1, -1, den.Key), room.world.game.GetNewID());
					room.AddEntity(creature);
					creature.spawnDen = new(room.index, -1, -1, den.Key);
					creature.RealizeInRoom();
				}
			}
		}
	}
}
