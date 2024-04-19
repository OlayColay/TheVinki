using System;
using System.Collections.Generic;
using SlugBase.SaveData;

namespace Vinki;

public class GraffitiObject : CustomDecal
{
    [Serializable]
    public struct SerializableGraffiti(PlacedObject placedObject, int cyclePlaced, int gNum)
    {
        public string data = (placedObject.data as PlacedObject.CustomDecalData).ToString();
        public float x = placedObject.pos.x, y = placedObject.pos.y;
        public int cyclePlaced = cyclePlaced;
        public int gNum = gNum;
    }

    private SerializableGraffiti serializableGraffiti;
	private readonly int cyclePlaced = 0;

    public GraffitiObject(PlacedObject placedObject, SaveState save, int gNum, string roomId, bool isStory) : base(placedObject)
    {
        if (save == null)
        {
            return;
        }

        cyclePlaced = isStory ? -1 : save.cycleNumber;
        serializableGraffiti = new(placedObject, cyclePlaced, gNum);

        SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(save.miscWorldSaveData);

        // Graffitis are indexed by room
        if (miscSave.TryGet("PlacedGraffitis", out Dictionary<string, List<SerializableGraffiti>> placedGraffitis))
        {
            // Add this graffiti to the dictionary
            if (!placedGraffitis.ContainsKey(roomId))
            {
                placedGraffitis[roomId] = [];
            }
            placedGraffitis[roomId].Add(serializableGraffiti);

            miscSave.Set("PlacedGraffitis", placedGraffitis);
        }
        else
        {
            // C# magic to create a new dictionary initialized with this graffiti
            miscSave.Set("PlacedGraffitis", new Dictionary<string, List<SerializableGraffiti>>() { { roomId, new() { { serializableGraffiti } } } });
        }
    }
}
