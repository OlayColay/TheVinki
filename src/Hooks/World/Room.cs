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

        On.Room.ctor += Room_ctor;
        On.Room.Loaded += Room_Loaded;

        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
    }
    private static void RemoveRoomHooks()
    {
        On.AbstractRoom.RealizeRoom -= AbstractRoom_RealizeRoom;

        On.Room.ctor -= Room_ctor;
        On.Room.Loaded -= Room_Loaded;

        On.RoomSpecificScript.AddRoomSpecificScript -= RoomSpecificScript_AddRoomSpecificScript;
    }

    private static void AbstractRoom_RealizeRoom(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
    {
        orig(self, world, game);

        if (game.GetStorySession == null)
        {
            return;
        }

        SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(world.game.GetStorySession.saveState.miscWorldSaveData);
        if (!miscSave.TryGet("PlacedGraffitis", out Dictionary<string, List<GraffitiObject.SerializableGraffiti>> placedGraffitis) || !placedGraffitis.ContainsKey(self.name))
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
                    PlacedObject.CustomDecalData decalData = new(null);
                    decalData.FromString(placedGraffitis[self.name][i].data);

                    PlacedObject placedObject = new(PlacedObject.Type.CustomDecal, decalData)
                    {
                        pos = new Vector2(placedGraffitis[self.name][i].x, placedGraffitis[self.name][i].y)
                    };
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

    private static void Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom)
    {
        orig(self, game, world, abstractRoom);

        if (game == null || game.StoryCharacter != Enums.vinki)
        {
            return;
        }

        self.roomSettings.placedObjects.RemoveAll((obj) => obj.type == PlacedObject.Type.Corruption
        || obj.type == PlacedObject.Type.CorruptionDarkness
        || obj.type == PlacedObject.Type.CorruptionTube);
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
        sprayedGNums ??= [];

        // Disable holograms for story graffiti tutorial room after they've been sprayed and erased by 5P already
        if (HologramsEnabledInRoom(self, miscWorldSave))
        {
            foreach (var storyGraffiti in storyGraffitisInRoom)
            {
                if ((!storyGraffitisHaveBeenSprayed || !sprayedGNums.Contains(storyGraffiti.Key)) && storyGraffiti.Key != 1)
                {
                    GraffitiHolder graffitiHolder = new(Plugin.graffitis["Story"][storyGraffiti.Key], storyGraffiti.Value, self, storyGraffiti.Key);
                    self.AddObject(graffitiHolder);
                }
            }
        }
    }

    public static bool HologramsEnabledInRoom(Room self, SlugBaseSaveData miscWorldSave)
    {
        //VLogger.LogInfo("HologramsEnabledInRoom: " + self.abstractRoom.name + " " + (!miscWorldSave.TryGet("SpawnUnlockablePearl", out int k) ? k : "null") + " " + Hooks.AllGraffitiUnlocked());
        bool notFPRepeat = self.abstractRoom?.name != "SS_AI" || self.game.GetStorySession.saveState.cycleNumber == 0;
        bool notTutorialGrafRepeat = self.abstractRoom?.name != "SS_D08" || !miscWorldSave.TryGet("StoryGraffitiTutorialPhase", out int i) || i < (int)StoryGraffitiTutorial.Phase.Explore;
        bool notFinalUnlockRepeat = self.abstractRoom?.name != "DM_AI" || ((!miscWorldSave.TryGet("SpawnUnlockablePearl", out int j) || j < 1) && AllGraffitiUnlocked());
        return notFPRepeat && notTutorialGrafRepeat && notFinalUnlockRepeat;
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

        string name = self.abstractRoom?.name;

        SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(self.game.rainWorld.progression.currentSaveState.miscWorldSaveData);
        // Graffiti Tutorial
        if (name == "SS_E08" && !VinkiConfig.SkipIntro.Value && self.game.rainWorld.progression.currentSaveState.cycleNumber == 0)
        {
            self.AddObject(new GraffitiTutorial(self));
        }
        // Story Graffiti Tutorial
        else if (name == "SS_D08" && !VinkiConfig.SkipIntro.Value && (!miscSave.TryGet("StoryGraffitiTutorialPhase", out int i) || i < (int)StoryGraffitiTutorial.Phase.End))
        {
            self.AddObject(new StoryGraffitiTutorial(self));
        }
        // Grinding Tutorial
        else if ((name == "UW_H01" || name == "UW_H01VI") && !VinkiConfig.SkipIntro.Value && (!miscSave.TryGet("GrindTutorialCompleted", out bool b) || !b))
        {
            self.AddObject(new GrindTutorial(self));
        }
        // Spawn pearl and disable hologram
        else if (name == "DM_AI")
        {
            if (miscSave.TryGet("SpawnUnlockablePearl", out int phase) && phase == 1)
            {
                var abstr = new DataPearl.AbstractDataPearl(self.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, new WorldCoordinate(self.abstractRoom.index, 250, 250, 0), self.game.GetNewID(),
                    -1, -1, null, new DataPearl.AbstractDataPearl.DataPearlType("Vinki_Pearl_1", true));
                abstr.Realize();
                self.abstractRoom.AddEntity(abstr);
                self.AddObject(abstr.realizedObject);
            }
        }
        // Spawn drone and cutscene
        else if (name == "CC_B01" && !self.game.GetStorySession.saveState.hasRobo && Plugin.FirstStoryGraffitisDone(miscSave))
        {
            self.AddObject(new CutsceneVinkiRobo(self));
        }
        // Set spawn position if intro and tutorials are skipped
        else if (name == "UW_F01" && VinkiConfig.SkipIntro.Value && self.abstractRoom.firstTimeRealized && self.game.GetStorySession.saveState.cycleNumber == 0)
        {
            self.AddObject(new SkipIntroSpawn(self));
        }
        // Close GATE_SS_UW on first cycle to imply Vinki just travelled through it
        else if (name == "GATE_SS_UW" && self.abstractRoom.firstTimeRealized && (self.game.GetStorySession.saveState.cycleNumber == 0 ||
            (self.game.GetStorySession.saveState.cycleNumber == 1 && VinkiConfig.SkipIntro.Value)))
        {
            self.AddObject(new CloseGateSsUw(self));
        }
    }
}
