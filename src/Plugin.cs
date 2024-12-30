using BepInEx;
using UnityEngine;
using System.Collections.Generic;
using ImprovedInput;
using System;
using Menu;
using SlugBase.SaveData;
using System.Linq;
using System.IO;
using BepInEx.Logging;

namespace Vinki
{
    [BepInDependency("slime-cubed.slugbase")]
    [BepInDependency("dressmyslugcat", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("improved-input-config", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("pushtomeow", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(MOD_ID, "The Vinki", "0.12.17")]
    class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "olaycolay.thevinki";

        public static bool restartMode = false;
        public static bool isDebug = false;
        
        public static bool introPlayed = false;
        public static int[] storyGraffitisOnMap = [];
        public static Texture2D TailTexture;
        public static Dictionary<string, List<PlacedObject.CustomDecalData>> graffitis = [];
        public static Dictionary<string, List<Vector2>> graffitiOffsets = [];
        public static Dictionary<string, List<Color>> graffitiAvgColors = [];
        public static List<string> shelterItems = [];
        public static Dictionary<int, KeyValuePair<string, Vector2>> storyGraffitiRoomPositions = [];
        public static Dictionary<AbstractPhysicalObject.AbstractObjectType, int> colorfulItems = [];
        public static string[] graffitiFolders;
        public static string baseGraffitiFolder = "decals" + Path.DirectorySeparatorChar + "VinkiGraffiti";
        public static string mainGraffitiFolder = baseGraffitiFolder;
        public static string storyGraffitiFolder = "decals" + Path.DirectorySeparatorChar + "StorySpoilers";
        public static bool sleeping = false;
        public static Color?[][] jollyColors = [
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6]
        ];
        public static List<MenuDepthIllustration> rotatingGhost = [];
        public static bool diedLastCycle = false;
        public static int blueCycles = 0;

        public static bool improvedInput;
        public static Array improvedControls;
        public static readonly int Grind = 0, ToggleGrind = 1, Graffiti = 2, Spray = 3, Craft = 4, Tag = 5;

        public static int[] queuedGNums = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
        public static bool[] repeatGraffiti = [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false];

        public static Dictionary<string, float> manualSongMsPerBeat;
        public static float curMsPerBeat = 0;
        public static AudioSource curAudioSource;
        public static string curPlayingSong;

        public static List<string> catMaidGraffitis = ["Beep - 5P or QT", "Tsuno - Loud Pebbles"];

        public static ManualLogSource VLogger;

        // Add hooks
        public void OnEnable()
        {
            VLogger = Logger;
            //VLogger.LogInfo("OnEnable\n" + StackTraceUtility.ExtractStackTrace());
            Hooks.ApplyInit();
        }

        public void OnDisable()
        {
            //VLogger.LogInfo("OnDisable\n" + StackTraceUtility.ExtractStackTrace());
            if (restartMode) {
                Hooks.RemoveHooks();
            };
        }

        public void Start()
        {
            isDebug = File.ReadAllText(Path.Combine(Application.dataPath, "boot.config")).Contains("player-connection-debug=1");
        }

        public static void SetImprovedInput()
        {
            improvedControls = Array.CreateInstance(typeof(PlayerKeybind), 6);
            improvedControls.SetValue(PlayerKeybind.Register("thevinki:grind", "The Vinki", "Grind", KeyCode.LeftShift, KeyCode.JoystickButton2), Grind);
            improvedControls.SetValue(PlayerKeybind.Register("thevinki:toggle_grind", "The Vinki", "Toggle Grind", KeyCode.None, KeyCode.None), ToggleGrind);
            improvedControls.SetValue(PlayerKeybind.Register("thevinki:graffiti", "The Vinki", "Graffiti Mode", KeyCode.UpArrow, KeyCode.JoystickButton4), Graffiti);
            improvedControls.SetValue(PlayerKeybind.Register("thevinki:spray", "The Vinki", "Spray Graffiti", KeyCode.LeftControl, KeyCode.JoystickButton3), Spray);
            improvedControls.SetValue(PlayerKeybind.Register("thevinki:craft", "The Vinki", "Craft Spray Can", KeyCode.LeftShift, KeyCode.JoystickButton2), Craft);
            improvedControls.SetValue(PlayerKeybind.Register("thevinki:tag", "The Vinki", "Tag Creatures", KeyCode.LeftControl, KeyCode.JoystickButton3), Tag);

            // Hide conflict warning for certain bindings
            ((PlayerKeybind)improvedControls.GetValue(Grind)).HideConflict = (_) => true;
            ((PlayerKeybind)improvedControls.GetValue(Graffiti)).HideConflict = (x) => x != ((PlayerKeybind)improvedControls.GetValue(Spray)) && x != ((PlayerKeybind)improvedControls.GetValue(Craft)) && x != ((PlayerKeybind)improvedControls.GetValue(Tag));
            ((PlayerKeybind)improvedControls.GetValue(Spray)).HideConflict = (x) => x != ((PlayerKeybind)improvedControls.GetValue(Graffiti)) && x != ((PlayerKeybind)improvedControls.GetValue(Craft));
            ((PlayerKeybind)improvedControls.GetValue(Craft)).HideConflict = (x) => x != ((PlayerKeybind)improvedControls.GetValue(Spray)) && x != ((PlayerKeybind)improvedControls.GetValue(Graffiti)) && x != ((PlayerKeybind)improvedControls.GetValue(Tag));
            ((PlayerKeybind)improvedControls.GetValue(Tag)).HideConflict = (x) => x != ((PlayerKeybind)improvedControls.GetValue(Craft)) && x != ((PlayerKeybind)improvedControls.GetValue(Graffiti));
        }

        public static void MapTextureColor(Texture2D texture, int alpha, Color32 to, bool apply = true)
        {
            var colors = texture.GetPixels32();

            for (var i = 0; i < colors.Length; i++)
            {
                if (colors[i].a == alpha)
                {
                    colors[i] = to;
                }
            }

            texture.SetPixels32(colors);

            if (apply)
            {
                texture.Apply(false);
            }
        }

        public static bool FirstStoryGraffitisDone(SlugBaseSaveData miscWorldSave)
        {
            if (miscWorldSave.TryGet("StoryGraffitisSprayed", out int[] sprd))
            {
                return Enumerable.Range(4, 7).All(i => sprd.Contains(i));
            }
            return false;
        }

        public static bool CCStoryGraffitisDone(SlugBaseSaveData miscWorldSave)
        {
            if (miscWorldSave.TryGet("StoryGraffitisSprayed", out int[] sprd))
            {
                return Enumerable.Range(11, 4).All(i => sprd.Contains(i));
            }
            return false;
        }

        public static bool EndStoryGraffitisDone(SlugBaseSaveData miscWorldSave)
        {
            if (miscWorldSave.TryGet("StoryGraffitisSprayed", out int[] sprd))
            {
                return Enumerable.Range(15, 4).All(i => sprd.Contains(i));
            }
            return false;
        }
    }
}