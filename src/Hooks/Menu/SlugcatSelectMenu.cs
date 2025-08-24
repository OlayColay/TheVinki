using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Vinki;

public class SlugcatPageData
{
    public MenuLabel bestCombo;
    public MenuLabel bestScore;
}
public static class SlugcatPageExtension
{
    private static readonly ConditionalWeakTable<SlugcatSelectMenu.SlugcatPage, SlugcatPageData> cwt = new();

    public static SlugcatPageData Vinki(this SlugcatSelectMenu.SlugcatPage gm) => cwt.GetValue(gm, _ => new SlugcatPageData());
}

public static partial class Hooks
{
    private static void ApplySlugcatSelectMenuHooks()
    {
        On.Menu.SlugcatSelectMenu.CommunicateWithUpcomingProcess += SlugcatSelectMenu_CommunicateWithUpcomingProcess;

        On.Menu.SlugcatSelectMenu.SlugcatPage.ctor += SlugcatSelectMenu_SlugcatPage_ctor;
        On.Menu.SlugcatSelectMenu.SlugcatPage.GrafUpdate += SlugcatPage_GrafUpdate;

        On.Menu.SlugcatSelectMenu.SlugcatPage.AddAltEndingImage += SlugcatPage_AddAltEndingImage;

        try
        {
            //IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
        }
        catch (Exception ex)
        {
            Plugin.VLogger.LogError("Could not apply StartGame IL hook\n" + ex.Message);
        }
    }

    private static void SlugcatSelectMenu_CommunicateWithUpcomingProcess(On.Menu.SlugcatSelectMenu.orig_CommunicateWithUpcomingProcess orig, SlugcatSelectMenu self, MainLoopProcess nextProcess)
    {
        orig(self, nextProcess);

        if (nextProcess.ID == Enums.FullscreenVideo)
        {
            (nextProcess as FullscreenVideo).StartVideo("videos/VinkiIntro.mp4", ProcessManager.ProcessID.Game);
        }
    }

    private static void SlugcatSelectMenu_SlugcatPage_ctor(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_ctor orig, SlugcatSelectMenu.SlugcatPage self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
    {
        orig(self, menu, owner, pageIndex, slugcatNumber);

        if (slugcatNumber != Enums.vinki)
        {
            return;
        }

        SlugBaseSaveData miscProgressionData = SaveDataExtension.GetSlugBaseData(menu.manager.rainWorld.progression.miscProgressionData);
        if (miscProgressionData.TryGet("VinkiBestCombo", out int bestCombo) && miscProgressionData.TryGet("VinkiBestScore", out int bestScore))
        {
            SlugcatPageData data = self.Vinki();

            data.bestCombo = new(menu, self, menu.Translate("Best Trick Combo: " + bestCombo), new(-1500f, 500f), new(110f, 30f), true);
            data.bestCombo.label.alignment = FLabelAlignment.Left;
            data.bestCombo.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            self.subObjects.Add(data.bestCombo);

            data.bestScore = new(menu, self, menu.Translate("Best Trick Score: " + bestScore), new(-1500f, 470f), new(110f, 30f), true);
            data.bestScore.label.alignment = FLabelAlignment.Left;
            data.bestScore.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            self.subObjects.Add(data.bestScore);
        }
    }

    private static void SlugcatPage_GrafUpdate(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_GrafUpdate orig, SlugcatSelectMenu.SlugcatPage self, float timeStacker)
    {
        orig(self, timeStacker);

        if (self.slugcatNumber != Enums.vinki)
        {
            return;
        }

        SlugcatPageData data = self.Vinki();
        if (data.bestCombo == null || data.bestScore == null)
        {
            return;
        }

        data.bestCombo.label.alpha = data.bestScore.label.alpha = self.UseAlpha(timeStacker);
        data.bestCombo.label.x = data.bestScore.label.x = self.MidXpos + self.Scroll(timeStacker) * self.ScrollMagnitude - 600f;
    }

    private static void SlugcatPage_AddAltEndingImage(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_AddAltEndingImage orig, SlugcatSelectMenu.SlugcatPage self)
    {
        if (self.slugcatNumber == Enums.vinki)
        {
            SlugBaseSaveData miscProgressionSave = SaveDataExtension.GetSlugBaseData(self.menu.manager.rainWorld.progression.miscProgressionData);
            if (miscProgressionSave.TryGet("VinkiEndingID", out int vinkiEndingID))
            {
                switch (vinkiEndingID)
                {
                    case Enums.EndingID.QuestIncompleteOE:
                        self.slugcatImage = new InteractiveMenuScene(self.menu, self, Enums.MenuSceneID.OEEnd_Vinki_Incomplete);
                        break;
                    case Enums.EndingID.QuestCompleteOE:
                        self.slugcatImage = new InteractiveMenuScene(self.menu, self, Enums.MenuSceneID.OEEnd_Vinki_Complete);
                        break;
                }

                self.imagePos = new Vector2(683f, 484f);
                self.subObjects.Add(self.slugcatImage);
                return;
            }
        }
        orig(self);
    }

    private static void SlugcatSelectMenu_StartGame(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel soundLabel = null;

        try
        {
            c.GotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.White)),
                x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Inequality"))
            );

            new ILCursor(c).GotoNext(x => x.MatchBr(out soundLabel));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter) =>
            {
                if (storyGameCharacter == Enums.vinki)
                {
                    SaveDataExtension.GetSlugBaseData(self.manager.rainWorld.progression.miscProgressionData)
                        .Set("StoryPlacedGraffitis", new Dictionary<string, List<GraffitiObject.SerializableGraffiti>>());
                    if (!VinkiConfig.SkipIntro.Value)
                    {
                        self.manager.RequestMainProcessSwitch(Enums.FullscreenVideo);
                        return true;
                    }
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue_S, soundLabel);
        }
        catch (Exception e) 
        { 
            Plugin.VLogger.LogError("Could not complete StartGame IL Hook\n" + e.Message); 
        }
    }
}