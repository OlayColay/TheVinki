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
            On.PlayerGraphics.JollyUniqueColorMenu += PlayerGraphics_JollyUniqueColorMenu;
            On.PlayerGraphics.LoadJollyColorsFromOptions += PlayerGraphics_Debug;
        }

        private static void PlayerGraphics_Debug(On.PlayerGraphics.orig_LoadJollyColorsFromOptions orig, int playerNumber)
        {
            orig(playerNumber);
            //Debug.Log("Called PlayerGraphics.LoadJollyColorsFromOptions. Lengths: [" + PlayerGraphics.jollyColors.Length + "][" + PlayerGraphics.jollyColors[playerNumber].Length + ']');
            if (PlayerGraphics.jollyColors[playerNumber].Length == 6)
            {
                jollyColors[playerNumber] = PlayerGraphics.jollyColors[playerNumber];
            }
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

            if (vinki.shoesSprite > 0 && sLeaser.sprites.Length > vinki.shoesSprite + 2)
            {
                // (Debug) Get indices of sprites
                //for (int i = 0; i < sLeaser.sprites.Length; i++)
                //{
                //    Debug.Log("Sprite " + i + ": " + sLeaser.sprites[i].element.name);
                //}

                var midgroundContainer = rCam.ReturnFContainer("Midground");

                //-- Glasses go in front of face
                sLeaser.sprites[vinki.glassesSprite].RemoveFromContainer();
                midgroundContainer.AddChild(sLeaser.sprites[vinki.glassesSprite]);
                sLeaser.sprites[vinki.glassesSprite].MoveInFrontOfOtherNode(sLeaser.sprites[9]);

                //-- RainPods go behind glasses
                sLeaser.sprites[vinki.rainPodsSprite].RemoveFromContainer();
                midgroundContainer.AddChild(sLeaser.sprites[vinki.rainPodsSprite]);
                sLeaser.sprites[vinki.rainPodsSprite].MoveBehindOtherNode(sLeaser.sprites[vinki.glassesSprite]);

                // Shoes go in front of legs
                sLeaser.sprites[vinki.shoesSprite].RemoveFromContainer();
                midgroundContainer.AddChild(sLeaser.sprites[vinki.shoesSprite]);
                sLeaser.sprites[vinki.shoesSprite].MoveInFrontOfOtherNode(sLeaser.sprites[4]);

                //-- Tail goes behind hips
                sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!self.player.IsVinki(out var vinki))
            {
                return;
            }

            //Vector2 animationOffset = GetAnimationOffset(self);

            Color glassesColor = GetCustomVinkiColor(self.player.JollyOption.playerNumber, 5);
            Color shoesColor = GetCustomVinkiColor(self.player.JollyOption.playerNumber, 4);
            Color rainPodsColor = GetCustomVinkiColor(self.player.JollyOption.playerNumber, 3);
            //Color stripesColor = vinki.StripesColor;

            sLeaser.sprites[2].color = Color.white;

            // RainPods
            var headSpriteName = sLeaser.sprites[3].element.name;
            if (!string.IsNullOrWhiteSpace(headSpriteName) && headSpriteName.Contains("HeadA"))
            {
                var headSpriteNumber = headSpriteName.Substring(headSpriteName.LastIndexOf("HeadA", StringComparison.InvariantCultureIgnoreCase) + 5);
                var rainPodsPos = new Vector2(sLeaser.sprites[3].x, sLeaser.sprites[3].y);

                sLeaser.sprites[vinki.rainPodsSprite].scaleX = sLeaser.sprites[3].scaleX;
                sLeaser.sprites[vinki.rainPodsSprite].scaleY = sLeaser.sprites[3].scaleY;
                sLeaser.sprites[vinki.rainPodsSprite].rotation = sLeaser.sprites[3].rotation;
                sLeaser.sprites[vinki.rainPodsSprite].anchorX = sLeaser.sprites[3].anchorX;
                sLeaser.sprites[vinki.rainPodsSprite].anchorY = sLeaser.sprites[3].anchorY;
                sLeaser.sprites[vinki.rainPodsSprite].x = rainPodsPos.x;
                sLeaser.sprites[vinki.rainPodsSprite].y = rainPodsPos.y;
                sLeaser.sprites[vinki.rainPodsSprite].element = Futile.atlasManager.GetElementWithName("RainPodsA" + headSpriteNumber);
                sLeaser.sprites[vinki.rainPodsSprite].color = rainPodsColor;
            }

            // Shoes
            var legsSpriteName = sLeaser.sprites[4].element.name;
            if (!string.IsNullOrWhiteSpace(legsSpriteName) && legsSpriteName.Contains("LegsA"))
            {
                var legsSpriteNumber = legsSpriteName.Substring(legsSpriteName.LastIndexOf("LegsA", StringComparison.InvariantCultureIgnoreCase) + 5);
                var shoesPos = new Vector2(sLeaser.sprites[4].x, sLeaser.sprites[4].y);

                sLeaser.sprites[vinki.shoesSprite].scaleX = sLeaser.sprites[4].scaleX;
                sLeaser.sprites[vinki.shoesSprite].scaleY = sLeaser.sprites[4].scaleY;
                sLeaser.sprites[vinki.shoesSprite].rotation = sLeaser.sprites[4].rotation;
                sLeaser.sprites[vinki.shoesSprite].anchorX = sLeaser.sprites[4].anchorX;
                sLeaser.sprites[vinki.shoesSprite].anchorY = sLeaser.sprites[4].anchorY;
                sLeaser.sprites[vinki.shoesSprite].x = shoesPos.x;
                sLeaser.sprites[vinki.shoesSprite].y = shoesPos.y;
                sLeaser.sprites[vinki.shoesSprite].element = Futile.atlasManager.GetElementWithName("ShoesA" + legsSpriteNumber);
                sLeaser.sprites[vinki.shoesSprite].color = shoesColor;
            }

            // Glasses
            var faceSpriteName = sLeaser.sprites[9].element.name;
            if (!string.IsNullOrWhiteSpace(faceSpriteName) && faceSpriteName.Contains("Face"))
            {
                var faceSpriteNumber = faceSpriteName.Substring(faceSpriteName.LastIndexOf("Face", StringComparison.InvariantCultureIgnoreCase) + 4);
                var glassesPos = new Vector2(sLeaser.sprites[9].x, sLeaser.sprites[9].y);

                sLeaser.sprites[vinki.glassesSprite].scaleX = sLeaser.sprites[9].scaleX;
                sLeaser.sprites[vinki.glassesSprite].scaleY = sLeaser.sprites[9].scaleY;
                sLeaser.sprites[vinki.glassesSprite].rotation = sLeaser.sprites[9].rotation;
                sLeaser.sprites[vinki.glassesSprite].anchorX = sLeaser.sprites[9].anchorX;
                sLeaser.sprites[vinki.glassesSprite].anchorY = sLeaser.sprites[9].anchorY;
                sLeaser.sprites[vinki.glassesSprite].x = glassesPos.x;
                sLeaser.sprites[vinki.glassesSprite].y = glassesPos.y;
                sLeaser.sprites[vinki.glassesSprite].element = Futile.atlasManager.GetElementWithName("Glasses" + faceSpriteNumber);
                sLeaser.sprites[vinki.glassesSprite].color = glassesColor;
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

        private static Color PlayerGraphics_JollyUniqueColorMenu(On.PlayerGraphics.orig_JollyUniqueColorMenu orig, SlugcatStats.Name slugName, SlugcatStats.Name reference, int playerNumber)
        {
            if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.DEFAULT || (playerNumber == 0 && Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)) && slugName == Enums.TheVinki)
            {
                return new Color(0.28627450980392155f, 0.3058823529411765f, 0.8274509803921568f);
            }
            else
            {
                return orig(slugName, reference, playerNumber);
            }
        }
    }
}