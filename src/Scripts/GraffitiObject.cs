using System;
using System.Collections.Generic;
using SlugBase.SaveData;

namespace Vinki;

public class GraffitiObject : CustomDecal
{
    [Serializable]
    public struct SerializableGraffiti
    {
        public string data;
        public float x, y;
        public int cyclePlaced;
        public int gNum;

        public SerializableGraffiti(PlacedObject placedObject, int cyclePlaced, int gNum)
        {
            data = (placedObject.data as PlacedObject.CustomDecalData).ToString();
            x = placedObject.pos.x;
            y = placedObject.pos.y;
            this.cyclePlaced = cyclePlaced;
            this.gNum = gNum;
        }
    }

    private SerializableGraffiti serializableGraffiti;
	private readonly int cyclePlaced = 0;

    public GraffitiObject(PlacedObject placedObject, SaveState save, int gNum, string roomId) : base(placedObject)
    {
        if (save == null)
        {
            return;
        }

        cyclePlaced = (gNum < Plugin.storyGraffitiCount) ? -1 : save.cycleNumber;
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
