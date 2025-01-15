using ImprovedInput;
using RWCustom;
using SlugBase.DataTypes;
using Smoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;

public class VinkiPlayerData
{
    public class PoisonedCreature(Creature creature, int timeLeft, float totalDamage)
    {
        public Creature creature = creature;
        public int timeLeft = timeLeft;
        public float damagePerTick = totalDamage / timeLeft / creature.Template.baseDamageResistance;
    }

    public readonly bool IsVinki;

    public FAtlas TailAtlas;

    public int shoesSprite;
    public int rainPodsSprite;
    public int glassesSprite;
    public int tagIconSprite;
    public int stripesSprite;
    public int tagLag = 0;
    public float tagIconSize = 0f;

    public WeakReference<Player> playerRef;
    public Color BodyColor;
    public Color EyesColor;
    public Color StripesColor;
    public Color ShoesColor;
    public Color RainPodsColor;
    public Color GlassesColor;

    public int lastXDirection = 1;
    public int lastYDirection = 1;
    public int craftCounter = 0;
    public int vineGrindDelay = 0;
    public bool grindUpPoleFlag = false;
    public bool grindToggle = false;
    public int canCoyote = 0;
    public int idleUpdates = 0;
    public float beatTimer = 0f;

    public bool isGrindingH = false;
    public bool isGrindingV = false;
    public bool isGrindingNoGrav = false;
    public bool isGrindingVine = false;
    public bool isGrinding = false;

    public bool lastIsGrindingH = false;
    public bool lastIsGrindingV = false;
    public bool lastIsGrindingNoGrav = false;
    public bool lastIsGrindingVine = false;
    public bool lastIsGrinding = false;

    public Vector2 lastVineDir = Vector2.zero;
    public ClimbableVinesSystem.VinePosition vineAtFeet = null;
    public Player.AnimationIndex lastAnimationFrame = Player.AnimationIndex.None;
    public Player.AnimationIndex lastAnimation = Player.AnimationIndex.None;
    public ChunkSoundEmitter grindSound;
    public BodyChunk tagableBodyChunk = null;
    public Creature tagableCreature = null;
    public List<PoisonedCreature> poisonedVictims = [];
    public TagSmoke tagSmoke = null;

    // Combo stuff
    public bool fastComboTimer = false;
    public int timeLeftInCombo = 0;
    public int comboSize = 0;
    public Dictionary<string, List<IEnumerable<Room.Tile>>> beamsInCombo = [];
    public string currentTrickName = string.Empty;
    public int currentTrickScore = 0;
    public int comboTotalScore;
    public Action OnNewTrick;

    public VinkiPlayerData(Player player)
    {
        IsVinki = player.slugcatStats.name == Enums.vinki;

        playerRef = new WeakReference<Player>(player);

        if (!IsVinki)
        {
            return;
        }
    }

    public void LoadTailAtlas()
    {
        var tailTexture = new Texture2D(Plugin.TailTexture.width, Plugin.TailTexture.height, TextureFormat.ARGB32, false);
        Graphics.CopyTexture(Plugin.TailTexture, tailTexture);

        Plugin.MapTextureColor(tailTexture, 255, StripesColor, false);

        if (playerRef.TryGetTarget(out var player))
        {
            TailAtlas = Futile.atlasManager.LoadAtlasFromTexture("vinkitailtexture_" + player.playerState.playerNumber + Time.time + UnityEngine.Random.value, tailTexture, false);
        }
    }

    public void SetupColors(PlayerGraphics pg)
    {
        BodyColor = pg.GetColor(Enums.Color.Body) ?? Custom.hexToColor("d2701d");
        EyesColor = pg.GetColor(Enums.Color.Eyes) ?? Custom.hexToColor("0E0202");
        StripesColor = pg.GetColor(Enums.Color.TailStripes) ?? Custom.hexToColor("494ed3");
        ShoesColor = pg.GetColor(Enums.Color.Shoes) ?? Custom.hexToColor("494ed3");
        RainPodsColor = pg.GetColor(Enums.Color.RainPods) ?? Custom.hexToColor("FFFFFF");
        GlassesColor = pg.GetColor(Enums.Color.Glasses) ?? Custom.hexToColor("0E0202");
    }

    public void AddCombo()
    {
        fastComboTimer = false;
        comboSize++;
        timeLeftInCombo = 400;
    }

