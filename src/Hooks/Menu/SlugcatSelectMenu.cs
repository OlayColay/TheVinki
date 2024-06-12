using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static Vinki.Plugin;

namespace Vinki;

public static partial class Hooks
{
    private static void ApplySlugcatSelectMenuHooks()
    {
        On.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
        On.Menu.SlugcatSelectMenu.CommunicateWithUpcomingProcess += SlugcatSelectMenu_CommunicateWithUpcomingProcess;
    }

    private static void SlugcatSelectMenu_StartGame(On.Menu.SlugcatSelectMenu.orig_StartGame orig, SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter)
    {
        if (storyGameCharacter != Enums.vinki || (!self.restartChecked && self.manager.rainWorld.progression.IsThereASavedGame(storyGameCharacter)))
        {
            orig(self, storyGameCharacter);
            return;
        }

        self.manager.rainWorld.inGameSlugCat = storyGameCharacter;
        bool flag2 = ModManager.MMF && self.manager.rainWorld.progression.miscProgressionData.colorsEnabled.ContainsKey(self.slugcatColorOrder[self.slugcatPageIndex].value) && self.manager.rainWorld.progression.miscProgressionData.colorsEnabled[self.slugcatColorOrder[self.slugcatPageIndex].value];
        if (flag2)
        {
            List<Color> list = new List<Color>();
            for (int i = 0; i < self.manager.rainWorld.progression.miscProgressionData.colorChoices[self.slugcatColorOrder[self.slugcatPageIndex].value].Count; i++)
            {
                Vector3 vector = new Vector3(1f, 1f, 1f);
                bool flag3 = self.manager.rainWorld.progression.miscProgressionData.colorChoices[self.slugcatColorOrder[self.slugcatPageIndex].value][i].Contains(",");
                if (flag3)
                {
                    string[] splitstr = self.manager.rainWorld.progression.miscProgressionData.colorChoices[self.slugcatColorOrder[self.slugcatPageIndex].value][i].Split(new char[] { ',' });
                    vector = new Vector3(float.Parse(splitstr[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(splitstr[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(splitstr[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                }
                list.Add(RWCustom.Custom.HSL2RGB(vector[0], vector[1], vector[2]));
            }
            PlayerGraphics.customColors = list;
        }
        else
        {
            PlayerGraphics.customColors = null;
        }
        self.manager.arenaSitting = null;
        self.manager.rainWorld.progression.currentSaveState = null;
        self.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = storyGameCharacter;
        bool coopAvailable = ModManager.CoopAvailable;
        if (coopAvailable)
        {
            for (int j = 1; j < self.manager.rainWorld.options.JollyPlayerCount; j++)
            {
                self.manager.rainWorld.ActivatePlayer(j);
            }
            for (int k = self.manager.rainWorld.options.JollyPlayerCount; k < 4; k++)
            {
                self.manager.rainWorld.DeactivatePlayer(k);
            }
        }

        self.manager.rainWorld.progression.WipeSaveState(storyGameCharacter);
        self.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
        self.manager.RequestMainProcessSwitch(Enums.FullscreenVideo);
        self.PlaySound(SoundID.MENU_Start_New_Game);
        bool flag8 = self.manager.musicPlayer != null && self.manager.musicPlayer.song != null && self.manager.musicPlayer.song is Music.IntroRollMusic;
		if (flag8)
		{
			self.manager.musicPlayer.song.FadeOut(20f);
		}
    }

    private static void RemoveSlugcatSelectMenuHooks()
    {
        On.Menu.SlugcatSelectMenu.CommunicateWithUpcomingProcess -= SlugcatSelectMenu_CommunicateWithUpcomingProcess;
    }

    private static void SlugcatSelectMenu_CommunicateWithUpcomingProcess(On.Menu.SlugcatSelectMenu.orig_CommunicateWithUpcomingProcess orig, SlugcatSelectMenu self, MainLoopProcess nextProcess)
    {
        orig(self, nextProcess);

        VLogger.LogInfo("CommunicateWithUpcomingProcess");
        if (nextProcess.ID == Enums.FullscreenVideo)
        {
            (nextProcess as FullscreenVideo).StartVideo("videos/VinkiIntro.mp4", ProcessManager.ProcessID.Game);
        }
    }
}
