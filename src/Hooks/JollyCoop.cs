using Menu;
using RWCustom;
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
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.LoadIcon += Vinki_Jolly_LoadIcon;

        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += Vinki_Jolly_Name;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += Vinki_Jolly_Update;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GrafUpdate += Vinki_Jolly_GrafUpdate;
    }

    private static void Vinki_Jolly_GrafUpdate(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GrafUpdate orig, JollyCoop.JollyMenu.JollyPlayerSelector self, float timeStacker)
    {
        if (self.slugName != Enums.TheVinki)
        {
            orig(self, timeStacker);
            return;
        }

        Color color = self.FadePortraitSprite(Color.white, timeStacker);
        Color color2 = self.FadePortraitSprite(new Color(0.28627450980392155f, 0.3058823529411765f, 0.8274509803921568f), timeStacker);
        Color color3 = self.FadePortraitSprite(new Color(0.054901960784313725f, 0.00784313725490196f, 0.00784313725490196f), timeStacker);

        rainPodsSymbol.sprite.color = color;
        shoesSymbol.sprite.color = color2;
        glassesSymbol.sprite.color = color3;

        orig(self, timeStacker);
    }

    private static void Vinki_Jolly_Update(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyCoop.JollyMenu.JollyPlayerSelector self)
    {
        orig(self);

        if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.DEFAULT || (self.index == 0 && Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)) && self.slugName == Enums.TheVinki)
        {
            rainPodsSymbol.sprite.color = Color.white;
            shoesSymbol.sprite.color = new Color(0.28627450980392155f, 0.3058823529411765f, 0.8274509803921568f);
            glassesSymbol.sprite.color = new Color(0.054901960784313725f, 0.00784313725490196f, 0.00784313725490196f);
        }
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
