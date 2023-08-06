using RWCustom;
using System;
using UnityEngine;
using static Vinki.Plugin;
using Random = UnityEngine.Random;

namespace Vinki
{
    public static partial class Hooks
    {

        // Add hooks
        private static void ApplyPlayerGraphicsHooks()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (!self.player.IsVinki(out var vinki))
            {
                return;
            }

            vinki.SetupColors(self);
            vinki.LoadTailAtlas();
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (!self.player.IsVinki(out var vinki))
            {
                return;
            }

            vinki.shoesSprite = sLeaser.sprites.Length;
            vinki.rainPodsSprite = vinki.shoesSprite + 1;
            vinki.glassesSprite = vinki.rainPodsSprite + 1;

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 3);

            //for (var i = 0; i < 2; i++)
            //{
            //    for (var j = 0; j < 2; j++)
            //    {
            //        sLeaser.sprites[vinki.WingSprite(i, j)] = new FSprite("VinkiWing" + (j == 0 ? "A1" : "B2"));
            //        sLeaser.sprites[vinki.WingSprite(i, j)].anchorX = 0f;
            //        sLeaser.sprites[vinki.WingSprite(i, j)].scaleY = 1f;
            //    }
            //}

            sLeaser.sprites[vinki.shoesSprite] = new FSprite("ShoesA0");
            sLeaser.sprites[vinki.rainPodsSprite] = new FSprite("RainPodsA0");
            sLeaser.sprites[vinki.glassesSprite] = new FSprite("GlassesA0");

            if (sLeaser.sprites[2] is TriangleMesh tail && vinki.TailAtlas.elements != null && vinki.TailAtlas.elements.Count > 0)
            {
                tail.element = vinki.TailAtlas.elements[0];
                for (var i = tail.vertices.Length - 1; i >= 0; i--)
                {
                    var perc = i / 2 / (float)(tail.vertices.Length / 2);
                    //tail.verticeColors[i] = Color.Lerp(fromColor, toColor, perc);
                    Vector2 uv;
                    if (i % 2 == 0)
                        uv = new Vector2(perc, 0f);
                    else if (i < tail.vertices.Length - 1)
                        uv = new Vector2(perc, 1f);
                    else
                        uv = new Vector2(1f, 0f);

                    // Map UV values to the element
                    uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                    uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                    tail.UVvertices[i] = uv;
                }
            }

