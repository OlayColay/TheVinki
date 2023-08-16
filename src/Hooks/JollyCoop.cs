using Menu;
using RWCustom;
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
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.Update += Vinki_Jolly_PupUpdate;

        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += Vinki_Jolly_Name;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += Vinki_Jolly_Update;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GrafUpdate += Vinki_Jolly_GrafUpdate;
    }

    private static void Vinki_Jolly_PupUpdate(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_Update orig, JollyCoop.JollyMenu.SymbolButtonTogglePupButton self)
    {
        // TODO: Make pup sprite
        if ((self.symbolNameOff != null && !self.symbolNameOff.Contains("vinki")) || self.isToggled)
        {
            if (rainPodsSymbol != null)
            {
                rainPodsSymbol.RemoveSprites();
                self.subObjects.Remove(rainPodsSymbol);
                rainPodsSymbol = null;
            }
            if (shoesSymbol != null)
            {
                shoesSymbol.RemoveSprites();
                self.subObjects.Remove(shoesSymbol);
                shoesSymbol = null;
            }
            if (glassesSymbol != null)
            {
                glassesSymbol.RemoveSprites();
                self.subObjects.Remove(glassesSymbol);
                glassesSymbol = null;
            }
        }
        else
        {
            string fileName;
            if (rainPodsSymbol == null)
            {
                fileName = "rainpods_" + (self.isToggled ? self.symbolNameOn : self.symbolNameOff);
                rainPodsSymbol = new MenuIllustration(self.menu, self, "", fileName, self.size / 2f, true, true);
                self.subObjects.Add(rainPodsSymbol);
            }
            if (shoesSymbol == null)
            {
                fileName = "shoes_" + (self.isToggled ? self.symbolNameOn : self.symbolNameOff);
                shoesSymbol = new MenuIllustration(self.menu, self, "", fileName, self.size / 2f, true, true);
                self.subObjects.Add(shoesSymbol);
            }
            if (glassesSymbol == null)
            {
                fileName = "glasses_" + (self.isToggled ? self.symbolNameOn : self.symbolNameOff);
                glassesSymbol = new MenuIllustration(self.menu, self, "", fileName, self.size / 2f, true, true);
                self.subObjects.Add(glassesSymbol);
            }
        }
        orig(self);
    }

    private static void Vinki_Jolly_GrafUpdate(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GrafUpdate orig, JollyCoop.JollyMenu.JollyPlayerSelector self, float timeStacker)
    {
        if (self.slugName != Enums.TheVinki)
        {
            orig(self, timeStacker);
            return;
        }

        Color color = self.FadePortraitSprite(GetCustomVinkiColor(self.index, 3), timeStacker);
        Color color2 = self.FadePortraitSprite(GetCustomVinkiColor(self.index, 4), timeStacker);
        Color color3 = self.FadePortraitSprite(GetCustomVinkiColor(self.index, 5), timeStacker);

        rainPodsSymbol.sprite.color = color;
        shoesSymbol.sprite.color = color2;
        glassesSymbol.sprite.color = color3;

        orig(self, timeStacker);
    }

    private static void Vinki_Jolly_Update(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyCoop.JollyMenu.JollyPlayerSelector self)
    {
        orig(self);
        // TODO: Make pup sprite
        SlugcatStats.Name playerClass = self.JollyOptions(self.index)?.playerClass;
        if (playerClass == null || !playerClass.value.Equals("TheVinki") || self.pupButton.isToggled || rainPodsSymbol == null)
        {
            orig(self);
            return;
        }
        
        Color color = GetCustomVinkiColor(self.index, 3);
        Color color2 = GetCustomVinkiColor(self.index, 4);
        Color color3 = GetCustomVinkiColor(self.index, 5);
        //Debug.Log("New shoe color: " + color2.ToString());

        rainPodsSymbol.sprite.color = color;
        shoesSymbol.sprite.color = color2;
        glassesSymbol.sprite.color = color3;

        self.pupButton.LoadIcon();
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
        // TODO: Make pup sprite
        if (self.symbolNameOff.Contains("vinki") && !self.isToggled) return true;
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
        rainPodsSymbol.sprite.color = GetCustomVinkiColor((self.owner as JollyCoop.JollyMenu.JollyPlayerSelector).index, 3);
        rainPodsSymbol.color = GetCustomVinkiColor((self.owner as JollyCoop.JollyMenu.JollyPlayerSelector).index, 3);
        rainPodsSymbol.sprite.SetElementByName(rainPodsSymbol.fileName);
        // Shoes
        shoesSymbol.fileName = "shoes_" + self.symbol.fileName;
        shoesSymbol.LoadFile();
        shoesSymbol.sprite.color = GetCustomVinkiColor((self.owner as JollyCoop.JollyMenu.JollyPlayerSelector).index, 4);
        shoesSymbol.color = GetCustomVinkiColor((self.owner as JollyCoop.JollyMenu.JollyPlayerSelector).index, 4);
        shoesSymbol.sprite.SetElementByName(shoesSymbol.fileName);
        // Glasses
        glassesSymbol.fileName = "glasses_" + self.symbol.fileName;
        glassesSymbol.LoadFile();
        glassesSymbol.pos = new Vector2(UnityEngine.Random.Range(0, 1000), UnityEngine.Random.Range(0, 500));
        glassesSymbol.sprite.color = GetCustomVinkiColor((self.owner as JollyCoop.JollyMenu.JollyPlayerSelector).index, 5);
        glassesSymbol.color = GetCustomVinkiColor((self.owner as JollyCoop.JollyMenu.JollyPlayerSelector).index, 5);
        glassesSymbol.sprite.SetElementByName(glassesSymbol.fileName);
    }

    private static Color GetCustomVinkiColor(int playerNumber, int bodyPartIndex)
    {
        if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.CUSTOM)
        {
            switch (bodyPartIndex)
            {
                case 3: return Color.white;
                case 4: return new Color(0.28627450980392155f, 0.3058823529411765f, 0.8274509803921568f);
                case 5: return new Color(0.054901960784313725f, 0.00784313725490196f, 0.00784313725490196f);
                default:
                    Debug.LogError("Invalid bodyPartIndex!\n" + StackTraceUtility.ExtractStackTrace());
                    return Color.white;
            }
        }
        Debug.Log("Checking custom color for player " + playerNumber + ": " + Plugin.jollyColors[playerNumber][bodyPartIndex].GetValueOrDefault().ToString());
        return Plugin.jollyColors[playerNumber][bodyPartIndex].GetValueOrDefault().CloneWithNewAlpha(1f);
    }
}
