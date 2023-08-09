using System;

namespace Vinki;

public static partial class Hooks
{
	private static void ApplyJollyCoopHooks()
	{
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite += Vinki_Jolly_Sprite;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += Vinki_Jolly_Name;
    }

    private static bool Vinki_Jolly_Sprite(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_HasUniqueSprite orig, JollyCoop.JollyMenu.SymbolButtonTogglePupButton self)
    {
        if (self.symbolNameOff.Contains("vinki")) return true;
        return orig(self);
    }

    private static string Vinki_Jolly_Name(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GetPupButtonOffName orig, JollyCoop.JollyMenu.JollyPlayerSelector self)
    {
        SlugcatStats.Name playerClass = self.JollyOptions(self.index).playerClass;
        if (playerClass != null && playerClass.value.Equals("TheVinki"))
        {
            return "vinki_pup_off";
        }
        return orig(self);
    }
}
