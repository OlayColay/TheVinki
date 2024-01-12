using Menu;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
    private static void ApplyMenuSceneHooks()
    {
        On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
        On.Menu.MenuScene.Update += MenuScene_Update;
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
            List<MenuDepthIllustration> menuGraffitis = new List<MenuDepthIllustration>();
            foreach (var image in self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Graffiti - ")))
            {
                menuGraffitis.Add(image);
            }

            // Randomize which graffiti shows
            int randGraffiti = UnityEngine.Random.Range(0, menuGraffitis.Count);
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
            List<MenuDepthIllustration> sleepItems = new List<MenuDepthIllustration>();
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
            List<MenuDepthIllustration> menuGraffitis = new List<MenuDepthIllustration>();
            foreach (var image in self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Graffiti - ")))
            {
                menuGraffitis.Add(image);
            }

            // Randomize which graffiti shows
            int randGraffiti = UnityEngine.Random.Range(0, menuGraffitis.Count - 1);
            string fileName = "Graffiti - " + randGraffiti.ToString();

            // Show the random graffiti and hide the rest
            foreach (var image in menuGraffitis)
            {
                string imageName = Path.GetFileNameWithoutExtension(image.fileName);
                //Debug.Log("Graffiti: Checking if " + imageName + " matches " + fileName + "\t" + (imageName == fileName));
                image.alpha = (imageName == fileName) ? 1f : 0f;
            }

            // Find the doodle layers of the slugcat select scene
            List<MenuDepthIllustration> menuDoodles = new List<MenuDepthIllustration>();
            foreach (var image in self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Doodle - ")))
            {
                menuDoodles.Add(image);
            }

            // Randomize which doodle shows
            int randDoodles = UnityEngine.Random.Range(0, menuDoodles.Count);
            fileName = "Doodle - " + randDoodles.ToString();

            // Show the random doodle and hide the rest
            foreach (var image in menuDoodles)
            {
                string imageName = Path.GetFileNameWithoutExtension(image.fileName);
                //Debug.Log("Doodle: Checking if " + imageName + " matches " + fileName);
                image.alpha = (imageName == fileName) ? 1f : 0f;
            }
        }
        else if (self.sceneID == Enums.GraffitiMap)
        {
            Debug.Log("Building Graffiti Map Scene!\n" + StackTraceUtility.ExtractStackTrace());
            self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Graffiti Map";
            if (self.flatMode)
            {
                self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "graffiti_map", new Vector2(Screen.width / 2, Screen.height / 2), true, true));
                self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "wip_black", new Vector2(Screen.width * 2 / 3, Screen.height / 3), true, true));
                self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "wip_overseer", new Vector2(Screen.width / 3, Screen.height * 5 / 12), true, true));
            }
            else
            {
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "graffiti_map", new Vector2(0f, 0f), 5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "wip_black", new Vector2(512f, -64f), 3f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "wip_overseer", new Vector2(0f, -64f), 2f, MenuDepthIllustration.MenuShader.Basic));
            }

            GraffitiDialog.graffitiSpots = new MenuDepthIllustration[]
            {
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, "graffiti_ss", new Vector2(750, 550), 4f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, "graffiti_ss", new Vector2(800, 560), 4.5f, MenuDepthIllustration.MenuShader.Basic),
                new MenuDepthIllustration(self.menu, self, self.sceneFolder, "graffiti_test", new Vector2(650, 580), 6f, MenuDepthIllustration.MenuShader.Basic)
            };
            GraffitiDialog.graffitiSlapping = new int[GraffitiDialog.graffitiSpots.Length];

            // Save that we sprayed self story graffiti
            SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(self.menu.manager.rainWorld.progression.currentSaveState.miscWorldSaveData);
            if (miscSave.TryGet("StoryGraffitisSprayed", out int[] sprd))
            {
                Plugin.storyGraffitisSprayed = sprd;
            }
            if (miscSave.TryGet("StoryGraffitisOnMap", out int[] onMap))
            {
                Plugin.storyGraffitisOnMap = onMap;
            }
            for (int i = 0; i < Plugin.storyGraffitisSprayed.Length; i++)
            {
                GraffitiDialog.graffitiSpots[i].alpha = Plugin.storyGraffitisOnMap.Contains(Plugin.storyGraffitisSprayed[i]) ? 1f : 0f;
                if (!Plugin.storyGraffitisOnMap.Contains(Plugin.storyGraffitisSprayed[i]))
                {
                    GraffitiDialog.graffitiSlapping[i] = (int)GraffitiDialog.slapLength;
                    GraffitiDialog.graffitiSpots[i].sprite.scale = 0.1f;
                    Plugin.storyGraffitisOnMap.Append(Plugin.storyGraffitisSprayed[i]);
                }
            }
            miscSave.Set("StoryGraffitisOnMap", Plugin.storyGraffitisOnMap);

            for (int i = 0; i < GraffitiDialog.graffitiSpots.Length; i++)
            {
                if (Plugin.storyGraffitisOnMap.Contains(Plugin.storyGraffitisSprayed[i]))
                {
                    // TODO
                    //self.AddIllustration(GraffitiDialog.graffitiSpots[i]);
                }
            }
        }
    }

    private static float rotateAmount = 1f;
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