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
    }

    private static void RemoveRainWorldGameHooks()
    {
        On.RainWorldGame.ctor -= RainWorldGame_ctor;
        On.RainWorldGame.GoToRedsGameOver -= RainWorldGame_GoToRedsGameOver;
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
        self.manager.musicPlayer?.FadeOutAllSongs(20f);
        if (self.Players[0].realizedCreature != null && (self.Players[0].realizedCreature as Player).redsIllness != null)
        {
            (self.Players[0].realizedCreature as Player).redsIllness.fadeOutSlow = true;
        }
        if (self.GetStorySession.saveState.saveStateNumber == SlugcatStats.Name.Red)
        {
            self.GetStorySession.saveState.deathPersistentSaveData.redsDeath = true;
            if (ModManager.CoopAvailable)
            {
                int num = 0;
                using (IEnumerator<Player> enumerator = self.Players.Select((AbstractCreature x) => x.realizedCreature as Player).GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Player player = enumerator.Current;
                        self.GetStorySession.saveState.AppendCycleToStatistics(player, self.GetStorySession, true, num);
                        num++;
                    }
                    goto IL_15D;
                }
            }
            self.GetStorySession.saveState.AppendCycleToStatistics(self.Players[0].realizedCreature as Player, self.GetStorySession, true, 0);
        }
    IL_15D:
        self.manager.rainWorld.progression.SaveWorldStateAndProgression(false);

        self.manager.statsAfterCredits = true;
        self.manager.nextSlideshow = Enums.SlideShowID.VinkiAltEnd;
        self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
    }
}
