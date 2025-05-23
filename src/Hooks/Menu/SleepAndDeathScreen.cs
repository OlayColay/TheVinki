﻿using Menu;
using SlugBase.SaveData;
using System;
using System.Linq;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
	private static void ApplySleepAndDeathScreenHooks()
	{
        On.Menu.SleepAndDeathScreen.AddSubObjects += SleepAndDeathScreen_AddSubObjects;
        On.Menu.SleepAndDeathScreen.Update += SleepAndDeathScreen_Update;
        On.Menu.SleepAndDeathScreen.Singal += SleepAndDeathScreen_Singal;
        On.Menu.SleepAndDeathScreen.UpdateInfoText += SleepAndDeathScreen_UpdateInfoText;
    }
    private static void RemoveSleepAndDeathScreenHooks()
    {
        On.Menu.SleepAndDeathScreen.AddSubObjects -= SleepAndDeathScreen_AddSubObjects;
        On.Menu.SleepAndDeathScreen.Update -= SleepAndDeathScreen_Update;
        On.Menu.SleepAndDeathScreen.Singal -= SleepAndDeathScreen_Singal;
        On.Menu.SleepAndDeathScreen.UpdateInfoText -= SleepAndDeathScreen_UpdateInfoText;
    }

    private static SimpleButton questButton;
    private static bool firstSleepUpdate;
    private static void SleepAndDeathScreen_AddSubObjects(On.Menu.SleepAndDeathScreen.orig_AddSubObjects orig, SleepAndDeathScreen self)
    {
        if (self.manager.slugcatLeaving != Enums.vinki)
        {
            orig(self);
            return;
        }

        questButton = new SimpleButton(self, self.pages[0], self.Translate("QUEST MAP"), "QUEST MAP", new Vector2(self.ContinueAndExitButtonsXPos - 460f - self.manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(self.manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
        self.pages[0].subObjects.Add(questButton);
        questButton.black = 0f;
        firstSleepUpdate = true;

        // Remove sprayed graffiti from being put on the map if we died
        Plugin.diedLastCycle = self.IsAnyDeath;

        orig(self);
    }

    private static void SleepAndDeathScreen_Update(On.Menu.SleepAndDeathScreen.orig_Update orig, SleepAndDeathScreen self)
    {
        orig(self);

        if (self.myGamePackage.saveState.saveStateNumber != Enums.vinki)
        {
            return;
        }

        if (questButton != null)
        {
            questButton.buttonBehav.greyedOut = self.ButtonsGreyedOut;
            questButton.black = Mathf.Max(0f, questButton.black - 0.025f);
        }

        // Open quest map if new graffiti has been sprayed
        SlugBaseSaveData miscWorldSave = SaveDataExtension.GetSlugBaseData(self.myGamePackage.saveState.miscWorldSaveData);
        if (firstSleepUpdate && !self.ButtonsGreyedOut && !self.IsAnyDeath && miscWorldSave.TryGet("StoryGraffitisSprayed", out int[] _) && 
            miscWorldSave.TryGet("AutoOpenMap", out bool autoMap) && autoMap && VinkiConfig.AutoOpenMap.Value)
        {
            self.Singal(self.pages[0], "QUEST MAP");
            firstSleepUpdate = false;
            miscWorldSave.Set("AutoOpenMap", false);
        }
    }

    private static void SleepAndDeathScreen_Singal(On.Menu.SleepAndDeathScreen.orig_Singal orig, SleepAndDeathScreen self, MenuObject sender, string message)
    {
        if(self.myGamePackage.saveState.saveStateNumber != Enums.vinki || message == null)
        {
            orig(self, sender, message);
            return;
        }

        if (message == "QUEST MAP")
        {
            GraffitiQuestDialog.removeCloud = Plugin.FirstStoryGraffitisDone(SaveDataExtension.GetSlugBaseData(self.myGamePackage.saveState.miscWorldSaveData)) ? (int)GraffitiQuestDialog.slapLength : 0;
            GraffitiQuestDialog dialog = new(self.manager, self.continueButton.pos);
            self.manager.ShowDialog(dialog);
            self.PlaySound(SoundID.MENU_Switch_Page_In);
        }
        else
        {
            orig(self, sender, message);
        }
    }

    private static string SleepAndDeathScreen_UpdateInfoText(On.Menu.SleepAndDeathScreen.orig_UpdateInfoText orig, SleepAndDeathScreen self)
    {
        if (self.selectedObject is SimpleButton)
        {
            SimpleButton simpleButton = self.selectedObject as SimpleButton;
            if (simpleButton.signalText == "QUEST MAP")
            {
                return self.Translate("View graffiti quest objectives and progress");
            }
        }
        return orig(self);
    }
}
