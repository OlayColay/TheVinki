using SlugBase.SaveData;
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
        if (game.GetStorySession == null)
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
            // Remove graffiti that are in Moon's chamber or the Five Pebbles region
            if (!self.name.StartsWith("SS_") && self.name != "DM_AI")
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
        if (self.game == null || self.game.GetStorySession == null || self.game.GetStorySession.saveState == null || self.game.GetStorySession.saveState.saveStateNumber != Enums.vinki)
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
        bool storyGraffitisHaveBeenSprayed = miscWorldSave.TryGet("StoryGraffitisSprayed", out int[] sprayedGNums);

        // Disable holograms for story graffiti tutorial room after they've been sprayed and erased by 5P already
        Debug.Log("Checking if holograms are enabled in " + self.abstractRoom.name);
        if (HologramsEnabledInRoom(self, miscWorldSave))
        {
            Debug.Log(storyGraffitisInRoom.Count() + " holograms are enabled");
            foreach (var storyGraffiti in storyGraffitisInRoom)
            {
                Debug.Log("Hologram: " + storyGraffiti.Key);
                if ((!storyGraffitisHaveBeenSprayed || !sprayedGNums.Contains(storyGraffiti.Key)) && storyGraffiti.Key != 1)
                {
                    GraffitiHolder graffitiHolder = new GraffitiHolder(Plugin.graffitis["Story"][storyGraffiti.Key], storyGraffiti.Value, self, storyGraffiti.Key);
                    self.AddObject(graffitiHolder);
                }
            }
        }
    }

    public static bool HologramsEnabledInRoom(Room self, SlugBaseSaveData miscWorldSave)
    {
        //Debug.Log("HologramsEnabledInRoom: " + self.abstractRoom.name + " " + (!miscWorldSave.TryGet("SpawnUnlockablePearl", out int k) ? k : "null") + " " + Hooks.AllGraffitiUnlocked());
        return (self.abstractRoom?.name != "SS_D08" || !miscWorldSave.TryGet("StoryGraffitiTutorialPhase", out int i) || i < (int)StoryGraffitiTutorial.Phase.End) &&
            (self.abstractRoom?.name != "DM_AI" || ((!miscWorldSave.TryGet("SpawnUnlockablePearl", out int j) || j < 1) && Hooks.AllGraffitiUnlocked()));
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room self)
    {
        orig(self);

        if (!self.game.IsStorySession || self.game.GetStorySession.saveState.saveStateNumber != Enums.vinki)
        {
            return;
        }

        //if (!self.abstractRoom.firstTimeRealized)
        //{
        //    return;
        //}

        SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(self.game.rainWorld.progression.currentSaveState.miscWorldSaveData);
        // Graffiti Tutorial
        if (self.abstractRoom?.name == "SS_E08" && self.game.rainWorld.progression.currentSaveState.cycleNumber == 0)
        {
            self.AddObject(new GraffitiTutorial(self));
        }
        // Story Graffiti Tutorial
        else if (self.abstractRoom?.name == "SS_D08" && (!miscSave.TryGet("StoryGraffitiTutorialPhase", out int i) || i < (int)StoryGraffitiTutorial.Phase.End))
        {
            self.AddObject(new StoryGraffitiTutorial(self));
        }
        // Grinding Tutorial
        else if ((self.abstractRoom?.name == "UW_H01" || self.abstractRoom?.name == "UW_H01VI") && (!miscSave.TryGet("GrindTutorialCompleted", out bool b) || !b))
        {
            self.AddObject(new GrindTutorial(self));
        }
        // Spawn pearl and disable hologram
        else if (self.abstractRoom?.name == "DM_AI")
        {
            int phase;
            if (miscSave.TryGet("SpawnUnlockablePearl", out phase) && phase == 1)
            {
                var abstr = new DataPearl.AbstractDataPearl(self.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, new WorldCoordinate(self.abstractRoom.index, 250, 250, 0), self.game.GetNewID(),
                    -1, -1, null, new DataPearl.AbstractDataPearl.DataPearlType("Vinki_Pearl_1", true));
                abstr.Realize();
                self.abstractRoom.AddEntity(abstr);
                self.AddObject(abstr.realizedObject);
            }
        }
    }
}
