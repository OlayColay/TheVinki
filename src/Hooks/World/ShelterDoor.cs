using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using static Vinki.Plugin;

namespace Vinki
{
    public static partial class Hooks
    {
        // Add hooks
        private static void ApplyShelterDoorHooks()
        {
            On.ShelterDoor.Close += ShelterDoor_Close;
        }
        private static void RemoveShelterDoorHooks()
        {
            On.ShelterDoor.Close -= ShelterDoor_Close;
        }

        private static void ShelterDoor_Close(On.ShelterDoor.orig_Close orig, ShelterDoor self)
        {
            if (self.IsClosing || self.room.PlayersInRoom.Count < 1 || self.room.game.GetStorySession.saveStateNumber != Enums.vinki)
            {
                orig(self);
                return;
            }
            orig(self);

            // We should have talked to Pebbles in the intro (case if died in cycle 0)
            self.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad = Math.Max(self.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad, 1);

            foreach(List<PhysicalObject> items in self.room.physicalObjects)
            {
                foreach (PhysicalObject item in items)
                {
                    if (item is PebblesPearl)
                    {
                        shelterItems.Add("DataPearl");
                    }
                    else
                    {
                        string itemType = item.GetType().ToString();
                        while (itemType.Contains('.'))
                        {
                            itemType = itemType.Substring(itemType.IndexOf('.') + 1);
                        }

                        //VLogger.LogInfo("Adding shelter item: " + itemType);
                        shelterItems.Add(itemType);
                    }
                }
            }
        }
    }
}