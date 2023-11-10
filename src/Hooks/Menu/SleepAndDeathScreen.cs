using Menu;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
	private static void ApplySleepAndDeathScreenHooks()
	{
		On.Menu.SleepAndDeathScreen.ctor += SleepAndDeathScreen_ctor;
        On.Menu.SleepAndDeathScreen.Update += SleepAndDeathScreen_Update;
        On.Menu.SleepAndDeathScreen.Singal += SleepAndDeathScreen_Singal;
        On.Menu.SleepAndDeathScreen.CommunicateWithUpcomingProcess += SleepAndDeathScreen_CommunicateWithUpcomingProcess;
        On.Menu.SleepAndDeathScreen.UpdateInfoText += SleepAndDeathScreen_UpdateInfoText;
	}

    private static SimpleButton questButton;
    private static void SleepAndDeathScreen_ctor(On.Menu.SleepAndDeathScreen.orig_ctor orig, SleepAndDeathScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
    {
        orig(self, manager, ID);

        if (manager.slugcatLeaving != Enums.vinki)
        {
            return;
        }

        questButton = new SimpleButton(self, self.pages[0], self.Translate("QUEST MAP"), "QUEST MAP", new Vector2(self.ContinueAndExitButtonsXPos - 460f - self.manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(self.manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
        self.pages[0].subObjects.Add(questButton);
        questButton.black = (false ? 1f : 0f);
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
            GraffitiDialog dialog = new GraffitiDialog(self.manager, self.continueButton.pos);
            self.manager.ShowDialog(dialog);
            self.PlaySound(SoundID.MENU_Switch_Page_In);
        }
        else
        {
            orig(self, sender, message);
        }
    }

    private static void SleepAndDeathScreen_CommunicateWithUpcomingProcess(On.Menu.SleepAndDeathScreen.orig_CommunicateWithUpcomingProcess orig, SleepAndDeathScreen self, MainLoopProcess nextProcess)
    {
        //if (nextProcess is GraffitiDreamScreen)
        //{
        //    (nextProcess as GraffitiDialog).GetDataFromGame(null);
        //}
        orig(self, nextProcess);
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
