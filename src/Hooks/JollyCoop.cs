using Menu;
using System;
using UnityEngine;

namespace Vinki;

public static partial class Hooks
{
    private static MenuIllustration rainPodsSymbol;
    private static MenuIllustration shoesSymbol;
    private static MenuIllustration glassesSymbol;
    private static void ApplyJollyCoopHooks()
	{
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.ctor += Vinki_Jolly_ctor;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite += Vinki_Jolly_Sprite;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += Vinki_Jolly_Name;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.LoadIcon += Vinki_Jolly_LoadIcon;
    }

    private static void Vinki_Jolly_ctor(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_ctor orig, JollyCoop.JollyMenu.SymbolButtonTogglePupButton self, Menu.Menu menu, MenuObject owner, string signal, Vector2 pos, Vector2 size, string symbolNameOn, string symbolNameOff, bool isOn, string stringLabelOn, string stringLabelOff)
    {
        orig(self, menu, owner, signal, pos, size, symbolNameOn, symbolNameOff, isOn, stringLabelOn, stringLabelOff);

        if (!symbolNameOff.Contains("vinki"))
        {
            return;
        }

        // RainPods
        string fileName = "rainpods_" + (isOn ? symbolNameOn : symbolNameOff);
        rainPodsSymbol = new MenuIllustration(menu, self, "", fileName, size / 2f, true, true);
        self.subObjects.Add(rainPodsSymbol);
        // Shoes
        fileName = "shoes_" + (isOn ? symbolNameOn : symbolNameOff);
        shoesSymbol = new MenuIllustration(menu, self, "", fileName, size / 2f, true, true);
        self.subObjects.Add(shoesSymbol);
        // Glasses
        fileName = "glasses_" + (isOn ? symbolNameOn : symbolNameOff);
        glassesSymbol = new MenuIllustration(menu, self, "", fileName, size / 2f, true, true);
        self.subObjects.Add(glassesSymbol);

        self.LoadIcon();
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

    private static void Vinki_Jolly_LoadIcon(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_LoadIcon orig, JollyCoop.JollyMenu.SymbolButtonTogglePupButton self)
    {
        orig(self);

        if (self.symbol == null || !self.symbol.fileName.Contains("vinki") || self.symbol.fileName.Contains("on") ||
            rainPodsSymbol == null)
        {
            return;
        }

        // RainPods
        rainPodsSymbol.fileName = "rainpods_" + self.symbol.fileName;
        rainPodsSymbol.LoadFile();
        rainPodsSymbol.sprite.SetElementByName(rainPodsSymbol.fileName);
        // Shoes
        shoesSymbol.fileName = "shoes_" + self.symbol.fileName;
        shoesSymbol.LoadFile();
        shoesSymbol.sprite.SetElementByName(shoesSymbol.fileName);
        // Glasses
        glassesSymbol.fileName = "glasses_" + self.symbol.fileName;
        glassesSymbol.LoadFile();
        glassesSymbol.sprite.SetElementByName(glassesSymbol.fileName);
    }
}
