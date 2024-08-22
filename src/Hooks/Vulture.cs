using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Vinki;
public static partial class Hooks
{
    private static void ApplyVultureGraphicsHooks()
    {
        On.Vulture.DropMask += Vulture_DropMask;

        On.VultureGraphics.InitiateSprites += VultureGraphics_InitiateSprites;
        On.VultureGraphics.DrawSprites += VultureGraphics_DrawSprites;
    }
    private static void RemoveVultureGraphicsHooks()
    {
        On.Vulture.DropMask -= Vulture_DropMask;

        On.VultureGraphics.InitiateSprites -= VultureGraphics_InitiateSprites;
        On.VultureGraphics.DrawSprites -= VultureGraphics_DrawSprites;
    }

    private static void Vulture_DropMask(On.Vulture.orig_DropMask orig, Vulture self, Vector2 violenceDir)
    {
        orig(self, violenceDir);

        if (self.graphicsModule == null || self.graphicsModule.Tag() == null)
        {
            return;
        }

        Color maskColor = self.graphicsModule.Tag().taggedColors[(self.graphicsModule as VultureGraphics).MaskSprite];
        if (maskColor.maxColorComponent > float.Epsilon)
        {
            PhysicalObject newestMask = self.room.physicalObjects[2].LastOrDefault((po) => po is VultureMask);
            if (newestMask != null)
            {
                Color.RGBToHSV(maskColor, out float h, out float s, out _);
                VultureMaskGraphics newestMaskGfx = (newestMask as VultureMask).maskGfx;
                if (newestMaskGfx.King)
                {
                    newestMaskGfx.ColorB = new HSLColor(h, s, Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
                    newestMaskGfx.ColorA = new HSLColor(newestMaskGfx.ColorB.hue + Mathf.Lerp(-0.25f, 0.25f, UnityEngine.Random.value), Mathf.Lerp(0.5f, 0.7f, UnityEngine.Random.value), Mathf.Lerp(0.7f, 0.8f, UnityEngine.Random.value));
                }
                else
                {
                    newestMaskGfx.ColorA = new HSLColor(h, s, Mathf.Lerp(0.7f, 0.8f, UnityEngine.Random.value));
                    newestMaskGfx.ColorB = new HSLColor(newestMaskGfx.ColorA.hue + Mathf.Lerp(-0.25f, 0.25f, UnityEngine.Random.value), Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value), Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
                }
            }
        }
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

        List<int> torsoSprites = [];
        for (int i = 0; i < self.vulture.appendages.Count; i++)
        {
            torsoSprites.Add(self.AppendageSprite(i));
        }
        torsoSprites.Add(self.BodySprite);
        torsoSprites.AddRange(sLeaser.sprites.Select((value, index) => new { value, index }).Where((sprite) => sprite.value.element.name == "KrakenShield0").Select(x => x.index));
        tag.bodyChunkSprites[0] = tag.bodyChunkSprites[1] = torsoSprites;

        int firstHeadSprite = self.IsKing ? self.FirstKingTuskSpriteBehind : self.NeckSprite;
        tag.bodyChunkSprites[4] = new(Enumerable.Range(firstHeadSprite, sLeaser.sprites.Length - firstHeadSprite));
        if (self.IsMiros)
        {
            tag.bodyChunkSprites[4].Remove(self.LaserSprite());
            tag.bodyChunkSprites[4].Remove(self.EyesSprite);
            tag.bodyChunkSprites[4].Remove(self.EyeTrailSprite());
        }
    }

    private static void VultureGraphics_DrawSprites(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        UpdateTagColors(self.Tag(), sLeaser, 0.1f);
    }
}
