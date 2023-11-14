using RWCustom;
using SlugBase.DataTypes;
using Smoke;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;

public class VinkiPlayerData
{
    public class PoisonedCreature
    {
        public Creature creature;
        public int timeLeft;
        public float damagePerTick;

        public PoisonedCreature(Creature creature, int timeLeft, float totalDamage)
        {
            this.creature = creature;
            this.timeLeft = timeLeft;
            damagePerTick = totalDamage / timeLeft / creature.Template.baseDamageResistance;
        }
    }

    public readonly bool IsVinki;

    public int tailStripesSprite;
    public int shoesSprite;
    public int rainPodsSprite;
    public int glassesSprite;
    public int tagIconSprite;
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
    public bool isGrindingH = false;
    public bool isGrindingV = false;
    public bool isGrindingNoGrav = false;
    public bool isGrindingVine = false;
    public bool isGrinding = false;
    public bool grindToggle = false;
    public int canCoyote = 0;

    public Vector2 lastVineDir = Vector2.zero;
    public ClimbableVinesSystem.VinePosition vineAtFeet = null;
    public Player.AnimationIndex lastAnimationFrame = Player.AnimationIndex.None;
    public Player.AnimationIndex lastAnimation = Player.AnimationIndex.None;
    public ChunkSoundEmitter grindSound;
    public Creature tagableCreature = null;
    public List<PoisonedCreature> poisonedVictims = new();
    public TagSmoke tagSmoke = null;

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
        Plugin.MapTextureColor(tailTexture, 0, BodyColor);

        if (playerRef.TryGetTarget(out var player))
        {
            Plugin.TailAtlas = Futile.atlasManager.LoadAtlasFromTexture("vinkitailtexture_" + player.playerState.playerNumber + Time.time + UnityEngine.Random.value, tailTexture, false);
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
}