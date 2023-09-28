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
    [BepInPlugin(MOD_ID, "The Vinki", "0.8.14.1")]
    class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "olaycolay.thevinki";
        public static int lastXDirection = 1;
        public static int lastYDirection = 1;
        public static int craftCounter = 0;
        public static int storyGraffitiCount = 0;
        public static int[] vineGrindDelay = { 
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        };
        public static bool grindUpPoleFlag = false;
        public static bool isGrindingH = false;
        public static bool isGrindingV = false;
        public static bool isGrindingNoGrav = false;
        public static bool isGrindingVine = false;
        public static bool isGrinding = false;
        public static bool[] grindToggle = { 
            false, false, false, false,
            false, false, false, false,
            false, false, false, false,
            false, false, false, false
        };
        public static bool sleeping;
        public static bool introPlayed = false;
        public static Vector2 lastVineDir = Vector2.zero;
        public static ClimbableVinesSystem.VinePosition[] vineAtFeet = new ClimbableVinesSystem.VinePosition[16];
        public static Player.AnimationIndex[] lastAnimationFrame = { 
            Player.AnimationIndex.None, Player.AnimationIndex.None, Player.AnimationIndex.None, Player.AnimationIndex.None,
            Player.AnimationIndex.None, Player.AnimationIndex.None, Player.AnimationIndex.None, Player.AnimationIndex.None,
            Player.AnimationIndex.None, Player.AnimationIndex.None, Player.AnimationIndex.None, Player.AnimationIndex.None,
            Player.AnimationIndex.None, Player.AnimationIndex.None, Player.AnimationIndex.None, Player.AnimationIndex.None
        };
        public static Player.AnimationIndex[] lastAnimation = lastAnimationFrame.Clone() as Player.AnimationIndex[];
        public static ChunkSoundEmitter grindSound;
        public static Dictionary<string, List<PlacedObject.CustomDecalData>> graffitis = new();
        public static Dictionary<string, List<Vector2>> graffitiOffsets = new();
        public static Dictionary<string, List<Color>> graffitiAvgColors = new();
        public static List<string> shelterItems = new List<string>();
        public static Dictionary<int, KeyValuePair<string, Vector2>> storyGraffitiRoomPositions = new();
        public static Dictionary<AbstractPhysicalObject.AbstractObjectType, int> colorfulItems = new Dictionary<AbstractPhysicalObject.AbstractObjectType, int>();
        public static Texture2D TailTexture;
        public static Color?[][] jollyColors = new Color?[16][] { 
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6],
            new Color?[6], new Color?[6], new Color?[6], new Color?[6]
        };
        public static string graffitiFolder = "decals/VinkiGraffiti";
        public static string storyGraffitiFolder = "decals/StorySpoilers";
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
        public static readonly PlayerKeybind Graffiti = PlayerKeybind.Register("thevinki:graffiti", "The Vinki", "Graffiti Mode", KeyCode.UpArrow, KeyCode.JoystickButton8);
        public static readonly PlayerKeybind Spray = PlayerKeybind.Register("thevinki:spray", "The Vinki", "Spray Graffiti", KeyCode.LeftControl, KeyCode.JoystickButton3);
        public static readonly PlayerKeybind Craft = PlayerKeybind.Register("thevinki:craft", "The Vinki", "Craft Spray Can", KeyCode.LeftShift, KeyCode.JoystickButton2);

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