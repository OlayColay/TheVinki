﻿using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Collections.Generic;
using ImprovedInput;
using System;
using Menu;
using BepInEx.Logging;

namespace Vinki
{
    [BepInDependency("slime-cubed.slugbase")]
    [BepInDependency("dressmyslugcat", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(MOD_ID, "The Vinki", "0.12.5.3")]
    class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "olaycolay.thevinki";

        public static bool debugMode = false;
        
        public static bool introPlayed = false;
        public static int[] storyGraffitisSprayed = [];
        public static int[] storyGraffitisOnMap = [];
        public static Texture2D TailTexture;
        public static Dictionary<string, List<PlacedObject.CustomDecalData>> graffitis = [];
        public static Dictionary<string, List<Vector2>> graffitiOffsets = [];
        public static Dictionary<string, List<Color>> graffitiAvgColors = [];
        public static List<string> shelterItems = [];
        public static Dictionary<int, KeyValuePair<string, Vector2>> storyGraffitiRoomPositions = [];
        public static Dictionary<AbstractPhysicalObject.AbstractObjectType, int> colorfulItems = [];
        public static string graffitiFolder = "decals/VinkiGraffiti";
        public static string storyGraffitiFolder = "decals/StorySpoilers";
        public static int storyGraffitiCount = 0;
        public static bool sleeping = false;
        public static Color?[][] jollyColors = [
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6]
        ];
        public static List<MenuDepthIllustration> rotatingGhost = [];

        public static bool improvedInput;
        public static Array improvedControls;
        public static readonly int Grind = 0, ToggleGrind = 1, Graffiti = 2, Spray = 3, Craft = 4, Tag = 5;

        public static int[] queuedGNums = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
        public static bool[] repeatGraffiti = [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false];

        public static ManualLogSource VLogger;

        // Add hooks
        public void OnEnable()
        {
            VLogger = Logger;
            VLogger.LogInfo("OnEnable\n" + StackTraceUtility.ExtractStackTrace());
            Hooks.ApplyInit();
        }

        public void OnDisable()
        {
            VLogger.LogInfo("OnDisable\n" + StackTraceUtility.ExtractStackTrace());
            if (debugMode) {
                Hooks.RemoveHooks();
            };
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
    }
}