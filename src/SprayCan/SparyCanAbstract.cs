using Fisobs.Core;
using IL.RWCustom;
using UnityEngine;

namespace SprayCans;

sealed class SprayCanAbstract : AbstractPhysicalObject
{
    public static Color[] CanColors = new Color[6]
    {
        new Color32(120,120,120,255),
        new Color32(225,23,103,255),
        new Color32(249,99,0,255),
        new Color32(255,214,0,255),
        new Color32(30,212,75,255),
        new Color32(9,122,247,255)
    };

    public SprayCanAbstract(World world, WorldCoordinate pos, EntityID ID, int uses = 3) : base(world, SprayCanFisob.SprayCan, null, pos, ID)
    {
        this.uses = uses;
        scaleX = 0.6f;
        scaleY = 0.6f;
        if (uses <= 9000)
        {
            Color.RGBToHSV(CanColors[uses], out hue, out saturation, out _);
        }
        else
        {
            hue = 0f;
            saturation = 0.9f;
        }
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null)
            realizedObject = new SprayCan(this, Room.realizedRoom.MiddleOfTile((pos != null) ? pos.Tile : new RWCustom.IntVector2(0,0)), Vector2.zero);
    }

    public float hue;
    public float saturation;
    public float scaleX;
    public float scaleY;
    public int uses;

    public override string ToString()
    {
        return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY};{uses}");
    }
}