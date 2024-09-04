using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SprayCans;
using Fisobs.Core;
using MoreSlugcats;
using static Vinki.Plugin;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using SlugBase;
using BepInEx;
using DressMySlugcat;
using SlugBase.SaveData;
using MonoMod.RuntimeDetour;
using PushToMeowMod;
using BepInEx.Logging;

namespace Vinki
{
    [BepInDependency("dressmyslugcat", BepInDependency.DependencyFlags.SoftDependency)]
    public static partial class Hooks
    {
        public static void ApplyInit()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;

            On.RainWorldGame.ctor += RainWorldGame_ctor;
        }

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);

            if (restartMode)
            {
                Enums.RegisterValues();
                ApplyHooks();

                LoadResources(self.rainWorld);
                RainWorld_PostModsInit((_) => { }, self.rainWorld);
            }

            if (self.IsStorySession && self.GetStorySession.saveStateNumber == Enums.vinki)
            { 
                SlugBaseSaveData miscWorldSave = SaveDataExtension.GetSlugBaseData(self.GetStorySession.saveState.miscWorldSaveData);
                miscWorldSave.Set("AutoOpenMap", false);

                // Save story graffiti on the map before cycle ends
                if (storyGraffitisOnMap.Length > 0)
                {
                    miscWorldSave.Set("StoryGraffitisOnMap", storyGraffitisOnMap);
                }
            }
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
            ApplySleepAndDeathScreenHooks();
            ApplyLizardGraphicsHooks();
            ApplySnailGraphicsHooks();
            ApplyVultureGraphicsHooks();
            ApplyCollectTokenHooks();
            ApplyExpeditionHooks();
            ApplyPauseMenuHooks();
            ApplyGhostHooks();
            ApplyAncientBotHooks();
            ApplyRegionGateHooks();
            ApplyAbstractCreatureHooks();
            ApplyMusicHooks();
        }

        public static void RemoveHooks()
        {
            On.RainWorld.OnModsInit -= Extras.WrapInit(LoadResources);
            On.RainWorld.PostModsInit -= RainWorld_PostModsInit;
            On.RainWorldGame.ctor -= RainWorldGame_ctor;

            RemovePlayerHooks();
            RemovePlayerGraphicsHooks();
            RemoveShelterDoorHooks();
            RemoveSSOracleHooks();
            RemoveRoomHooks();
            RemoveJollyCoopHooks();
            RemoveSaveStateHooks();
            RemoveSleepAndDeathScreenHooks();
            RemoveLizardGraphicsHooks();
            RemoveSnailGraphicsHooks();
            RemoveVultureGraphicsHooks();
            RemoveCollectTokenHooks();
            RemoveExpeditionHooks();
            RemovePauseMenuHooks();
            RemoveGhostHooks();
            RemoveAncientBotHooks();
            RemoveRegionGateHooks();
            RemoveAbstractCreatureHooks();
            RemoveMusicHooks();

            RemoveMenuSceneHooks();

            On.ProcessManager.PostSwitchMainProcess -= ProcessManager_PostSwitchMainProcess;
            IL.Menu.IntroRoll.ctor -= IntroRoll_ctor;
        }

        // Load any resources, such as sprites or sounds
        private static void LoadResources(RainWorld rainWorld)
        {
            if (!restartMode)
            {
                Enums.RegisterValues();
                ApplyHooks();
            }

            ApplySlugcatSelectMenuHooks();

            bool modChanged = false;
            if (rainWorld.options.modLoadOrder.TryGetValue("olaycolay.thevinki", out _) && VinkiConfig.RestoreGraffitiOnUpdate.Value)
            {
                ModManager.Mod vinkiMod = ModManager.InstalledMods.Where(mod => mod.id == "olaycolay.thevinki").FirstOrDefault();
                var saveData = SaveDataExtension.GetSlugBaseData(rainWorld.progression.miscProgressionData);
                if (saveData.TryGet("VinkiVersion", out string modVersion))
                {
                    modChanged = vinkiMod.version != modVersion;
                    if (modChanged) VLogger.LogInfo("Vinki mod version changed!");
                }
                else
                {
                    VLogger.LogInfo("Didn't find saved vinki mod version");
                    modChanged = true;
                }
                VLogger.LogInfo("Setting vinki version to " + vinkiMod.version);
                saveData.Set("VinkiVersion", vinkiMod.version);
                rainWorld.progression.SaveProgression(false, true);
            }
            else
            {
                VLogger.LogInfo("Can't find vinki mod ID");
            }

            graffitiFolders = AssetManager.ListDirectory(baseGraffitiFolder, true, true, true);
            mainGraffitiFolder = AssetManager.ResolveDirectory(storyGraffitiFolder + "/../VinkiGraffiti");
            storyGraffitiFolder = AssetManager.ResolveDirectory(storyGraffitiFolder);

            VLogger.LogInfo("Graffiti folders: " + string.Join("; ", graffitiFolders));

            // If the graffiti folder doesn't exist (or is empty), copy it from the mod
            if (!Directory.Exists(mainGraffitiFolder) || !Directory.EnumerateDirectories(mainGraffitiFolder).Any() ||
                !Directory.Exists(mainGraffitiFolder + "/vinki") || !Directory.EnumerateFileSystemEntries(mainGraffitiFolder + "/vinki").Any() ||
                !Directory.Exists(mainGraffitiFolder + "/White") || !Directory.EnumerateFileSystemEntries(mainGraffitiFolder + "/White").Any() || modChanged)
            {
                if (!CopyGraffitiBackup())
                {
                    return;
                }
            }

            // Go through each graffiti image and add it to the list of decals Vinki can place
            LoadGraffiti();

            // Remix menu config
            VinkiConfig.RegisterOI();

            // Get sprite atlases
            Futile.atlasManager.LoadAtlas("atlases/SprayCan");
            Futile.atlasManager.LoadAtlas("atlases/glasses");
            Futile.atlasManager.LoadAtlas("atlases/rainpods");
            Futile.atlasManager.LoadAtlas("atlases/shoes");
            Futile.atlasManager.LoadAtlas("atlases/TagIcon");
            Futile.atlasManager.LoadAtlas("atlases/vinki_expedition");
            Futile.atlasManager.LoadImage("atlases/icon_SprayCan");
            Futile.atlasManager.LoadImage("decals/QUESTIONMARK");

            TailTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
            var tailTextureFile = AssetManager.ResolveFilePath("textures/VinkiTail.png");
            if (File.Exists(tailTextureFile))
            {
                var rawData = File.ReadAllBytes(tailTextureFile);
                TailTexture.LoadImage(rawData);
            }

            // Populate the colorfulItems List for crafting Spray Cans
            InitColorfulItems();

            // Populate dictionary of creatures that will spawn from spraying all the story graffiti in a room
            GraffitiCreatureSpawner.PopulateDictionary();
        }

        public static bool CopyGraffitiBackup()
        {
            string backupFolder = AssetManager.ResolveDirectory("decals/GraffitiBackup");
            if (!Directory.Exists(backupFolder))
            {
                VLogger.LogError("Could not find Vinki graffiti backup folder in workshop files or local mods!");
                return false;
            }
            VLogger.LogInfo("Graffiti folder doesn't exist! Copying from backup folder: " + backupFolder);
            CopyFilesRecursively(backupFolder, backupFolder + "/../VinkiGraffiti");
            mainGraffitiFolder = AssetManager.ResolveDirectory(storyGraffitiFolder + "/../VinkiGraffiti");
            return true;
        }

        public static void LoadGraffiti()
        {
            graffitiOffsets.Clear();
            graffitis.Clear();
            storyGraffitiRoomPositions.Clear();

            foreach (string scugGraffitiFolder in graffitiFolders)
            {
                foreach (var image in Directory.EnumerateFiles(scugGraffitiFolder, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".png")))
                {
                    AddGraffiti(image, new DirectoryInfo(scugGraffitiFolder).Name);
                }
            }

            //foreach (var kvp in graffitis)
            //{
            //    string values = string.Join(", ", kvp.Value);
            //    VLogger.LogInfo($"Key: {kvp.Key}, Values: {values}");
            //}
        }

        private static void AddGraffiti(string image, string slugcat, KeyValuePair<string, Vector2>? storyGraffitiRoomPos = null)
        {
            PlacedObject.CustomDecalData decal = new(null)
            {
                imageName = "VinkiGraffiti/" + slugcat + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(image),
                fromDepth = 0.2f
            };

            if (!graffitis.ContainsKey(slugcat))
            {
                graffitiOffsets[slugcat] = [];
                graffitis[slugcat] = [];
                graffitiAvgColors[slugcat] = [];
            }

            string filePath;
            if (storyGraffitiRoomPos.HasValue)
            {
                filePath = AssetManager.ResolveFilePath(storyGraffitiFolder + '/' + Path.GetFileNameWithoutExtension(image) + ".png");
                // If the image name can't be found, then it should just be in the normal decals folder
                if (File.Exists(filePath))
                {
                    decal.imageName = "StorySpoilers/" + Path.GetFileNameWithoutExtension(image);
                }
                else
                {
                    filePath = AssetManager.ResolveFilePath("decals/" + Path.GetFileNameWithoutExtension(image) + ".png");
                    decal.imageName = Path.GetFileNameWithoutExtension(image);
                }
                storyGraffitiRoomPositions.Add(graffitis["Story"].Count, storyGraffitiRoomPos.Value);
            }
            else
            {
                VLogger.LogInfo("Adding new reqular graffiti: " + baseGraffitiFolder + Path.DirectorySeparatorChar + slugcat + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(image) + ".png");
                filePath = AssetManager.ResolveFilePath(baseGraffitiFolder + Path.DirectorySeparatorChar + slugcat + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(image) + ".png");

                // Add to futile atlas for sprite to show in graffiti selector
                Futile.atlasManager.LoadImage("decals/" + decal.imageName);
            }

            // Get the image as a 2d texture so we can resize it to something manageable
            Texture2D img = new(2, 2);
            byte[] tmpBytes = File.ReadAllBytes(filePath);
            ImageConversion.LoadImage(img, tmpBytes);

            // Get average color of image (to use for graffiti spray/smoke color)
            graffitiAvgColors[slugcat].Add(AverageColorFromTexture(img));

            // Resize image to look good in game
            if (!storyGraffitiRoomPos.HasValue)
            {
                int[] newSize = ResizeAndKeepAspectRatio(img.width, img.height, 150f * 150f);
                img.Resize(newSize[0], newSize[1]);
            }

            decal.handles[0] = new Vector2(0f, img.height);
            decal.handles[1] = new Vector2(img.width, img.height);
            decal.handles[2] = new Vector2(img.width, 0f);

            float halfWidth = img.width / 2f;
            float halfHeight = img.height / 2f;
            graffitiOffsets[slugcat].Add(new Vector2(-halfWidth, -halfHeight));
            graffitis[slugcat].Add(decal);
        }

        public static void AddGraffitiObjectives()
        {
            JsonList json = JsonAny.Parse(File.ReadAllText(AssetManager.ResolveFilePath("VinkiObjectives.txt"))).AsList();
            foreach (JsonAny objective in json)
            {
                JsonObject obj = objective.AsObject();
                AddGraffiti(obj.GetString("name"), "Story", new(obj.GetString("room"), JsonUtils.ToVector2(obj["position"])));
            }
        }

        public static void AddSongTempos()
        {
            manualSongMsPerBeat = [];
            JsonList json = JsonAny.Parse(File.ReadAllText(AssetManager.ResolveFilePath("SongTempos.txt"))).AsList();
            foreach (JsonAny song in json)
            {
                JsonObject obj = song.AsObject();
                manualSongMsPerBeat.Add(obj.GetString("name"), obj.GetFloat("millisecondsPerBeat"));
            }
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
                ApplyMenuSceneHooks();
                On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;

                if (!isDebug)
                {
                    IL.Menu.IntroRoll.ctor += IntroRoll_ctor;
                }

                // Add the story graffitis
                AddGraffitiObjectives();

                // Add updates per beat for vanilla songs
                AddSongTempos();

                //-- You can have the DMS sprite setup in a separate method and only call it if DMS is loaded
                //-- With this the mod will still work even if DMS isn't installed
                if (ModManager.ActiveMods.Any(mod => mod.id == "dressmyslugcat"))
                {
                    SetupDMSSprites();
                }
            }
            catch (Exception ex)
            {
                VLogger.LogError(ex.Message);
            }

            improvedInput = ModManager.ActiveMods.Exists((mod) => mod.id == "improved-input-config");
            if (!improvedInput)
            {
                return;
            }

            try
            {
                SetImprovedInput();
            }
            catch
            {
                throw new Exception("Improved Input enabled but also not enabled???");
            }

            try
            {

                if (ModManager.ActiveMods.Exists((mod) => mod.id == "pushtomeow"))
                {
                    try
                    {
                        SetPushToMeow();
                    }
                    catch
                    {
                        throw new Exception("Push to Meow enabled but also not enabled???");
                    }
                }
            }
            catch (Exception ex)
            {
                VLogger.LogError(ex.Message);
            }
        }

        private static void SetupDMSSprites()
        {
            //-- The ID of the spritesheet we will be using as the default sprites for our slugcat
            var sheetID = "olaycolay.thevinki";

            //-- Each player slot (0, 1, 2, 3) can be customized individually
            for (int i = 0; i < 4; i++)
            {
                SpriteDefinitions.AddSlugcatDefault(new Customization()
                {
                    //-- Make sure to use the same ID as the one used for our slugcat
                    Slugcat = Enums.vinki.ToString(),
                    PlayerNumber = i,
                    CustomSprites =
                    [
                        //-- You can customize which spritesheet and color each body part will use
                        new() { Sprite = "HEAD", SpriteSheetID = sheetID + i },
                        //new CustomSprite() { Sprite = "FACE", SpriteSheetID = sheetID + i },
                        new() { Sprite = "BODY", SpriteSheetID = sheetID + i },
                        new() { Sprite = "ARMS", SpriteSheetID = sheetID + i },
                        new() { Sprite = "HIPS", SpriteSheetID = sheetID + i },
                        new() { Sprite = "LEGS", SpriteSheetID = sheetID + i },
                        new() { Sprite = "TAIL", SpriteSheetID = sheetID + i }
                    ],
                });
            }
        }

        public static void SetPushToMeow()
        {
            new Hook(typeof(PushToMeowMain).GetMethod(nameof(PushToMeowMain.DoMeow)), PushToMeowMain_DoMeow);
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
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.NeedleEgg, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.PuffBall, 1);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, 3);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.SlimeMold, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.SporePlant, 1);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.WaterNut, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, 2);
            colorfulItems.Add(AbstractPhysicalObject.AbstractObjectType.PebblesPearl, 3);

            if (!ModManager.MSC)
            {
                return;
            }

            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.DandelionPeach, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.FireEgg, 3);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.Germinator, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.GlowWeed, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.GooieDuck, 2);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.LillyPuck, 1);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.Seed, 1);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, 9001);
            colorfulItems.Add(MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl, 3);
        }

        private static int[] ResizeAndKeepAspectRatio(float original_width, float original_height, float target_area)
        {
            float new_width = Mathf.Sqrt((original_width / original_height) * target_area);
            float new_height = target_area / new_width;

            int w = Mathf.RoundToInt(new_width); // round to the nearest integer
            int h = Mathf.RoundToInt(new_height - (w - new_width)); // adjust the rounded width with height 

            return [w, h];
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
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

        private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            if (ID == Enums.FullscreenVideo)
            {
                self.currentMainLoop = new FullscreenVideo(self);
            }
            orig(self, ID);
        }

        private static void IntroRoll_ctor(ILContext il)
        {
            var cursor = new ILCursor(il);
            int localVarNum = 0;

            if (cursor.TryGotoNext(i => i.MatchNewarr<string>())
                && cursor.TryGotoNext(MoveType.After, i => i.MatchStloc(out localVarNum)))
            {
                cursor.Emit(OpCodes.Ldloc, localVarNum);
                cursor.EmitDelegate<Func<string[], string[]>>((oldTitleImages) => [.. oldTitleImages, "vinki_0", "vinki_1"]);
                cursor.Emit(OpCodes.Stloc, localVarNum);
                //cursor.Emit(OpCodes.Ldloc, localVarNum);
                //cursor.EmitDelegate<Action<string[]>>((oldTitleImages) =>
                //{
                //    VLogger.LogInfo("Title screens (" + oldTitleImages.Length + "): " + string.Join(", ", oldTitleImages));
                //});
            }
        }

        public static bool AllGraffitiUnlocked()
        {
            // Return true if all of the graffiti in Unlockables is contained within VinkiGraffiti
            string[] files1 = Directory.GetFiles(AssetManager.ResolveDirectory("decals/Unlockables"), "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToArray();
            string[] files2 = Directory.GetFiles(AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki"), "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToArray();

            IEnumerable<string> difference = files1.Except(files2);

            if (!difference.Any())
            {
                VLogger.LogInfo("All files in Unlockables are also contained in VinkiGraffiti/vinki.");
                return true;
            }
            else
            {
                VLogger.LogInfo("The following files in Unlockables are not contained in VinkiGraffiti/vinki:");
                foreach (string file in difference)
                {
                    VLogger.LogInfo(file);
                }
                return false;
            }
        }

        public static bool AnyGraffitiUnlocked()
        {
            // Return true if any of the graffiti in Unlockables is contained within VinkiGraffiti
            string[] sourceFiles = Directory.GetFiles(AssetManager.ResolveDirectory("decals/Unlockables"), "*", SearchOption.AllDirectories);
            string[] targetFiles = Directory.GetFiles(AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki"), "*", SearchOption.AllDirectories);

            IEnumerable<string> commonFiles = sourceFiles.Select(Path.GetFileName).Intersect(targetFiles.Select(Path.GetFileName));

            if (commonFiles.Any())
            {
                VLogger.LogInfo("The following files are present in both directories:");
                foreach (string file in commonFiles)
                {
                    VLogger.LogInfo(file);
                }
                return true;
            }
            else
            {
                VLogger.LogInfo("No common files found.");
                return false;
            }
        }
    }
}