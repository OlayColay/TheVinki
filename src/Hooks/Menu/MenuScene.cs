using Menu;
using SlugBase.SaveData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
    private static void ApplyMenuSceneHooks()
    {
        On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
        On.Menu.MenuScene.Update += MenuScene_Update;
    }
    private static void RemoveMenuSceneHooks()
    {
        On.Menu.MenuScene.BuildScene -= MenuScene_BuildScene;
        On.Menu.MenuScene.Update -= MenuScene_Update;
    }

    private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
    {
        orig(self);

        if (self.sceneID == null)
        {
            return;
        }

        if (self.sceneID.ToString() == "Ghost_Vinki")
        {
            // Find the ghost layers of the slugcat ghost scene
            Plugin.rotatingGhost.Clear();
            foreach (var image in self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Ghost Vinki")))
            {
                image.sprite.anchorX = 0.5f;
                image.sprite.anchorY = 0.5f;
                Plugin.rotatingGhost.Add(image);
            }
        }
        else if (self.sceneID.ToString() == "Slugcat_Vinki")
        {
            // Find the graffiti layers of the slugcat select scene
            List<MenuDepthIllustration> menuGraffitis = [.. self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Graffiti - "))];

            // Randomize which graffiti shows
            int randGraffiti = Random.Range(0, menuGraffitis.Count);
            string fileName = "Graffiti - " + randGraffiti.ToString();

            // Show the random graffiti and hide the rest
            foreach (var image in menuGraffitis)
            {
                string imageName = Path.GetFileNameWithoutExtension(image.fileName);
                image.alpha = (imageName == fileName) ? 1f : 0f;
            }
        }
        else if (self.sceneID.ToString() == "Sleep_Vinki")
        {
            // Find the item layers of the slugcat select scene
            List<MenuDepthIllustration> sleepItems = [];
            foreach (MenuDepthIllustration image in self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Item - ")))
            {
                image.alpha = 0f;
                string imageName = Path.GetFileNameWithoutExtension(image.fileName);
                imageName = imageName.Substring(imageName.IndexOf('-') + 2);

                // Show the item layers that are in the shelter
                foreach (string item in Plugin.shelterItems)
                {
                    if (imageName == item)
                    {
                        image.alpha = 1f;
                    }
                }
            }

            Plugin.shelterItems.Clear();

            // Find the graffiti layers of the slugcat select scene
            List<MenuDepthIllustration> menuGraffitis = [.. self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Graffiti - "))];

            // Randomize which graffiti shows
            int randGraffiti = Random.Range(0, menuGraffitis.Count - 1);
            string fileName = "Graffiti - " + randGraffiti.ToString();

            // Show the random graffiti and hide the rest
            foreach (var image in menuGraffitis)
            {
                string imageName = Path.GetFileNameWithoutExtension(image.fileName);
                //VLogger.LogInfo("Graffiti: Checking if " + imageName + " matches " + fileName + "\t" + (imageName == fileName));
                image.alpha = (imageName == fileName) ? 1f : 0f;
            }

            // Find the doodle layers of the slugcat select scene
            List<MenuDepthIllustration> menuDoodles = [.. self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Doodle - "))];

            // Randomize which doodle shows
            int randDoodles = Random.Range(0, menuDoodles.Count);
            fileName = "Doodle - " + randDoodles.ToString();

            // Show the random doodle and hide the rest
            foreach (var image in menuDoodles)
            {
                string imageName = Path.GetFileNameWithoutExtension(image.fileName);
                //VLogger.LogInfo("Doodle: Checking if " + imageName + " matches " + fileName);
                image.alpha = (imageName == fileName) ? 1f : 0f;
            }
        }
        else if (self.sceneID == Enums.MenuSceneID.GraffitiMap)
        {
            //Plugin.VLogger.LogInfo("Building Graffiti Map Scene!\n" + StackTraceUtility.ExtractStackTrace());
            self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Graffiti Map";
            self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "graffiti_map", new Vector2(0f, 0f), 5f, MenuDepthIllustration.MenuShader.Basic));
            self.AddIllustration(GraffitiQuestDialog.cloud = new MenuDepthIllustration(self.menu, self, self.sceneFolder, "cloud", new Vector2(680f, 324f), 5f, MenuDepthIllustration.MenuShader.Basic));
            self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "wip_black", new Vector2(762f, -64f), 3f, MenuDepthIllustration.MenuShader.Basic));
            self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "wip_overseer", new Vector2(-50f, -64f), 2f, MenuDepthIllustration.MenuShader.Basic));

            GraffitiQuestDialog.graffitiSpots =
            [
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_ss" : "graffiti_ss_alt", new Vector2(826, 569), 4f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_ss" : "graffiti_ss_alt", new Vector2(826, 569), 4.5f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_test" : "graffiti_test_alt", new Vector2(706, 632), 6f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, "true_victory", new Vector2(1163, 454), 6f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_test" : "graffiti_test_alt", new Vector2(861, 419), 6f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_test" : "graffiti_test_alt", new Vector2(1131, 587), 6f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_test" : "graffiti_test_alt", new Vector2(868, 190), 6f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_test" : "graffiti_test_alt", new Vector2(719, 230), 6f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_test" : "graffiti_test_alt", new Vector2(508, 186), 6f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_test" : "graffiti_test_alt", new Vector2(263, 136), 6f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, VinkiConfig.CatPebbles.Value ? "graffiti_test" : "graffiti_test_alt", new Vector2(382, 304), 6f, MenuDepthIllustration.MenuShader.Basic)
            ];
            GraffitiQuestDialog.graffitiSlapping = new int[GraffitiQuestDialog.graffitiSpots.Length];

            foreach (MenuDepthIllustration illustration in GraffitiQuestDialog.graffitiSpots)
            {
                illustration.sprite.SetAnchor(0.5f, 0.5f);
            }
            GraffitiQuestDialog.cloud.sprite.SetAnchor(0.5f, 0.5f);

            // Save that we sprayed self story graffiti
            SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(self.menu.manager.rainWorld.progression.currentSaveState.miscWorldSaveData);
            miscSave.TryGet("StoryGraffitisSprayed", out int[] sprd);
            miscSave.TryGet("StoryGraffitisOnMap", out int[] onMap);
            sprd ??= [];
            onMap ??= [];
            //RWCustom.Custom.Log("Sprayed: " + string.Join(", ", sprd) + "\nOn Map: " + string.Join(", ", onMap) + "\nDied last cycle: " + Plugin.diedLastCycle.ToString());
            if (!Plugin.diedLastCycle)
            {
                for (int i = 0; i < sprd.Length && i < GraffitiQuestDialog.graffitiSpots.Length; i++)
                {
                    // We don't care about the Monroe graffiti (the second story graffiti)
                    if (sprd[i] < GraffitiQuestDialog.graffitiSpots.Length && sprd[i] != 1)
                    {
                        GraffitiQuestDialog.graffitiSpots[i].alpha = onMap.Contains(sprd[i]) ? 1f : 0f;
                        if (!onMap.Contains(sprd[i]))
                        {
                            GraffitiQuestDialog.graffitiSlapping[sprd[i]] = (int)GraffitiQuestDialog.slapLength;
                            GraffitiQuestDialog.graffitiSpots[sprd[i]].sprite.scale = 0.001f;
                            onMap = [.. onMap, sprd[i]];
                        }
                    }
                }
            }
            //RWCustom.Custom.Log("Setting map to: ", $"[{string.Join(", ", onMap)}]");
            Plugin.storyGraffitisOnMap = onMap;

            for (int i = 0; i < sprd.Length; i++)
            {
                if (sprd[i] < GraffitiQuestDialog.graffitiSpots.Length && sprd[i] != 1 && onMap.Contains(sprd[i]))
                {
                    self.AddIllustration(GraffitiQuestDialog.graffitiSpots[sprd[i]]);
                }
            }
        }
    }

    private static readonly float rotateAmount = 1f;
    private static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, MenuScene self)
    {
        orig(self);

        if (self.sceneID == null)
        {
            return;
        }

        if (self.sceneID.ToString() == "Ghost_Vinki")
        {
            foreach(MenuDepthIllustration layer in Plugin.rotatingGhost)
            {
                layer.sprite.rotation += rotateAmount;
            }
        }
    }
}