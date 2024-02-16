using Menu;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;

public class PauseMenuData
{
    public GraffitiSelectDialog graffitiMenu;
    public SimpleButton graffitiMenuButton;
    public bool graffitiMenuMode = false;
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
        On.Menu.PauseMenu.ctor += PauseMenu_ctor;
        On.Menu.PauseMenu.Update += PauseMenu_Update;
        On.Menu.PauseMenu.Singal += PauseMenu_Singal;
    }

    private static void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, PauseMenu self, ProcessManager manager, RainWorldGame game)
    {
        orig(self, manager, game);

        if (self.game.GetStorySession?.saveState.saveStateNumber != Enums.vinki)
        {
            return;
        }

        PauseMenuData data = self.VinkiData();
        data.graffitiMenuButton = new SimpleButton(self, self.pages[0], self.Translate("SELECT GRAFFITI"), "SELECT GRAFFITI", new Vector2(self.ContinueAndExitButtonsXPos - 460f - self.manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(self.manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
        self.pages[0].subObjects.Add(data.graffitiMenuButton);
        data.graffitiMenuButton.black = 0f;
    }

    private static void PauseMenu_Update(On.Menu.PauseMenu.orig_Update orig, Menu.PauseMenu self)
    {
        orig(self);

        if (self.game.GetStorySession?.saveState.saveStateNumber != Enums.vinki)
        {
            return;
        }

        PauseMenuData data = self.VinkiData();
        if (questButton != null)
        {
            data.graffitiMenuButton.buttonBehav.greyedOut = self.continueButton.buttonBehav.greyedOut;
            data.graffitiMenuButton.black = self.continueButton.black;
        }
    }

    private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, PauseMenu self, MenuObject sender, string message)
    {
        Debug.Log("PauseMenu Singal: " + message);
        if (self.game.GetStorySession?.saveState.saveStateNumber != Enums.vinki || message == null)
        {
            orig(self, sender, message);
            return;
        }

        Debug.Log("We are Vinki");
        if (message == "SELECT GRAFFITI")
        {
            GraffitiSelectDialog dialog = new GraffitiSelectDialog(self.manager, self.continueButton.pos, self.game);
            self.manager.ShowDialog(dialog);
            self.PlaySound(SoundID.MENU_Switch_Page_In);
        }
        else
        {
            orig(self, sender, message);
        }
    }
}
