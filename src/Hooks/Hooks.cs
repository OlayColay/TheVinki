using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SprayCans;
using Fisobs.Core;
using MoreSlugcats;
using static Vinki.Plugin;

namespace Vinki
{
    public static partial class Hooks
    {
        public static void ApplyInit()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        }

        // Add hooks
        private static void ApplyHooks()
        {
            Content.Register(new SprayCanFisob());

            // Put your custom hooks here!
            ApplyPlayerHooks();
            ApplyPlayerGraphicsHooks();
            ApplyShelterDoorHooks();
            ApplySSOracleHooks();
            ApplyRoomHooks();
            ApplyJollyCoopHooks();
            ApplySaveStateHooks();
        }

        // Load any resources, such as sprites or sounds
        private static void LoadResources(RainWorld rainWorld)
        {
            Enums.RegisterValues();
            ApplyHooks();

            // Add the story graffitis
            AddGraffiti("5P", new Vector2?(new Vector2(650, 200)));
            AddGraffiti("5P_stretched", new Vector2?(new Vector2(520, 400)));

            // If the graffiti folder doesn't exist (or is empty), copy it from the mod
            if (!Directory.Exists(graffitiFolder) || !Directory.EnumerateFileSystemEntries(graffitiFolder).Any())
            {
                string modFolder = AssetManager.ResolveDirectory("../../../../workshop/content/312520/3001275271");
                if (!Directory.Exists(modFolder))
                {
                    Debug.Log("Vinki was not installed from Steam Workshop. Attempting to find locally...");
                    modFolder = AssetManager.ResolveDirectory("./mods/thevinki");
                    if (!Directory.Exists(modFolder))
                    {
                        Debug.LogError("Could not find Vinki mod in workshop files or local mods!");
                        return;
                    }
                }
                Debug.Log("Graffiti folder doesn't exist! Copying from mod folder: " + modFolder);
                CopyFilesRecursively(modFolder + "/VinkiGraffiti", graffitiFolder);
            }

            // Go through each graffiti image and add it to the list of decals Vinki can place
            string parent = Path.GetFileNameWithoutExtension(graffitiFolder);
            foreach (var image in Directory.EnumerateFiles(graffitiFolder, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".png")).Select(f => parent + '/' + Path.GetFileNameWithoutExtension(f)))
            {
                AddGraffiti(image);
            }

            // Remix menu config
            VinkiConfig.RegisterOI();

            // Get sprite atlases
            Futile.atlasManager.LoadAtlas("atlases/SprayCan");
            Futile.atlasManager.LoadAtlas("atlases/glasses");
            Futile.atlasManager.LoadAtlas("atlases/rainpods");
            Futile.atlasManager.LoadAtlas("atlases/shoes");

            TailTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
            var tailTextureFile = AssetManager.ResolveFilePath("textures/VinkiTail.png");
            if (File.Exists(tailTextureFile))
            {
                var rawData = File.ReadAllBytes(tailTextureFile);
                TailTexture.LoadImage(rawData);
            }

