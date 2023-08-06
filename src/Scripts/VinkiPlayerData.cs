using RWCustom;
using SlugBase.DataTypes;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;

public class VinkiPlayerData
{
    public readonly bool IsVinki;

    public int tailStripesSprite;
    public int shoesSprite;
    public int rainPodsSprite;
    public int glassesSprite;

    public WeakReference<Player> playerRef;
    public Color BodyColor;
    public Color EyesColor;
    public Color StripesColor;
    public Color ShoesColor;
    public Color RainPodsColor;
    public Color GlassesColor;

    public FAtlas TailAtlas;

    public VinkiPlayerData(Player player)
    {
        IsVinki = player.slugcatStats.name == Enums.TheVinki;

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
}

public static class PlayerExtension
{
    private static readonly ConditionalWeakTable<Player, VinkiPlayerData> _cwt = new();

    public static VinkiPlayerData Vinki(this Player player) => _cwt.GetValue(player, _ => new VinkiPlayerData(player));

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