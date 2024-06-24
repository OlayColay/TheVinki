using JollyCoop.JollyMenu;
using Menu;
using RWCustom;
using UnityEngine;

namespace Vinki;

public static partial class Hooks
{
    private static readonly MenuIllustration[] rainPodsSymbol = new MenuIllustration[16];
    private static readonly MenuIllustration[] shoesSymbol = new MenuIllustration[16];
    private static readonly MenuIllustration[] glassesSymbol = new MenuIllustration[16];

    private static void ApplyJollyCoopHooks()
    {
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.ctor += Vinki_Jolly_ctor;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite += Vinki_Jolly_Sprite;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.LoadIcon += Vinki_Jolly_LoadIcon;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.Update += Vinki_Jolly_PupUpdate;

        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += Vinki_Jolly_Name;
        //On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += Vinki_Jolly_Update;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GrafUpdate += Vinki_Jolly_GrafUpdate;
    }
    private static void RemoveJollyCoopHooks()
    {
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.ctor -= Vinki_Jolly_ctor;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite -= Vinki_Jolly_Sprite;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.LoadIcon -= Vinki_Jolly_LoadIcon;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.Update -= Vinki_Jolly_PupUpdate;

        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName -= Vinki_Jolly_Name;
        //On.JollyCoop.JollyMenu.JollyPlayerSelector.Update -= Vinki_Jolly_Update;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GrafUpdate -= Vinki_Jolly_GrafUpdate;
    }

    private static void Vinki_Jolly_PupUpdate(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_Update orig, SymbolButtonTogglePupButton self)
    {
        if (self.menu is not JollySetupDialog)
        {
            orig(self);
            return;
        }

        int index = GetPlayerIndex(self);

        // TODO: Make pup sprite
        if ((self.symbolNameOff != null && !self.symbolNameOff.Contains(Enums.vinkiStr)) || self.isToggled)
        {
            if (rainPodsSymbol[index] != null)
            {
                rainPodsSymbol[index].RemoveSprites();
                self.subObjects.Remove(rainPodsSymbol[index]);
                rainPodsSymbol[index] = null;
            }
            if (shoesSymbol[index] != null)
            {
                shoesSymbol[index].RemoveSprites();
                self.subObjects.Remove(shoesSymbol[index]);
                shoesSymbol[index] = null;
            }
            if (glassesSymbol[index] != null)
            {
                glassesSymbol[index].RemoveSprites();
                self.subObjects.Remove(glassesSymbol[index]);
                glassesSymbol[index] = null;
            }
        }
        else
        {
            string fileName;
            if (rainPodsSymbol[index] == null)
            {
                fileName = "rainpods_" + (self.isToggled ? self.symbolNameOn : self.symbolNameOff);
                rainPodsSymbol[index] = new MenuIllustration(self.menu, self, "", fileName, self.size / 2f, true, true);
                self.subObjects.Add(rainPodsSymbol[index]);
            }
            if (shoesSymbol[index] == null)
            {
                fileName = "shoes_" + (self.isToggled ? self.symbolNameOn : self.symbolNameOff);
                shoesSymbol[index] = new MenuIllustration(self.menu, self, "", fileName, self.size / 2f, true, true);
                self.subObjects.Add(shoesSymbol[index]);
            }
            if (glassesSymbol[index] == null)
            {
                fileName = "glasses_" + (self.isToggled ? self.symbolNameOn : self.symbolNameOff);
                glassesSymbol[index] = new MenuIllustration(self.menu, self, "", fileName, self.size / 2f, true, true);
                self.subObjects.Add(glassesSymbol[index]);
            }
        }
        orig(self);
    }

    private static void Vinki_Jolly_GrafUpdate(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GrafUpdate orig, JollyPlayerSelector self, float timeStacker)
    {
        if (self.slugName != Enums.vinki || rainPodsSymbol[self.index] == null)
        {
            orig(self, timeStacker);
            return;
        }

        Color color = self.FadePortraitSprite(GetCustomVinkiColor(self.index, 3), timeStacker);
        Color color2 = self.FadePortraitSprite(GetCustomVinkiColor(self.index, 4), timeStacker);
        Color color3 = self.FadePortraitSprite(GetCustomVinkiColor(self.index, 5), timeStacker);

        rainPodsSymbol[self.index].sprite.color = color;
        shoesSymbol[self.index].sprite.color = color2;
        glassesSymbol[self.index].sprite.color = color3;

        orig(self, timeStacker);
    }

    private static void Vinki_Jolly_Update(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyPlayerSelector self)
    {
        orig(self);
        // TODO: Make pup sprite
        SlugcatStats.Name playerClass = self.JollyOptions(self.index)?.playerClass;
        if (playerClass == null || !playerClass.value.Equals(Enums.vinki) || self.pupButton.isToggled || rainPodsSymbol == null)
        {
            orig(self);
            return;
        }

        Color color = GetCustomVinkiColor(self.index, 3);
        Color color2 = GetCustomVinkiColor(self.index, 4);
        Color color3 = GetCustomVinkiColor(self.index, 5);
        //VLogger.LogInfo("New shoe color: " + color2.ToString());

        rainPodsSymbol[self.index].sprite.color = color;
        shoesSymbol[self.index].sprite.color = color2;
        glassesSymbol[self.index].sprite.color = color3;

        self.pupButton.LoadIcon();
    }

