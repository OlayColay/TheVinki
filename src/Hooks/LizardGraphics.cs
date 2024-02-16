using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
	private static void ApplyLizardGraphicsHooks()
	{
		On.LizardGraphics.InitiateSprites += LizardGraphics_InitiateSprites;
        On.LizardGraphics.DrawSprites += LizardGraphics_DrawSprites;
	}

    private static void LizardGraphics_InitiateSprites(On.LizardGraphics.orig_InitiateSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (self.debugVisualization)
        {
            return;
        }

        List<int> affectedSprites = new(Enumerable.Range(0, sLeaser.sprites.Length));
        //affectedSprites.RemoveRange(self.SpriteHeadStart, self.SpriteHeadEnd - self.SpriteHeadStart);

        self.Tag().affectedSprites = affectedSprites.ToArray();
    }

    private static void LizardGraphics_DrawSprites(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        UpdateTagColors(self.Tag(), sLeaser, 0.3f);
    }
}
