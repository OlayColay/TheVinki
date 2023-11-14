using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Collections.Generic;
using ImprovedInput;
using IL.Menu.Remix;

namespace Vinki
{
    [BepInDependency("slime-cubed.slugbase")]
    [BepInDependency("dressmyslugcat", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(MOD_ID, "The Vinki", "0.9.12")]
    class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "olaycolay.thevinki";
        
        public static bool introPlayed = false;
        public static bool[] storyGraffitisSprayed = { 
            false, false, false
        };
        public static bool[] storyGraffitisOnMap = {
            false, false, false
        };
        public static FAtlas TailAtlas;
        public static Texture2D TailTexture;
        public static Dictionary<string, List<PlacedObject.CustomDecalData>> graffitis = new();
        public static Dictionary<string, List<Vector2>> graffitiOffsets = new();
        public static Dictionary<string, List<Color>> graffitiAvgColors = new();
        public static List<string> shelterItems = new List<string>();
        public static Dictionary<int, KeyValuePair<string, Vector2>> storyGraffitiRoomPositions = new();
        public static Dictionary<AbstractPhysicalObject.AbstractObjectType, int> colorfulItems = new Dictionary<AbstractPhysicalObject.AbstractObjectType, int>();
        public static string graffitiFolder = "decals/VinkiGraffiti";
        public static string storyGraffitiFolder = "decals/StorySpoilers";
        public static int storyGraffitiCount = 0;
        public static bool sleeping = false;
        public static Color?[][] jollyColors = new Color?[16][] {
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6]
        };


        public static readonly PlayerFeature<float> CoyoteBoost = PlayerFloat("thevinki/coyote_boost");
        public static readonly PlayerFeature<float> GrindXSpeed = PlayerFloat("thevinki/grind_x_speed");
        public static readonly PlayerFeature<float> GrindVineSpeed = PlayerFloat("thevinki/grind_vine_speed");
        public static readonly PlayerFeature<float> GrindYSpeed = PlayerFloat("thevinki/grind_y_speed");
        public static readonly PlayerFeature<float> NormalXSpeed = PlayerFloat("thevinki/normal_x_speed");
        public static readonly PlayerFeature<float> NormalYSpeed = PlayerFloat("thevinki/normal_y_speed");
        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("thevinki/super_jump");
        public static readonly PlayerFeature<Color> SparkColor = PlayerColor("thevinki/spark_color");

        public static readonly PlayerKeybind Grind = PlayerKeybind.Register("thevinki:grind", "The Vinki", "Grind", KeyCode.LeftShift, KeyCode.JoystickButton2);
        public static readonly PlayerKeybind ToggleGrind = PlayerKeybind.Register("thevinki:toggle_grind", "The Vinki", "Toggle Grind", KeyCode.None, KeyCode.None);
        public static readonly PlayerKeybind Graffiti = PlayerKeybind.Register("thevinki:graffiti", "The Vinki", "Graffiti Mode", KeyCode.UpArrow, KeyCode.JoystickButton4);
        public static readonly PlayerKeybind Spray = PlayerKeybind.Register("thevinki:spray", "The Vinki", "Spray Graffiti", KeyCode.LeftControl, KeyCode.JoystickButton3);
        public static readonly PlayerKeybind Craft = PlayerKeybind.Register("thevinki:craft", "The Vinki", "Craft Spray Can", KeyCode.LeftShift, KeyCode.JoystickButton2);
        public static readonly PlayerKeybind Tag = PlayerKeybind.Register("thevinki:tag", "The Vinki", "Tag Creatures", KeyCode.LeftControl, KeyCode.JoystickButton3);

        // Add hooks
        public void OnEnable()
        {
            Hooks.ApplyInit();
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