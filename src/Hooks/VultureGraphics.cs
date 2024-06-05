using BepInEx;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
    private static void ApplyVultureGraphicsHooks()
    {
        On.VultureGraphics.InitiateSprites += VultureGraphics_InitiateSprites;
        On.VultureGraphics.DrawSprites += VultureGraphics_DrawSprites;
    }
    private static void RemoveVultureGraphicsHooks()
    {
        On.VultureGraphics.InitiateSprites -= VultureGraphics_InitiateSprites;
        On.VultureGraphics.DrawSprites -= VultureGraphics_DrawSprites;
    }

    private static void VultureGraphics_InitiateSprites(On.VultureGraphics.orig_InitiateSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        GraphicsModuleData tag = self.Tag();

        tag.bodyChunkSprites = new List<int>[self.vulture.bodyChunks.Length];
        for (int i = 0; i < self.vulture.tentacles.Length; i++)
        {
            tag.bodyChunkSprites[2 + i % 2] = new(Enumerable.Range(self.FeatherSprite(i, 0), self.FeatherSprite(i+1, 0) - self.FeatherSprite(i, 0) - 1));
            tag.bodyChunkSprites[2 + i % 2].Insert(0, self.TentacleSprite(i));
        }
        List<int> torsoSprites = new(Enumerable.Range(self.AppendageSprite(0), self.AppendageSprite(self.appendages.Length - 1) - self.AppendageSprite(0) + 1));
        torsoSprites.AddRange(Enumerable.Range(self.BackShieldSprite(0), self.NeckSprite - self.BackShieldSprite(0)));
        tag.bodyChunkSprites[0] = tag.bodyChunkSprites[1] = torsoSprites;
        tag.bodyChunkSprites[4] = new(Enumerable.Range(self.NeckSprite, sLeaser.sprites.Length - self.NeckSprite));
        //affectedSprites.RemoveRange(self.SpriteHeadStart, self.SpriteHeadEnd - self.SpriteHeadStart);

        //for (int i = 0; i < sLeaser.sprites.Length; i++)
        //{
        //    RWCustom.Custom.Log("Vulture sprite " + i + ": " + sLeaser.sprites[i].element.name);
        //}

        //self.Tag().affectedSprites = [.. affectedSprites];
    }

    private static void VultureGraphics_DrawSprites(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        UpdateTagColors(self.Tag(), sLeaser, 0.1f);
    }
}
