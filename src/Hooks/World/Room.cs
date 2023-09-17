using SlugBase.SaveData;
using System;
using System.Collections.Generic;

namespace Vinki;

public static partial class Hooks
{
    // Add hooks
    private static void ApplyRoomHooks()
    {
        On.AbstractRoom.RealizeRoom += AbstractRoom_RealizeRoom;
        On.Room.Loaded += Room_Loaded;

        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
    }

    private static void AbstractRoom_RealizeRoom(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
    {
        orig(self, world, game);

        SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(world.game.GetStorySession.saveState.miscWorldSaveData);
        Dictionary<string, List<GraffitiObject.SerializableGraffiti>> placedGraffitis;
        if (game.GetStorySession.saveStateNumber != Enums.vinki || !miscSave.TryGet("PlacedGraffitis", out placedGraffitis) || 
            !placedGraffitis.ContainsKey(self.name))
        {
            return;
        }

        int thisCycle = game.GetStorySession.saveState.cycleNumber;
        for (int i = 0; i < placedGraffitis[self.name].Count; i++)
        {
            // If the graffiti's placement cycle + number of fade cycles >= the current cycle number (or if story graffiti)
            if (VinkiConfig.GraffitiFadeTime.Value == -1 || 
                placedGraffitis[self.name][i].cyclePlaced + VinkiConfig.GraffitiFadeTime.Value >= thisCycle || 
                placedGraffitis[self.name][i].cyclePlaced == -1)
            {
                // Spawn the graffiti
                PlacedObject.CustomDecalData decalData = new PlacedObject.CustomDecalData(null);
                decalData.FromString(placedGraffitis[self.name][i].data);
                PlacedObject placedObject = new PlacedObject(PlacedObject.Type.CustomDecal, decalData);
                placedObject.pos = new UnityEngine.Vector2(placedGraffitis[self.name][i].x, placedGraffitis[self.name][i].y);
                self.realizedRoom.AddObject(new CustomDecal(placedObject));
            }
            else if (VinkiConfig.DeleteGraffiti.Value)
            {
                // Delete this graffiti if it's faded out
                placedGraffitis[self.name].Remove(placedGraffitis[self.name][i]);
                i--;
            }
        }

        miscSave.Set("PlacedGraffitis", placedGraffitis);
    }

    private static CutsceneVinkiIntro intro = null;
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);
        if (self.game?.GetStorySession?.saveState?.saveStateNumber != Enums.vinki)
        {
            return;
        }

        if (self.abstractRoom?.name == "SS_AI" && self.game?.GetStorySession?.saveState?.miscWorldSaveData?.SSaiConversationsHad == 0)
        {
            intro = new CutsceneVinkiIntro(self);
            self.AddObject(intro);
        }
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room self)
    {
        orig(self);

        if (!self.game.IsStorySession || self.game.GetStorySession.saveState.saveStateNumber != Enums.vinki)
        {
            return;
        }

        if (!self.abstractRoom.firstTimeRealized)
        {
            return;
        }

        // Graffiti Tutorial
        else if (self.abstractRoom?.name == "SS_E08" && self.game.rainWorld.progression.currentSaveState.cycleNumber == 0)
        {
            self.AddObject(new GraffitiTutorial(self));
        }
    }
}
