using MoreSlugcats;
using SlugBase.SaveData;
using System.Collections.Generic;
using System.Linq;

namespace Vinki;

public static partial class Hooks
{
	private static void ApplyRainWorldGameHooks()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.GoToRedsGameOver += RainWorldGame_GoToRedsGameOver;
        On.RainWorldGame.BeatGameMode += RainWorldGame_BeatGameMode;
    }

    private static void RemoveRainWorldGameHooks()
    {
        On.RainWorldGame.ctor -= RainWorldGame_ctor;
        On.RainWorldGame.GoToRedsGameOver -= RainWorldGame_GoToRedsGameOver;
        On.RainWorldGame.BeatGameMode -= RainWorldGame_BeatGameMode;
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (Plugin.restartMode)
        {
            Enums.RegisterValues();
            ApplyHooks();

            LoadResources(self.rainWorld);
            RainWorld_PostModsInit((_) => { }, self.rainWorld);
        }

        if (self.IsStorySession && self.GetStorySession.saveStateNumber == Enums.vinki)
        {
            SlugBaseSaveData miscWorldSave = SaveDataExtension.GetSlugBaseData(self.GetStorySession.saveState.miscWorldSaveData);
            miscWorldSave.Set("AutoOpenMap", false);

            // Save story graffiti on the map before cycle ends
            if (Plugin.storyGraffitisOnMap.Length > 0)
            {
                miscWorldSave.Set("StoryGraffitisOnMap", Plugin.storyGraffitisOnMap);
            }

            // Decrement blueCycles from Moon easter egg
            if (Plugin.blueCycles > 0)
            {
                Plugin.blueCycles--;
            }
        }
    }

    // OE Endings
    private static void RainWorldGame_GoToRedsGameOver(On.RainWorldGame.orig_GoToRedsGameOver orig, RainWorldGame self)
    {
        if (self.GetStorySession.saveState.saveStateNumber != Enums.vinki)
        {
            orig(self);
            return;
        }

        if (self.manager.upcomingProcess != null)
        {
            return;
        }

        SlugBaseSaveData miscWorldSave = SaveDataExtension.GetSlugBaseData(self.GetStorySession.saveState.miscWorldSaveData);
        SlugBaseSaveData miscProgressionSave = SaveDataExtension.GetSlugBaseData(self.rainWorld.progression.miscProgressionData);
        if (Plugin.FirstStoryGraffitisDone(miscWorldSave))
        {
            miscProgressionSave.Set("VinkiEndingID", Enums.EndingID.QuestCompleteOE);
            self.manager.nextSlideshow = Enums.SlideShowID.VinkiOEEnd;
        }
        else
        {
            miscProgressionSave.Set("VinkiEndingID", Enums.EndingID.QuestIncompleteOE);
            self.manager.nextSlideshow = Enums.SlideShowID.VinkiOEEnd;
        }

        self.manager.musicPlayer?.FadeOutAllSongs(20f);
        self.manager.rainWorld.progression.SaveWorldStateAndProgression(false);

        self.manager.statsAfterCredits = true;
        self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
    }

    private static void RainWorldGame_BeatGameMode(On.RainWorldGame.orig_BeatGameMode orig, RainWorldGame game, bool standardVoidSea)
    {
        if (standardVoidSea || game.StoryCharacter != Enums.vinki)
        {
            orig(game, standardVoidSea);
            return;
        }

        string newSpawnRoom = "";
        SlugBaseSaveData miscProgressionSave = SaveDataExtension.GetSlugBaseData(game.rainWorld.progression.miscProgressionData);
        if (miscProgressionSave.TryGet("VinkiEndingID", out int vinkiEndingID))
        {
            switch (vinkiEndingID)
            {
                case Enums.EndingID.QuestIncompleteOE:
                case Enums.EndingID.QuestCompleteOE:
                    newSpawnRoom = "OE_SEXTRA";
                    break;
            }
            game.GetStorySession.saveState.deathPersistentSaveData.altEnding = true;
            game.GetStorySession.saveState.deathPersistentSaveData.ascended = false;
            game.GetStorySession.saveState.deathPersistentSaveData.karma = game.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
        }

        if (newSpawnRoom != "")
        {
            AbstractCreature abstractCreature2 = game.FirstAlivePlayer;
            abstractCreature2 ??= game.FirstAnyPlayer;
            SaveState.forcedEndRoomToAllowwSave = abstractCreature2.Room.name;
            game.GetStorySession.saveState.BringUpToDate(game);
            SaveState.forcedEndRoomToAllowwSave = "";
        }
        game.AppendCycleToStatisticsForPlayers();
        if (newSpawnRoom == "")
        {
            game.GetStorySession.saveState.progression.SaveWorldStateAndProgression(false);
            return;
        }
        RainWorldGame.ForceSaveNewDenLocation(game, newSpawnRoom, false);
    }
}
