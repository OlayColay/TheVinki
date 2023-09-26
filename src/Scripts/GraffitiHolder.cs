using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vinki;

public partial class GraffitiHolder : UpdatableAndDeletable, IDrawable
{
	public Shape shape;
    public float[,] directionsPower = new float[12, 3];

    public GraffitiHolder(PlacedObject.CustomDecalData graffitiData, KeyValuePair<string, Vector2> graffitiPosition)
	{
		shape = new(null, Shape.ShapeType.Main, graffitiPosition.Value, 0f, 0f);
	}

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
    }

    internal void Update()
    {
        shape.Update(UnityEngine.Random.value < 0.05f, 0f, 1f, Vector2.zero, 0f, ref directionsPower);
    }
}
