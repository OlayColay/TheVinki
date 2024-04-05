using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoreSlugcats;
using static Vinki.Plugin;
using RWCustom;

namespace Vinki
{
    public static partial class Hooks
    {
        // Add hooks
        private static void ApplyAncientBotHooks()
        {
            On.MoreSlugcats.AncientBot.ctor += AncientBot_ctor;
            On.MoreSlugcats.AncientBot.InitiateSprites += AncientBot_Inititate_Sprites;
            On.MoreSlugcats.AncientBot.ApplyPalette += AncientBot_ApplyPalette;
            On.MoreSlugcats.AncientBot.DrawSprites += AncientBot_Draw_Sprites;
        }

        private static void RemoveAncientBotHooks()
        {
            On.MoreSlugcats.AncientBot.ctor -= AncientBot_ctor;
            On.MoreSlugcats.AncientBot.InitiateSprites -= AncientBot_Inititate_Sprites;
            On.MoreSlugcats.AncientBot.ApplyPalette -= AncientBot_ApplyPalette;
            On.MoreSlugcats.AncientBot.DrawSprites -= AncientBot_Draw_Sprites;
        }

        private static void AncientBot_ctor(On.MoreSlugcats.AncientBot.orig_ctor orig, AncientBot self, Vector2 initPos, Color color, Creature tiedToObject, bool online)
        {
            if (tiedToObject is Player && (tiedToObject as Player).SlugCatClass == Enums.vinki)
            {
                color = new Color(0.945f, 0.3765f, 0f);
            }
            orig(self, initPos, color, tiedToObject, online);
        }

        private static void AncientBot_Inititate_Sprites(On.MoreSlugcats.AncientBot.orig_InitiateSprites orig, AncientBot self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (self.room.game.StoryCharacter != Enums.vinki)
            {
                orig(self, sLeaser, rCam);
                return;
            }

            self.xoffs = new float[self.TotalSprites];
            self.yoffs = new float[self.TotalSprites];
            self.baseXScales = new float[self.TotalSprites];
            self.baseYScales = new float[self.TotalSprites];
            for (int i = 0; i < self.baseXScales.Length; i++)
            {
                self.baseXScales[i] = 1f;
                self.baseYScales[i] = 1f;
            }
            sLeaser.sprites = new FSprite[self.TotalSprites];
            float num = 0.5f;
            for (int j = self.BodyIndex; j < self.LightIndex; j++)
            {
                if (j == self.HeadIndex || j == self.LightBaseIndex)
                {
                    sLeaser.sprites[j] = new FSprite("Circle20", true);
                }
                else
                {
                    sLeaser.sprites[j] = new FSprite("pixel", true);
                }
                sLeaser.sprites[j].anchorX = 0.5f;
                sLeaser.sprites[j].anchorY = 0.5f;
            }
            self.baseXScales[self.BodyIndex + 4] = 10f * num;
            self.baseYScales[self.BodyIndex + 4] = 16f * num;
            self.baseXScales[self.BodyIndex] = 6f * num;
            self.baseYScales[self.BodyIndex] = 8f * num;
            for (int k = 1; k <= 3; k++)
            {
                self.baseXScales[self.BodyIndex + k] = 12f * num;
                self.baseYScales[self.BodyIndex + k] = 3f * num;
            }
            self.baseYScales[self.BodyIndex + 3] = 6f * num;
            self.baseXScales[self.HeadIndex] = 0.8f * num;
            self.baseYScales[self.HeadIndex] = 1f * num;
            self.baseXScales[self.LeftAntIndex + 1] = 8f * num;
            self.baseYScales[self.LeftAntIndex + 1] = 8f * num;
            sLeaser.sprites[self.LeftAntIndex + 1].anchorY = 0.9f;
            self.baseXScales[self.LeftAntIndex] = 4f * num;
            self.baseYScales[self.LeftAntIndex] = 8f * num;
            sLeaser.sprites[self.LeftAntIndex].anchorY = 0.9f;
            self.baseXScales[self.RightAntIndex + 1] = 8f * num;
            self.baseYScales[self.RightAntIndex + 1] = 8f * num;
            sLeaser.sprites[self.RightAntIndex + 1].anchorY = 0.9f;
            self.baseXScales[self.RightAntIndex] = 4f * num;
            self.baseYScales[self.RightAntIndex] = 8f * num;
            sLeaser.sprites[self.RightAntIndex].anchorY = 0.9f;
            sLeaser.sprites[self.LightBaseIndex] = new FSprite("Circle20", true);
            self.baseXScales[self.LightBaseIndex] = 0.5f * num;
            self.baseYScales[self.LightBaseIndex] = 0.5f * num;
            sLeaser.sprites[self.LightIndex] = new FSprite(self.ElementName, true)
            {
                shader = rCam.room.game.rainWorld.Shaders[self.flat ? "FlatLight" : "LightSource"],
                color = self.color
            };
            sLeaser.sprites[self.LightIndex + 1] = new FSprite("Futile_White", true)
            {
                shader = rCam.room.game.rainWorld.Shaders["UnderWaterLight"],
                color = self.color
            };
            for (int m = 0; m < self.baseXScales.Length; m++)
            {
                sLeaser.sprites[m].scaleX = self.baseXScales[m];
                sLeaser.sprites[m].scaleY = self.baseYScales[m];
            }
            self.AddToContainer(sLeaser, rCam, null);
        }

