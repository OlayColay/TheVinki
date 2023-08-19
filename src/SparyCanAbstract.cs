using Fisobs.Core;
using UnityEngine;

namespace SprayCans;

sealed class SprayCanAbstract : AbstractPhysicalObject
{
    public SprayCanAbstract(World world, WorldCoordinate pos, EntityID ID, int uses = 3) : base(world, SprayCanFisob.SprayCan, null, pos, ID)
    {
        this.uses = uses;
        scaleX = 0.6f;
        scaleY = 0.6f;
        saturation = 0.5f;
        hue = 1f;
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null)
            realizedObject = new SprayCan(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
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