            // Populate the colorfulItems List for crafting Spray Cans
            InitColorfulItems();
        }

        private static void AddGraffiti(string image, Vector2? storyGraffitiRoomPos = null)
        {
            PlacedObject.CustomDecalData decal = new PlacedObject.CustomDecalData(null);
            decal.imageName = image;
            decal.fromDepth = 0.2f;

            string filePath;
            if (storyGraffitiRoomPos.HasValue)
            {
                filePath = AssetManager.ResolveFilePath("decals/" + image + ".png");
                storyGraffitiRoomPositions.Add(graffitis.Count, storyGraffitiRoomPos.Value);
                storyGraffitiCount++;
            }
            else
            {
                filePath = graffitiFolder + "/" + Path.GetFileNameWithoutExtension(image) + ".png";
            }

            // Get the image as a 2d texture so we can resize it to something manageable
            Texture2D img = new Texture2D(2, 2);
            byte[] tmpBytes = File.ReadAllBytes(filePath);
            ImageConversion.LoadImage(img, tmpBytes);

            // Get average color of image (to use for graffiti spray/smoke color)
            graffitiAvgColors.Add(AverageColorFromTexture(img));

            // Resize image to look good in game
            if (!storyGraffitiRoomPos.HasValue)
            {
                int[] newSize = ResizeAndKeepAspectRatio(img.width, img.height, 100f * 100f);
                img.Resize(newSize[0], newSize[1]);
            }

            decal.handles[0] = new Vector2(0f, img.height);
            decal.handles[1] = new Vector2(img.width, img.height);
            decal.handles[2] = new Vector2(img.width, 0f);

            float halfWidth = img.width / 2f;
            float halfHeight = img.height / 2f;

            graffitiOffsets.Add(new Vector2(-halfWidth, -halfHeight));
            graffitis.Add(decal);
        }

        public static bool IsPostInit;
        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsPostInit) return;
                IsPostInit = true;

                // Putting this hook here ensures that SlugBase's BuildScene hook goes first
                On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void InitColorfulItems()
        {
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.AttachedBee, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.BlinkingFlower, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.BubbleGrass, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.DangleFruit, 1);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.DataPearl, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.EggBugEgg, 1);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.FlareBomb, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.FlyLure, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.Lantern, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.Mushroom, 1);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.NeedleEgg, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.PuffBall, 1);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.SlimeMold, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.SporePlant, 1);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.WaterNut, 2);

            if (!ModManager.MSC)
            {
                return;
            }

            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.DandelionPeach, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.FireEgg, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.Germinator, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.GlowWeed, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.GooieDuck, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.LillyPuck, 1);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.Seed, 1);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, 9001);
        }

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, Menu.MenuScene self)
        {
            orig(self);

            if (self.sceneID.ToString() == "Slugcat_Vinki")
            {
                // Find the graffiti layers of the slugcat select scene
                List<Menu.MenuDepthIllustration> menuGraffitis = new List<Menu.MenuDepthIllustration>();
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
                List<Menu.MenuDepthIllustration> sleepItems = new List<Menu.MenuDepthIllustration>();
                foreach (Menu.MenuDepthIllustration image in self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Item - ")))
                {
                    image.alpha = 0f;
                    string imageName = Path.GetFileNameWithoutExtension(image.fileName);

                    // Show the item layers that are in the shelter
                    foreach (string item in shelterItems)
                    {
                        if (imageName.EndsWith(item))
                        {
                            image.alpha = 1f;
                        }
                    }
                }

                shelterItems.Clear();

                // Find the graffiti layers of the slugcat select scene
                List<Menu.MenuDepthIllustration> menuGraffitis = new List<Menu.MenuDepthIllustration>();
                foreach (var image in self.depthIllustrations.Where(f => Path.GetFileNameWithoutExtension(f.fileName).StartsWith("Graffiti - ")))
                {
                    menuGraffitis.Add(image);
                }

                // Randomize which graffiti shows
                int randGraffiti = UnityEngine.Random.Range(0, menuGraffitis.Count-1);
                string fileName = "Graffiti - " + randGraffiti.ToString();

                // Show the random graffiti and hide the rest
                foreach (var image in menuGraffitis)
                {
                    string imageName = Path.GetFileNameWithoutExtension(image.fileName);
                    Debug.Log("Graffiti: Checking if " + imageName + " matches " + fileName + "\t" + (imageName == fileName));
                    image.alpha = (imageName == fileName) ? 1f : 0f;
                }

                // Find the doodle layers of the slugcat select scene
                List<Menu.MenuDepthIllustration> menuDoodles = new List<Menu.MenuDepthIllustration>();
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
                    Debug.Log("Doodle: Checking if " + imageName + " matches " + fileName);
                    image.alpha = (imageName == fileName) ? 1f : 0f;
                }
            }
        }

        private static int[] ResizeAndKeepAspectRatio(float original_width, float original_height, float target_area)
        {
            float new_width = Mathf.Sqrt((original_width / original_height) * target_area);
            float new_height = target_area / new_width;

            int w = Mathf.RoundToInt(new_width); // round to the nearest integer
            int h = Mathf.RoundToInt(new_height - (w - new_width)); // adjust the rounded width with height 

            return new int[] { w, h };
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static Color AverageColorFromTexture(Texture2D tex)
        {
            Color[] texColors = tex.GetPixels();
            int total = texColors.Length;

            float r = 0f;
            float g = 0f;
            float b = 0f;

            for (int i = 0; i < texColors.Length; i++)
            {
                if (texColors[i].a <= 0.1f)
                {
                    total--;
                    continue;
                }

                r += texColors[i].r;
                g += texColors[i].g;
                b += texColors[i].b;
            }

            r /= total;
            g /= total;
            b /= total;

            if (r + g + b < float.Epsilon)
            {
                return Color.white;
            }
            return new Color(r, g, b, 1f);
        }
    }
}