        private static void AncientBot_ApplyPalette(On.MoreSlugcats.AncientBot.orig_ApplyPalette orig, AncientBot self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.room.game.IsStorySession && self.room.game.StoryCharacter != Enums.vinki)
            {
                return;
            }

            for (int i = self.BodyIndex; i < self.LightBaseIndex; i++)
            {
                if (i == self.LeftAntIndex + 1 || i == self.RightAntIndex + 1)
                {
                    sLeaser.sprites[i].color = new Color(0.845f, 0.1765f, 0.07f);
                }
                else if (i == self.BodyIndex)
                {
                    sLeaser.sprites[i].color = new Color(0.845f, 0.1765f, 0.07f);
                }
                else
                {
                    sLeaser.sprites[i].color = new Color(0.28f, 0.053f, 0.12f);
                }
            }
        }

        private static void AncientBot_Draw_Sprites(On.MoreSlugcats.AncientBot.orig_DrawSprites orig, AncientBot self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.room.game.IsStorySession && self.room.game.StoryCharacter != Enums.vinki)
            {
                return;
            }

            if (self.lockTarget != null && !self.lightOn)
            {
                sLeaser.sprites[self.LightBaseIndex].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            }

            Vector2 vector = Custom.DegToVec(-90f - self.antAngOff) * sLeaser.sprites[self.LeftAntIndex + 1].scaleY * 0.8f;
            Vector2 vector2 = Custom.DegToVec(90f + self.antAngOff) * sLeaser.sprites[self.RightAntIndex + 1].scaleY * 0.8f;
            sLeaser.sprites[self.LeftAntIndex].x = sLeaser.sprites[self.LeftAntIndex + 1].x + vector.x;
            sLeaser.sprites[self.LeftAntIndex].y = sLeaser.sprites[self.LeftAntIndex + 1].y + vector.y;
            sLeaser.sprites[self.RightAntIndex + 1].x = sLeaser.sprites[self.HeadIndex].x + sLeaser.sprites[self.HeadIndex].scaleX * 20f * 0.3f;
            sLeaser.sprites[self.RightAntIndex + 1].y = sLeaser.sprites[self.HeadIndex].y + sLeaser.sprites[self.HeadIndex].scaleY * 20f * 0.3f;
            sLeaser.sprites[self.RightAntIndex].x = sLeaser.sprites[self.RightAntIndex + 1].x + vector2.x;
            sLeaser.sprites[self.RightAntIndex].y = sLeaser.sprites[self.RightAntIndex + 1].y + vector2.y;
            sLeaser.sprites[self.LeftAntIndex + 1].rotation = 90f - self.antAngOff;
            sLeaser.sprites[self.LeftAntIndex].rotation = 90f - self.antAngOff;
            sLeaser.sprites[self.RightAntIndex + 1].rotation = 270f + self.antAngOff;
            sLeaser.sprites[self.RightAntIndex].rotation = 270f + self.antAngOff;
            sLeaser.sprites[self.RightAntIndex].y -= sLeaser.sprites[self.HeadIndex].scaleY * 20f * 0.4f;
            sLeaser.sprites[self.RightAntIndex + 1].y -= sLeaser.sprites[self.HeadIndex].scaleY * 20f * 0.4f;
            sLeaser.sprites[self.LeftAntIndex].y -= sLeaser.sprites[self.HeadIndex].scaleY * 20f * 0.4f;
            sLeaser.sprites[self.LeftAntIndex + 1].y -= sLeaser.sprites[self.HeadIndex].scaleY * 20f * 0.4f;
        }
    }
}