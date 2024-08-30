using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
    private static void ApplySnailGraphicsHooks()
    {
        On.SnailGraphics.InitiateSprites += SnailGraphics_InitiateSprites;
        On.SnailGraphics.DrawSprites += SnailGraphics_DrawSprites;
    }
    private static void RemoveSnailGraphicsHooks()
    {
        On.SnailGraphics.InitiateSprites -= SnailGraphics_InitiateSprites;
        On.SnailGraphics.DrawSprites -= SnailGraphics_DrawSprites;
    }

    private static void SnailGraphics_InitiateSprites(On.SnailGraphics.orig_InitiateSprites orig, SnailGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        List<int> affectedSprites = [6, 7];
        //affectedSprites.RemoveRange(self.SpriteHeadStart, self.SpriteHeadEnd - self.SpriteHeadStart);

        self.Tag().affectedSprites = [.. affectedSprites];
    }

    private static void SnailGraphics_DrawSprites(On.SnailGraphics.orig_DrawSprites orig, SnailGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        UpdateTagColors(self.Tag(), sLeaser, 0.3f);
    }
}