    private static void Vinki_Jolly_ctor(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_ctor orig, SymbolButtonTogglePupButton self, Menu.Menu menu, MenuObject owner, string signal, Vector2 pos, Vector2 size, string symbolNameOn, string symbolNameOff, bool isOn, string stringLabelOn, string stringLabelOff)
    {
        orig(self, menu, owner, signal, pos, size, symbolNameOn, symbolNameOff, isOn, stringLabelOn, stringLabelOff);

        if (!symbolNameOff.Contains(Enums.vinkiStr) || menu is not JollySetupDialog)
        {
            return;
        }

        int index = GetPlayerIndex(self);

        // RainPods
        string fileName = "rainpods_" + (isOn ? symbolNameOn : symbolNameOff);
        rainPodsSymbol[index] = new MenuIllustration(menu, self, "", fileName, size / 2f, true, true);
        self.subObjects.Add(rainPodsSymbol[index]);
        // Shoes
        fileName = "shoes_" + (isOn ? symbolNameOn : symbolNameOff);
        shoesSymbol[index] = new MenuIllustration(menu, self, "", fileName, size / 2f, true, true);
        self.subObjects.Add(shoesSymbol[index]);
        // Glasses
        fileName = "glasses_" + (isOn ? symbolNameOn : symbolNameOff);
        glassesSymbol[index] = new MenuIllustration(menu, self, "", fileName, size / 2f, true, true);
        self.subObjects.Add(glassesSymbol[index]);

        self.LoadIcon();
    }

    private static bool Vinki_Jolly_Sprite(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_HasUniqueSprite orig, SymbolButtonTogglePupButton self)
    {
        // TODO: Make pup sprite
        if (self.symbolNameOff.Contains(Enums.vinkiStr) && !self.isToggled) return true;
        return orig(self);
    }

    private static string Vinki_Jolly_Name(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GetPupButtonOffName orig, JollyPlayerSelector self)
    {
        SlugcatStats.Name playerClass = self.JollyOptions(self.index).playerClass;
        if (playerClass != null && playerClass == Enums.vinki)
        {
            return "vinki_pup_off";
        }
        return orig(self);
    }

    private static void Vinki_Jolly_LoadIcon(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_LoadIcon orig, SymbolButtonTogglePupButton self)
    {
        orig(self);

        if (self.menu is not JollySetupDialog)
        {
            return;
        }

        int index = GetPlayerIndex(self);

        if (self.symbol == null || !self.symbol.fileName.Contains(Enums.vinkiStr) || self.symbol.fileName.Contains("on") ||
            rainPodsSymbol[index] == null)
        {
            return;
        }

        // RainPods
        rainPodsSymbol[index].fileName = "rainpods_" + self.symbol.fileName;
        rainPodsSymbol[index].LoadFile();
        rainPodsSymbol[index].sprite.color = GetCustomVinkiColor((self.owner as JollyPlayerSelector).index, 3);
        rainPodsSymbol[index].color = GetCustomVinkiColor((self.owner as JollyPlayerSelector).index, 3);
        rainPodsSymbol[index].sprite.SetElementByName(rainPodsSymbol[index].fileName);
        // Shoes
        shoesSymbol[index].fileName = "shoes_" + self.symbol.fileName;
        shoesSymbol[index].LoadFile();
        shoesSymbol[index].sprite.color = GetCustomVinkiColor((self.owner as JollyPlayerSelector).index, 4);
        shoesSymbol[index].color = GetCustomVinkiColor((self.owner as JollyPlayerSelector).index, 4);
        shoesSymbol[index].sprite.SetElementByName(shoesSymbol[index].fileName);
        // Glasses
        glassesSymbol[index].fileName = "glasses_" + self.symbol.fileName;
        glassesSymbol[index].LoadFile();
        glassesSymbol[index].sprite.color = GetCustomVinkiColor((self.owner as JollyPlayerSelector).index, 5);
        glassesSymbol[index].color = GetCustomVinkiColor((self.owner as JollyPlayerSelector).index, 5);
        glassesSymbol[index].sprite.SetElementByName(glassesSymbol[index].fileName);
    }

    private static Color GetCustomVinkiColor(int playerNumber, int bodyPartIndex)
    {
        if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.CUSTOM)
        {
            // Rider OC for AUTO player 2
            if (playerNumber == 1 && Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && bodyPartIndex < 5)
            {
                return new Color(0.98f, 1f, 0.039f);
            }

            switch (bodyPartIndex)
            {
                case 3: return Color.white;
                case 4:
                    if (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)
                    {
                        switch (playerNumber)
                        {
                            case 0: return new Color(0.28627450980392155f, 0.3058823529411765f, 0.8274509803921568f);
                            case 1: return new Color(0.79296875f, 0.953125f, 0.984375f);
                            case 2: return new Color(0.88671875f, 0.7890625f, 0.984375f);
                            case 3: return new Color(0.984375f, 0.7890625f, 0.81640625f);
                        }
                    }
                    return new Color(0.28627450980392155f, 0.3058823529411765f, 0.8274509803921568f);
                case 5: return new Color(0.054901960784313725f, 0.00784313725490196f, 0.00784313725490196f);
                default:
                    Plugin.VLogger.LogError("Invalid bodyPartIndex!\n" + StackTraceUtility.ExtractStackTrace());
                    return Color.white;
            }
        }
        //VLogger.LogInfo("Checking custom color for player " + playerNumber + ": " + Plugin.jollyColors[playerNumber][bodyPartIndex].GetValueOrDefault().ToString());
        return Plugin.jollyColors[playerNumber][bodyPartIndex].GetValueOrDefault().CloneWithNewAlpha(1f);
    }

    private static int GetPlayerIndex(SymbolButtonTogglePupButton pupButton)
    {
        return (pupButton.owner as JollyPlayerSelector).index;
    }
}
