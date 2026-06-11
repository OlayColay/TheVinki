using BepInEx;
using DressMySlugcat;
using MonoMod.RuntimeDetour;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Vinki;

[BepInDependency("dressmyslugcat", BepInDependency.DependencyFlags.SoftDependency)]
public static class DMSHooks
{
    public static int GlassesSprite, RainPodsSprite, ShoesSprite, ShineSprite;

    public static void ApplyDMSHooks()
    {
        new Hook(typeof(PlayerGraphicsDummy).GetConstructor([typeof(FancyMenu)]), PlayerGraphicsDummy_Constructor);
        new Hook(typeof(PlayerGraphicsDummy).GetMethod(nameof(PlayerGraphicsDummy.UpdateSpritePositions)), PlayerGraphicsDummy_UpdateSpritePositions);
        new Hook(typeof(PlayerGraphicsDummy).GetMethod(nameof(PlayerGraphicsDummy.UpdateSprites), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic), PlayerGraphicsDummy_UpdateSprites);
    }

    public static void PlayerGraphicsDummy_Constructor(Action<PlayerGraphicsDummy, FancyMenu> orig, PlayerGraphicsDummy self, FancyMenu owner)
    {
        orig(self, owner);

        RainPodsSprite = self.Sprites.Length;
        GlassesSprite = RainPodsSprite + 1;
        ShoesSprite = GlassesSprite + 1;
        ShineSprite = ShoesSprite + 1;

        Array.Resize(ref self.Sprites, ShineSprite + 1);
        self.Sprites[RainPodsSprite] = new FSprite("RainPodsA0");
        self.Sprites[GlassesSprite] = new FSprite("GlassesA0");
        self.Sprites[ShoesSprite] = new FSprite("ShoesA0");
        self.Sprites[ShineSprite] = new FSprite("ShineA0");

        self.UpdateSprites();

        self.AddToContainer();
    }

    public static void PlayerGraphicsDummy_UpdateSpritePositions(Action<PlayerGraphicsDummy> orig, PlayerGraphicsDummy self)
    {
        orig(self);

        ToggleVinkiSprites(self);
        if (self.Sprites.Length <= ShineSprite)
        {
            return;
        }

        self.Sprites[RainPodsSprite].x = self.Sprites[3].x;
        self.Sprites[RainPodsSprite].y = self.Sprites[3].y;
        self.Sprites[RainPodsSprite].anchorX = 0.5f;
        self.Sprites[RainPodsSprite].anchorY = 0.5f;
        self.Sprites[RainPodsSprite].scaleX = 1;
        self.Sprites[RainPodsSprite].scaleY = 1;
        self.Sprites[RainPodsSprite].rotation = 3.578826f;

        self.Sprites[GlassesSprite].x = self.Sprites[ShineSprite].x = self.Sprites[9].x;
        self.Sprites[GlassesSprite].y = self.Sprites[ShineSprite].y = self.Sprites[9].y;
        self.Sprites[GlassesSprite].anchorX = self.Sprites[ShineSprite].anchorX = 0.5f;
        self.Sprites[GlassesSprite].anchorY = self.Sprites[ShineSprite].anchorY = 0.5f;
        self.Sprites[GlassesSprite].scaleX = self.Sprites[ShineSprite].scaleX = 1;
        self.Sprites[GlassesSprite].scaleY = self.Sprites[ShineSprite].scaleY = 1;
        self.Sprites[GlassesSprite].rotation = self.Sprites[ShineSprite].rotation = 0;

        self.Sprites[ShoesSprite].x = self.Sprites[4].x;
        self.Sprites[ShoesSprite].y = self.Sprites[4].y;
        self.Sprites[ShoesSprite].anchorX = 0.5f;
        self.Sprites[ShoesSprite].anchorY = 0.25f;
        self.Sprites[ShoesSprite].scaleX = 1;
        self.Sprites[ShoesSprite].scaleY = 1;
        self.Sprites[ShoesSprite].rotation = 0;
    }

    public static void PlayerGraphicsDummy_UpdateSprites(Action<PlayerGraphicsDummy> orig, PlayerGraphicsDummy self)
    {
        orig(self);

        if (!ToggleVinkiSprites(self))
        {
            return;
        }

        Customization customization = Customization.For(self.owner.selectedSlugcat, self.owner.selectedPlayerIndex);

        // Glasses
        CustomSprite customSprite = customization.CustomSprite("GLASSES");
        if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("GlassesA0"))
        {
            self.Sprites[GlassesSprite].element = customSprite.SpriteSheet.Elements["GlassesA0"];
        }
        else
        {
            self.Sprites[GlassesSprite].element = Futile.atlasManager.GetElementWithName("GlassesA0");
        }
        self.Sprites[GlassesSprite].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(self.owner.selectedSlugcat, "GLASSES");


        // Rain Pods
        customSprite = customization.CustomSprite("RAIN PODS");
        if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("RainPodsA0"))
        {
            self.Sprites[RainPodsSprite].element = customSprite.SpriteSheet.Elements["RainPodsA0"];
        }
        else
        {
            self.Sprites[RainPodsSprite].element = Futile.atlasManager.GetElementWithName("RainPodsA0");
        }
        self.Sprites[RainPodsSprite].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(self.owner.selectedSlugcat, "RAIN PODS");


        // Shoes
        customSprite = customization.CustomSprite("SHOES");
        if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("ShoesA0"))
        {
            self.Sprites[ShoesSprite].element = customSprite.SpriteSheet.Elements["ShoesA0"];
        }
        else
        {
            self.Sprites[ShoesSprite].element = Futile.atlasManager.GetElementWithName("ShoesA0");
        }
        self.Sprites[ShoesSprite].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(self.owner.selectedSlugcat, "SHOES");

        // Glasses' Shine
        customSprite = customization.CustomSprite("SHINE");
        if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("ShineA0"))
        {
            self.Sprites[ShineSprite].element = customSprite.SpriteSheet.Elements["ShineA0"];
        }
        else
        {
            self.Sprites[ShineSprite].element = Futile.atlasManager.GetElementWithName("ShineA0");
        }
        self.Sprites[ShineSprite].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(self.owner.selectedSlugcat, "SHINE");
    }

    public static bool ToggleVinkiSprites(PlayerGraphicsDummy self)
    {
        bool isVinkiBased = self.owner.selectedSlugcat == Enums.vinkiStr || self.owner.selectedSlugcat == Enums.Swaggypup.ToString();
        if (ShineSprite > 0 && self.Sprites.Length > ShineSprite)
        {
            self.Sprites[RainPodsSprite].isVisible = isVinkiBased;
            self.Sprites[GlassesSprite].isVisible = isVinkiBased;
            self.Sprites[ShoesSprite].isVisible = isVinkiBased;
            self.Sprites[ShineSprite].isVisible = isVinkiBased;
            return isVinkiBased;
        }
        return false;
    }
}