            self.AddToContainer(sLeaser, rCam, null);
        }

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (!self.player.IsVinki(out var vinki))
            {
                return;
            }

            if (vinki.tailStripesSprite > 0 && sLeaser.sprites.Length > vinki.tailStripesSprite + 3)
            {
                var midgroundContainer = rCam.ReturnFContainer("Midground");
                var hud2Container = rCam.ReturnFContainer("HUD2");

                // (Debug) Get indices of sprites
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    Debug.Log("Sprite " + i + ": " + sLeaser.sprites[i].element.name);
                }

                //-- Glasses go in front of face
                //for (var i = 0; i < 2; i++)
                //{
                //    for (var j = 0; j < 2; j++)
                //    {
                //        var sprite = sLeaser.sprites[vinki.glassesSprite(i, j)];
                //        sprite.RemoveFromContainer();
                //        midgroundContainer.AddChild(sprite);
                //    }
                //}
                sLeaser.sprites[vinki.glassesSprite].RemoveFromContainer();
                midgroundContainer.AddChild(sLeaser.sprites[vinki.glassesSprite]);

                //-- Tail go behind hips
                sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);

                //-- RainPods go behind glasses
                sLeaser.sprites[vinki.rainPodsSprite].RemoveFromContainer();
                midgroundContainer.AddChild(sLeaser.sprites[vinki.rainPodsSprite]);
                sLeaser.sprites[vinki.rainPodsSprite].MoveBehindOtherNode(sLeaser.sprites[2]);

                // Shoes go behind hips
                sLeaser.sprites[vinki.shoesSprite].RemoveFromContainer();
                midgroundContainer.AddChild(sLeaser.sprites[vinki.shoesSprite]);
                sLeaser.sprites[vinki.shoesSprite].MoveBehindOtherNode(sLeaser.sprites[1]);

                //-- Stamina HUD
                //sLeaser.sprites[vinki.staminaSprite].RemoveFromContainer();
                //hud2Container.AddChild(sLeaser.sprites[vinki.staminaSprite]);
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!self.player.IsVinki(out var vinki))
            {
                return;
            }

            Vector2 animationOffset = GetAnimationOffset(self);

            Color glassesColor = vinki.GlassesColor;
            Color shoesColor = vinki.ShoesColor;
            Color rainPodsColor = vinki.RainPodsColor;
            Color stripesColor = vinki.StripesColor;

            sLeaser.sprites[2].color = Color.white;

            //-- Antennae stuff

            var headSpriteName = sLeaser.sprites[3].element.name;
            if (!string.IsNullOrWhiteSpace(headSpriteName) && headSpriteName.Contains("HeadA"))
            {
                var headSpriteNumber = headSpriteName.Substring(headSpriteName.LastIndexOf("HeadA", StringComparison.InvariantCultureIgnoreCase) + 5);

                var antennaeOffsetX = 0f;
                var antennaeOffsetY = 0f;
                switch (headSpriteNumber)
                {
                    case "0":
                    case "1":
                    case "2":
                    case "3":
                        antennaeOffsetY = 2f;
                        break;
                    case "5":
                    case "6":
                        antennaeOffsetX = -1.5f * Math.Sign(sLeaser.sprites[3].scaleX);
                        break;
                    case "7":
                        antennaeOffsetY = -3.5f;
                        break;
                }

                var antennaePos = new Vector2(sLeaser.sprites[3].x + antennaeOffsetX, sLeaser.sprites[3].y + antennaeOffsetY);

                sLeaser.sprites[vinki.shoesSprite].scaleX = sLeaser.sprites[3].scaleX * 1.3f;
                sLeaser.sprites[vinki.shoesSprite].scaleY = 1.3f;
                sLeaser.sprites[vinki.shoesSprite].rotation = sLeaser.sprites[3].rotation;
                sLeaser.sprites[vinki.shoesSprite].x = antennaePos.x;
                sLeaser.sprites[vinki.shoesSprite].y = antennaePos.y;
                //sLeaser.sprites[vinki.shoesSprite].element = Futile.atlasManager.GetElementWithName("BeeAntennaeHeadA" + headSpriteNumber);
                sLeaser.sprites[vinki.shoesSprite].color = shoesColor;
            }

            //-- Fluff stuff
            var headToBody = (new Vector2(sLeaser.sprites[1].x, sLeaser.sprites[1].y) - new Vector2(sLeaser.sprites[3].x, sLeaser.sprites[3].y)).normalized;
            //var floofPos = Vector2.Lerp(player.lastFloofPos, new Vector2(sLeaser.sprites[3].x + headToBody.x * 4f, sLeaser.sprites[3].y + headToBody.y * 4f), timeStacker * 2);
            var floofPos = new Vector2(sLeaser.sprites[3].x + headToBody.x * 7.5f, sLeaser.sprites[3].y + headToBody.y * 7.5f);

            //sLeaser.sprites[player.rainPodsSprite].scaleX = sLeaser.sprites[3].scaleX;
            sLeaser.sprites[vinki.rainPodsSprite].scaleY = 0.75f;
            sLeaser.sprites[vinki.rainPodsSprite].rotation = sLeaser.sprites[3].rotation;
            sLeaser.sprites[vinki.rainPodsSprite].x = floofPos.x;
            sLeaser.sprites[vinki.rainPodsSprite].y = floofPos.y;
            sLeaser.sprites[vinki.rainPodsSprite].color = rainPodsColor;

            sLeaser.sprites[vinki.glassesSprite].color = glassesColor;
            sLeaser.sprites[vinki.tailStripesSprite].color = stripesColor;

            //player.lastFloofPos = new Vector2(sLeaser.sprites[player.rainPodsSprite].x, sLeaser.sprites[player.rainPodsSprite].y);

            //-- Hand stuff

            for (var i = 5; i <= 8; i++)
            {
                var name = "Beecat" + sLeaser.sprites[i].element.name;
                if (!name.StartsWith("Beecat") && Futile.atlasManager.DoesContainElementWithName(name))
                {
                    sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(name);
                }
            }
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (self.player.SlugCatClass != Enums.TheVinki)
            {
                return;
            }

            if (craftCounter > 0)
            {
                foreach (SlugcatHand hand in self.hands)
                {
                    hand.pos = Vector2.Lerp(hand.pos, self.drawPositions[0, 0], (float)craftCounter / 25f);
                }

                float num10 = Mathf.InverseLerp(0f, 110f, (float)craftCounter);
                float num11 = (float)craftCounter / Mathf.Lerp(30f, 15f, num10);
                if (self.player.standing)
                {
                    self.drawPositions[0, 0].y += Mathf.Sin(num11 * 3.1415927f * 2f) * num10 * 2f;
                    self.drawPositions[1, 0].y += -Mathf.Sin((num11 + 0.2f) * 3.1415927f * 2f) * num10 * 3f;
                }
                else
                {
                    self.drawPositions[0, 0].y += Mathf.Sin(num11 * 3.1415927f * 2f) * num10 * 3f;
                    self.drawPositions[0, 0].x += Mathf.Cos(num11 * 3.1415927f * 2f) * num10 * 1f;
                    self.drawPositions[1, 0].y += Mathf.Sin((num11 + 0.2f) * 3.1415927f * 2f) * num10 * 2f;
                    self.drawPositions[1, 0].x += -Mathf.Cos(num11 * 3.1415927f * 2f) * num10 * 3f;
                }
            }
        }

        public static Vector2 GetAnimationOffset(PlayerGraphics self)
        {
            var result = Vector2.zero;

            if (self.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                result.x += self.player.flipDirection * (self.RenderAsPup ? 2f : 6f) * Mathf.Clamp(Mathf.Abs(self.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f) * 0.3f;
                result.y += Mathf.Cos((self.player.animationFrame + 0f) / 6f * 2f * 3.1415927f) * (self.RenderAsPup ? 1.5f : 2f) * 0.3f;
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.Crawl)
            {
                var num4 = Mathf.Sin(self.player.animationFrame / 21f * 2f * 3.1415927f);
                var num5 = Mathf.Cos(self.player.animationFrame / 14f * 2f * 3.1415927f);
                result.x += num5 * self.player.flipDirection * 2f;
                result.y -= num4 * -1.5f - 3f;
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
            {
                if (self.player.animation == Player.AnimationIndex.ClimbOnBeam)
                {
                    result.x += self.player.flipDirection * 2.5f + self.player.flipDirection * 0.5f * Mathf.Sin(self.player.animationFrame / 20f * 3.1415927f * 2f);
                }
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.WallClimb)
            {
                result.y += 2f;
                result.x -= self.player.flipDirection * (self.owner.bodyChunks[1].ContactPoint.y < 0 ? 3f : 5f);
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.Default)
            {
                if (self.player.animation == Player.AnimationIndex.LedgeGrab)
                {
                    result.x -= self.player.flipDirection * 5f;
                }
            }
            else if (self.player.animation == Player.AnimationIndex.CorridorTurn)
            {
                result += Custom.DegToVec(Random.value * 360f) * 3f * Random.value;
            }

            return result;
        }
    }
}