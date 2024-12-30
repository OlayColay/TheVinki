using Menu;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;

public class PauseMenuData
{
    public GraffitiSelectDialog graffitiMenu;
    public SimpleButton graffitiMenuButton;
}
public static class PauseMenuExtension
{
    private static readonly ConditionalWeakTable<PauseMenu, PauseMenuData> cwt = new();

    public static PauseMenuData VinkiData(this PauseMenu gm) => cwt.GetValue(gm, _ => new PauseMenuData());
}

public static partial class Hooks
{
    // Add hooks
    private static void ApplyPauseMenuHooks()
    {
        On.Menu.PauseMenu.SpawnExitContinueButtons += PauseMenu_SpawnExitContinueButtons;
        On.Menu.PauseMenu.SpawnConfirmButtons += PauseMenu_SpawnConfirmButtons;
        On.Menu.PauseMenu.Update += PauseMenu_Update;
        On.Menu.PauseMenu.Singal += PauseMenu_Singal;
    }

    private static void RemovePauseMenuHooks()
    {
        On.Menu.PauseMenu.SpawnExitContinueButtons -= PauseMenu_SpawnExitContinueButtons;
        On.Menu.PauseMenu.Update -= PauseMenu_Update;
        On.Menu.PauseMenu.Singal -= PauseMenu_Singal;
    }

    private static void PauseMenu_SpawnExitContinueButtons(On.Menu.PauseMenu.orig_SpawnExitContinueButtons orig, PauseMenu self)
    {
        orig(self);

        //if (self.game.GetStorySession?.saveState.saveStateNumber != Enums.vinki)
        if (self.game.IsArenaSession)
        {
            return;
        }

        bool needToMoveButtonUp = ModManager.ActiveMods.Exists(mod => mod.id == "ved_s.restartbutton" || mod.id == "henpemaz_rainmeadow") &&
            self.pages[0].subObjects.Any(x => x is SimpleButton b && b.menuLabel.text == self.Translate("RESTART"));

        PauseMenuData data = self.VinkiData();
        data.graffitiMenuButton = new SimpleButton(self, self.pages[0], self.Translate("SELECT GRAFFITI"), "SELECT GRAFFITI", new Vector2(self.ContinueAndExitButtonsXPos - 460f - self.manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(self.manager.rainWorld.options.SafeScreenOffset.y, 15f + (needToMoveButtonUp ? 38f : 0f))), new Vector2(110f, 30f));
        self.pages[0].subObjects.Add(data.graffitiMenuButton);
        data.graffitiMenuButton.black = 0f;
    }

    private static void PauseMenu_SpawnConfirmButtons(On.Menu.PauseMenu.orig_SpawnConfirmButtons orig, PauseMenu self)
    {
        orig(self);

        PauseMenuData data = self.VinkiData();
        if (data.graffitiMenuButton != null)
        {
            data.graffitiMenuButton.RemoveSprites();
            self.pages[0].RemoveSubObject(data.graffitiMenuButton);
        }
        data.graffitiMenuButton = null;
    }

    private static void PauseMenu_Update(On.Menu.PauseMenu.orig_Update orig, PauseMenu self)
    {
        orig(self);

        //if (self.game.GetStorySession?.saveState.saveStateNumber != Enums.vinki)
        if (self.game.IsArenaSession)
        {
            return;
        }

        PauseMenuData data = self.VinkiData();
        if (questButton != null && self.continueButton != null && self.continueButton.buttonBehav != null)
        {
            data.graffitiMenuButton.buttonBehav.greyedOut = self.continueButton.buttonBehav.greyedOut;
            data.graffitiMenuButton.black = self.continueButton.black;
        }

        if (self.wantToContinue && self.manager.sideProcesses.Contains(data.graffitiMenu))
        {
            data.graffitiMenu.Singal(self.pages[0], "CLOSE MUTED");
        }
    }

    private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, PauseMenu self, MenuObject sender, string message)
    {
        //if (self.game.GetStorySession?.saveState.saveStateNumber != Enums.vinki || message == null)
        if (self.game.IsArenaSession)
        {
            orig(self, sender, message);
            return;
        }

        if (message == "SELECT GRAFFITI")
        {
            PauseMenuData data = self.VinkiData();
            data.graffitiMenu = new GraffitiSelectDialog(self.manager, self.continueButton.pos, self.game);
            self.manager.ShowDialog(data.graffitiMenu);
            self.PlaySound(SoundID.MENU_Switch_Page_In);
        }
        else
        {
            orig(self, sender, message);
        }
    }
}
