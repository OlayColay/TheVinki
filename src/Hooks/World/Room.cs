using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        Dictionary<string, List<GraffitiObject.SerializableGraffiti>> placedGraffitis;
        if (game.GetStorySession == null || game.GetStorySession.saveStateNumber != Enums.vinki)
        {
            return;
        }

        SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(world.game.GetStorySession.saveState.miscWorldSaveData);
        if (!miscSave.TryGet("PlacedGraffitis", out placedGraffitis) || !placedGraffitis.ContainsKey(self.name))
        {
            return;
        }

        int thisCycle = game.GetStorySession.saveState.cycleNumber;
        for (int i = 0; i < placedGraffitis[self.name].Count; i++)
        {
            // The first three story graffitis are erased after one cycle because FP erases them
            if (placedGraffitis[self.name][i].cyclePlaced >= 0 || (placedGraffitis[self.name][i].gNum < Plugin.storyGraffitiCount && placedGraffitis[self.name][i].gNum >= 3))
            {
                // If the graffiti's placement cycle + number of fade cycles >= the current cycle number (or if story graffiti)
                if (VinkiConfig.GraffitiFadeTime.Value == -1 ||
                    placedGraffitis[self.name][i].cyclePlaced + VinkiConfig.GraffitiFadeTime.Value >= thisCycle)
                {
                    // Spawn the graffiti
                    PlacedObject.CustomDecalData decalData = new PlacedObject.CustomDecalData(null);
                    decalData.FromString(placedGraffitis[self.name][i].data);
                    PlacedObject placedObject = new PlacedObject(PlacedObject.Type.CustomDecal, decalData);
                    placedObject.pos = new Vector2(placedGraffitis[self.name][i].x, placedGraffitis[self.name][i].y);
                    self.realizedRoom.AddObject(new CustomDecal(placedObject));
                } 
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

        // Create hologram for any story graffiti
        var storyGraffitisInRoom = Plugin.storyGraffitiRoomPositions.Where(e => e.Value.Key == self.abstractRoom.name);
        var miscWorldSave = SaveDataExtension.GetSlugBaseData(self.game.GetStorySession.saveState.miscWorldSaveData);
        bool storyGraffitisHaveBeenSprayed = miscWorldSave.TryGet("StoryGraffitisSprayed", out bool[] sprayedGNums);
        foreach (var storyGraffiti in storyGraffitisInRoom)
        {
            if ((!storyGraffitisHaveBeenSprayed || !sprayedGNums[storyGraffiti.Key]) && storyGraffiti.Key != 1)
            {
                GraffitiHolder graffitiHolder = new GraffitiHolder(Plugin.graffitis["Story"][storyGraffiti.Key], storyGraffiti.Value, self, storyGraffiti.Key);
                self.AddObject(graffitiHolder);
            }
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
        // Story GraffitiTutorial
        else if (self.abstractRoom?.name == "SS_D08")
        {
            self.AddObject(new StoryGraffitiTutorial(self));
        }
    }
}
