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

        private static void ShelterDoor_Close(On.ShelterDoor.orig_Close orig, ShelterDoor self)
        {
            if (self.IsClosing || self.room.PlayersInRoom.Count < 1)
            {
                orig(self);
                return;
            }
            orig(self);

            Player player = self.room.PlayersInRoom[0];

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
                        if (itemType.Contains('.'))
                        {
                            itemType = itemType.Substring(itemType.IndexOf('.') + 1);
                        }

                        shelterItems.Add(itemType);
                    }
                }
            }
        }
    }
}