    public bool AddBeamToCombo(Room room, Room.Tile tile)
    {
        IEnumerable<Room.Tile> beam = [ tile ];
        if (this.isGrindingH && tile.horizontalBeam)
        {
            int y = tile.Y;
            // Add all horizontal beam tiles to the left
            for (int x = tile.X; x >= 0 && room.Tiles[x, y].horizontalBeam; x--)
            {
                beam = beam.Prepend(room.Tiles[x, y]);
            }
            // Add all horizontal beam tiles to the right
            for (int x = tile.X; x < room.Tiles.GetLength(0) && room.Tiles[x, y].horizontalBeam; x++)
            {
                beam = beam.Append(room.Tiles[x, y]);
            }
        }
        else if (this.isGrindingV && tile.verticalBeam)
        {
            int x = tile.X;
            // Add all vertical beam tiles below
            for (int y = tile.Y; y >= 0 && room.Tiles[x, y].verticalBeam; y--)
            {
                beam = beam.Prepend(room.Tiles[x, y]);
            }
            // Add all vertical beam tiles above
            for (int y = tile.Y; y < room.Tiles.GetLength(1) && room.Tiles[x, y].verticalBeam; y++)
            {
                beam = beam.Append(room.Tiles[x, y]);
            }
        }

        string roomName = room.abstractRoom.name;
        if (beamsInCombo.ContainsKey(roomName))
        {
            if (beamsInCombo[roomName].Any((IEnumerable<Room.Tile> oldBeam) => oldBeam.First() == beam.First() && oldBeam.Last() == beam.Last()))
            {
                fastComboTimer = false;
                return false;
            }
            else
            {
                AddCombo();
                beamsInCombo[roomName].Add(beam);

                return true;
            }
        }
        else
        {
            AddCombo();
            beamsInCombo.Add(roomName, [ beam ]);

            return true;
        }
    }

    public void NewTrick(Enums.TrickType type, bool comboAdded = false, bool fakie = false)
    {
        int rand = 0;
        string prefix = string.Empty;
        string suffix = string.Empty;

        switch (type)
        {
            case Enums.TrickType.Flip:
                rand = fakie ? UnityEngine.Random.Range(2, 4) : UnityEngine.Random.Range(0, 2);
                this.currentTrickScore = 0;
                break;
            case Enums.TrickType.HorizontalBeam:
            case Enums.TrickType.VerticalBeam:
            case Enums.TrickType.Vine:
                if (fakie)
                {
                    prefix = "Fakie ";
                }
                goto case Enums.TrickType.ZeroGravity;
            case Enums.TrickType.ZeroGravity:
                if (comboAdded)
                {
                    suffix = " (New)";
                }
                this.currentTrickScore = 0;
                goto default;
            default:
                rand = UnityEngine.Random.Range(0, Enums.trickNames[type].Count());
                break;
        }
        this.currentTrickName = prefix + Enums.trickNames[type][rand] + suffix;

        this.OnNewTrick.Invoke();
    }
}

public static class PlayerExtension
{
    private static readonly ConditionalWeakTable<Player, VinkiPlayerData> cwt = new();

    public static VinkiPlayerData Vinki(this Player player) => cwt.GetValue(player, _ => new VinkiPlayerData(player));

    public static Color? GetColor(this PlayerGraphics pg, PlayerColor color) => color.GetColor(pg);

    //public static Color? GetColor(this Player player, PlayerColor color) => (player.graphicsModule as PlayerGraphics)?.GetColor(color);

    public static Player Get(this WeakReference<Player> weakRef)
    {
        weakRef.TryGetTarget(out var result);
        return result;
    }

    public static PlayerGraphics PlayerGraphics(this Player player) => (PlayerGraphics)player.graphicsModule;

    public static TailSegment[] Tail(this Player player) => player.PlayerGraphics().tail;

    public static bool IsVinki(this Player player) => player.Vinki().IsVinki;

    public static bool IsVinki(this Player player, out VinkiPlayerData Vinki)
    {
        Vinki = player.Vinki();
        return Vinki.IsVinki;
    }
    
    public static bool IsPressed(this Player player, int index)
    {
        if (Plugin.improvedInput)
        {
            try {
                return player.IsPressedImprovedInput(index);
            } catch {
                throw new Exception("Could not find ImprovedInput!");
            }
        }

        return index switch
        {
            0 or 4 => player.input[0].pckp,
            1 => false,
            2 => player.input[0].y == 1,
            3 or 5 => player.input[0].jmp && player.input[0].pckp,
            _ => false,
        };
    }
    private static bool IsPressedImprovedInput(this Player player, int index)
    {
        return player.IsPressed((PlayerKeybind)Plugin.improvedControls.GetValue(index));
    }

    public static bool JustPressed(this Player player, int index)
    {
        if (Plugin.improvedInput)
        {
            try
            {
                return player.JustPressedImprovedInput(index);
            }
            catch
            {
                throw new Exception("Could not find ImprovedInput!");
            }
        }

        return index switch
        {
            0 or 4 => player.input[0].pckp && !player.input[1].pckp,
            1 => false,
            2 => player.input[0].y == 1 && player.input[1].y != 1,
            3 or 5 => (player.wantToJump > 0 || (player.dangerGrasp != null && player.dangerGraspTime < 30)) && RWInput.PlayerInput(player.playerState.playerNumber).pckp,
            _ => false,
        };
    }
    private static bool JustPressedImprovedInput(this Player player, int index)
    {
        // Able to tag while caught
        if (index == 5)
        {
            return player.RawInput()[(PlayerKeybind)Plugin.improvedControls.GetValue(index)];
        }
        return player.JustPressed((PlayerKeybind)Plugin.improvedControls.GetValue(index));
    }
}