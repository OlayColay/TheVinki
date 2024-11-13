using Menu;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;

public class CollectiblesTrackerData()
{
    public List<Enums.GraffitiUnlockID> unlockedGraffitis = [];
}
public static class CollectiblesTrackerExtension
{
    private static readonly ConditionalWeakTable<CollectiblesTracker, CollectiblesTrackerData> cwt = new();

    public static CollectiblesTrackerData Vinki(this CollectiblesTracker self) => cwt.GetValue(self, _ => new CollectiblesTrackerData());
}

public static partial class Hooks
{
    private static void ApplyCollectiblesTrackerHooks()
    {
        On.MoreSlugcats.CollectiblesTracker.ctor += CollectiblesTracker_ctor;
        On.MoreSlugcats.CollectiblesTracker.GrafUpdate += CollectiblesTracker_GrafUpdate;
        On.MoreSlugcats.CollectiblesTracker.MineForSaveData += CollectiblesTracker_MineForSaveData;
    }

    private static void RemoveCollectiblesTrackerHooks()
    {
        On.MoreSlugcats.CollectiblesTracker.ctor -= CollectiblesTracker_ctor;
        On.MoreSlugcats.CollectiblesTracker.GrafUpdate -= CollectiblesTracker_GrafUpdate;
        On.MoreSlugcats.CollectiblesTracker.MineForSaveData -= CollectiblesTracker_MineForSaveData;
    }

    private static void CollectiblesTracker_ctor(On.MoreSlugcats.CollectiblesTracker.orig_ctor orig, MoreSlugcats.CollectiblesTracker self, Menu.Menu menu, MenuObject owner, Vector2 pos, FContainer container, SlugcatStats.Name saveSlot)
    {
        orig(self, menu, owner, pos, container, saveSlot);

        if (saveSlot != Enums.vinki)
        {
            return;
        }

        //Plugin.VLogger.LogInfo("Gold tokens: " + string.Join("\t", menu.manager.rainWorld.regionGoldTokens.FirstOrDefault().Value));
        //Plugin.VLogger.LogInfo(string.Join("\t", ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.entries));
        //if (menu.manager.rainWorld.Vinki().regionGraffitiTokens != null)
        //{
        //    Plugin.VLogger.LogInfo("Graffiti tokens: " + string.Join("\t", menu.manager.rainWorld.Vinki().regionGraffitiTokens["dm"]));
        //    Plugin.VLogger.LogInfo(string.Join("\t", ExtEnum<Enums.GraffitiUnlockID>.values.entries));
        //}

        RainWorldData rainWorldData = menu.manager.rainWorld.Vinki();
        CollectiblesTrackerData ext = self.Vinki();
        foreach (string region in self.displayRegions)
        {
            if (self.collectionData == null || !self.collectionData.regionsVisited.Contains(region))
            {
                //Plugin.VLogger.LogInfo("collectionData does not include region: " + self.displayRegions[l]);
                continue;
            }
            if (!rainWorldData.regionGraffitiTokens.ContainsKey(region))
            {
                //Plugin.VLogger.LogInfo("regionGraffitiTokens does not include region: " + self.displayRegions[l]);
            }

            //Plugin.VLogger.LogInfo("regionGraffitiTokens for " + self.displayRegions[l] + ": " + string.Join(", ", rainWorldData.regionGraffitiTokens[self.displayRegions[l]]));
            foreach (Enums.GraffitiUnlockID token in rainWorldData.regionGraffitiTokens[region])
            {
                self.spriteColors[region].Add(new Color(1f, 0.051f, 0.965f));
                if (!ext.unlockedGraffitis.Contains(token))
                {
                    self.sprites[region].Add(new FSprite("ctOff", true));
                }
                else
                {
                    self.sprites[region].Add(new FSprite("ctOn", true));
                }
                self.sprites[region][self.sprites[region].Count - 1].color = self.spriteColors[region][self.spriteColors[region].Count - 1];
                container.AddChild(self.sprites[region][self.sprites[region].Count - 1]);
            }
        }
    }

    private static void CollectiblesTracker_GrafUpdate(On.MoreSlugcats.CollectiblesTracker.orig_GrafUpdate orig, CollectiblesTracker self, float timeStacker)
    {
        orig(self, timeStacker);
        for (int i = 0; i < self.displayRegions.Count; i++)
        {
            //Plugin.VLogger.LogInfo("sprites for " + self.displayRegions[i] + ":");
            for (int j = 0; j < self.sprites[self.displayRegions[i]].Count; j++)
            {
                FSprite s = self.sprites[self.displayRegions[i]][j];
                //Plugin.VLogger.LogInfo(j + ": " + s.color.ToString() + "\ty:" + s.y);
            }
        }
    }

    private static CollectiblesTracker.SaveGameData CollectiblesTracker_MineForSaveData(On.MoreSlugcats.CollectiblesTracker.orig_MineForSaveData orig, MoreSlugcats.CollectiblesTracker self, ProcessManager manager, SlugcatStats.Name slugcat)
    {
        CollectiblesTracker.SaveGameData saveGameData = orig(self, manager, slugcat);

        if (saveGameData == null || !manager.rainWorld.progression.IsThereASavedGame(slugcat))
        {
            return null;
        }
        SaveState saveState = null;
        if (manager.rainWorld.progression.currentSaveState != null)
        {
            saveState = manager.rainWorld.progression.currentSaveState;
        }
        else if (manager.rainWorld.progression.starvedSaveState != null)
        {
            saveState = manager.rainWorld.progression.starvedSaveState;
        }
        if (saveState != null && saveState.saveStateNumber == slugcat)
        {
            self.Vinki().unlockedGraffitis = [];
            foreach (string entry in ExtEnum<Enums.GraffitiUnlockID>.values.entries)
            {
                Enums.GraffitiUnlockID graffitiUnlockID = new(entry, false);
                if (manager.rainWorld.progression.miscProgressionData.GetTokenCollected(graffitiUnlockID))
                {
                    self.Vinki().unlockedGraffitis.Add(graffitiUnlockID);
                }
            }
        }

        return saveGameData;
    }
}
