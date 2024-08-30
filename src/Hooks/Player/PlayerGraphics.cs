using JollyCoop;
using RWCustom;
using SprayCans;
using System;
using System.Collections.Generic;
using System.Linq;
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
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPallete;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.PlayerGraphics.JollyUniqueColorMenu += PlayerGraphics_JollyUniqueColorMenu;
            On.PlayerGraphics.LoadJollyColorsFromOptions += PlayerGraphics_LoadJollyColorsFromOptions;
            On.PlayerGraphics.PopulateJollyColorArray += PlayerGraphics_PopulateJollyColorsArray;
        }
        private static void RemovePlayerGraphicsHooks()
        {
            On.PlayerGraphics.ctor -= PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites -= PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.AddToContainer -= PlayerGraphics_AddToContainer;
            On.PlayerGraphics.ApplyPalette -= PlayerGraphics_ApplyPallete;
            On.PlayerGraphics.DrawSprites -= PlayerGraphics_DrawSprites;
            On.PlayerGraphics.Update -= PlayerGraphics_Update;
            On.PlayerGraphics.JollyUniqueColorMenu -= PlayerGraphics_JollyUniqueColorMenu;
            On.PlayerGraphics.LoadJollyColorsFromOptions -= PlayerGraphics_LoadJollyColorsFromOptions;
            On.PlayerGraphics.PopulateJollyColorArray -= PlayerGraphics_PopulateJollyColorsArray;
        }

        private static void PlayerGraphics_PopulateJollyColorsArray(On.PlayerGraphics.orig_PopulateJollyColorArray orig, SlugcatStats.Name reference)
        {
            if (reference != Enums.vinki || Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.AUTO)
            {
                orig(reference);
                return;
            }

            PlayerGraphics.jollyColors = new Color?[16][];
            JollyCustom.Log("Initializing colors... reference " + (reference?.ToString()), false);
            for (int i = 0; i < PlayerGraphics.jollyColors.Length; i++)
            {
                PlayerGraphics.jollyColors[i] = new Color?[3];
                JollyCustom.Log("Need to generate colors for player " + i.ToString(), false);
                if (i == 0)
                {
                    List<string> list = PlayerGraphics.DefaultBodyPartColorHex(reference);
                    PlayerGraphics.jollyColors[0][0] = new Color?(Color.white);
                    PlayerGraphics.jollyColors[0][1] = new Color?(Color.black);
                    PlayerGraphics.jollyColors[0][2] = new Color?(Color.green);
                    if (list.Count >= 1)
                    {
                        PlayerGraphics.jollyColors[0][0] = new Color?(Custom.hexToColor(list[0]));
                    }
                    if (list.Count >= 2)
                    {
                        PlayerGraphics.jollyColors[0][1] = new Color?(Custom.hexToColor(list[1]));
                    }
                    if (list.Count >= 3)
                    {
                        PlayerGraphics.jollyColors[0][2] = new Color?(Custom.hexToColor(list[2]));
                    }
                }
                // Rider OC for AUTO player 2
                else if (i == 1)
                {
                    PlayerGraphics.jollyColors[1][0] = new Color(0.484f, 0.297f, 1f);
                    PlayerGraphics.jollyColors[1][1] = PlayerGraphics.jollyColors[0][1];
                    PlayerGraphics.jollyColors[1][2] = new Color(0.98f, 1f, 0.039f);
                }
                else
                {
                    Color color = JollyCustom.GenerateComplementaryColor(PlayerGraphics.JollyColor(i - 1, 0));
                    PlayerGraphics.jollyColors[i][0] = new Color?(color);
                    HSLColor hslcolor = JollyCustom.RGB2HSL(JollyCustom.GenerateClippedInverseColor(color));
                    float num = hslcolor.lightness + 0.45f;
                    hslcolor.lightness *= num;
                    hslcolor.saturation *= num;
                    PlayerGraphics.jollyColors[i][1] = new Color?(hslcolor.rgb);
                    HSLColor hslcolor2 = JollyCustom.RGB2HSL(JollyCustom.GenerateComplementaryColor(hslcolor.rgb));
                    hslcolor2.saturation = Mathf.Lerp(hslcolor2.saturation, 1f, 0.8f);
                    hslcolor2.lightness = Mathf.Lerp(hslcolor2.lightness, 1f, 0.8f);
                    PlayerGraphics.jollyColors[i][2] = new Color?(hslcolor2.rgb);
                    JollyCustom.Log("Generating auto color for player " + i.ToString(), false);
                }
            }
        }

        private static void PlayerGraphics_LoadJollyColorsFromOptions(On.PlayerGraphics.orig_LoadJollyColorsFromOptions orig, int playerNumber)
        {
            orig(playerNumber);
            //VLogger.LogInfo("Called PlayerGraphics.LoadJollyColorsFromOptions. Lengths: [" + PlayerGraphics.jollyColors.Length + "][" + PlayerGraphics.jollyColors[playerNumber].Length + ']');
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

            if (!ModManager.MSC)
            {
                self.Tag().affectedSprites = [0, 1, 2, 3, 4, 5, 6, 7, 8, 10];
                return;
            }

            if (!self.player.IsVinki(out var vinki))
            {
                self.Tag().affectedSprites = [0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 12];
                return;
            }

            vinki.tagIconSprite = sLeaser.sprites.Length;
            vinki.glassesSprite = vinki.tagIconSprite + 1;
            vinki.stripesSprite = vinki.glassesSprite + 1;
            vinki.rainPodsSprite = vinki.stripesSprite + 1;
            vinki.shoesSprite = vinki.rainPodsSprite + 1;
            self.Tag().affectedSprites = [];

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
            sLeaser.sprites[vinki.tagIconSprite] = new FSprite("TagIcon")
            {
                isVisible = false
            };

            if (!ModManager.ActiveMods.Exists((ModManager.Mod mod) => mod.id == "dressmyslugcat"))
            {
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 4);

                sLeaser.sprites[vinki.stripesSprite] = new TriangleMesh("Futile_White", (sLeaser.sprites[2] as TriangleMesh).triangles, false);
                sLeaser.sprites[vinki.shoesSprite] = new FSprite("ShoesA0");
                sLeaser.sprites[vinki.rainPodsSprite] = new FSprite("RainPodsA0");
                sLeaser.sprites[vinki.glassesSprite] = new FSprite("GlassesA0");
            }
            else
            {
                string faceSprite = "";
                try
                {
                    faceSprite = GetDMSFaceSprite(self.player);
                }
                catch (Exception e)
                {
                    throw new Exception("Somehow DMS is active yet it isn't? " + e.Message);
                }
                switch (faceSprite)
                {
                    case "olaycolay.thevinki0":
                    case "olaycolay.thevinki1":
                    case "olaycolay.thevinki2":
                    case "olaycolay.thevinki3":
                    case "olaycolay.thevinki4":
                    case "olaycolay.thevinki5":
                        break;
                    default:
                        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
                        sLeaser.sprites[vinki.glassesSprite] = new FSprite("GlassesA0");
                        break;
                }
            }

            if (sLeaser.sprites.Length > vinki.stripesSprite && sLeaser.sprites[vinki.stripesSprite] is TriangleMesh tail && vinki.TailAtlas.elements != null && vinki.TailAtlas.elements.Count > 0)
            {
                tail.element = vinki.TailAtlas.elements[0];
                for (var i = tail.vertices.Length - 1; i >= 0; i--)
                {
                    var perc = i / 2 / (float)(tail.vertices.Length / 2);
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

            if (vinki.shoesSprite > 0 && sLeaser.sprites.Length > vinki.glassesSprite)
            {
                // (Debug) Get indices of sprites
                //for (int i = 0; i < sLeaser.sprites.Length; i++)
                //{
                //    VLogger.LogInfo("Sprite " + i + ": " + sLeaser.sprites[i].element.name);
                //}

                var midgroundContainer = rCam.ReturnFContainer("Midground");

                //-- TagIcon goes above face
                sLeaser.sprites[vinki.tagIconSprite].RemoveFromContainer();
                rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[vinki.tagIconSprite]);
                sLeaser.sprites[vinki.tagIconSprite].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[vinki.tagIconSprite].shader = FShader.Basic;

                //-- Glasses go in front of face
                sLeaser.sprites[vinki.glassesSprite].RemoveFromContainer();
                if (!ModManager.ActiveMods.Exists((ModManager.Mod mod) => mod.id == "dressmyslugcat") || VinkiConfig.GlassesOverDMS.Value)
                {
                    midgroundContainer.AddChild(sLeaser.sprites[vinki.glassesSprite]);
                    sLeaser.sprites[vinki.glassesSprite].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                }

                if (sLeaser.sprites.Length > vinki.shoesSprite)
                {
                    //-- RainPods go behind glasses
                    sLeaser.sprites[vinki.rainPodsSprite].RemoveFromContainer();
                    midgroundContainer.AddChild(sLeaser.sprites[vinki.rainPodsSprite]);
                    sLeaser.sprites[vinki.rainPodsSprite].MoveBehindOtherNode(sLeaser.sprites[vinki.glassesSprite]);

                    // Shoes go in front of legs
                    sLeaser.sprites[vinki.shoesSprite].RemoveFromContainer();
                    midgroundContainer.AddChild(sLeaser.sprites[vinki.shoesSprite]);
                    sLeaser.sprites[vinki.shoesSprite].MoveInFrontOfOtherNode(sLeaser.sprites[4]);
                }

                //-- Tail goes behind hips
                if (sLeaser.sprites.Length > vinki.stripesSprite)
                {
                    sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);
                    sLeaser.sprites[vinki.stripesSprite].MoveBehindOtherNode(sLeaser.sprites[1]);
                    midgroundContainer.AddChild(sLeaser.sprites[vinki.stripesSprite]);
                }
            }
        }

        private static void PlayerGraphics_ApplyPallete(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (!self.player.IsVinki(out var vinki) || sLeaser.sprites.Length <= vinki.tagIconSprite)
            {
                return;
            }

            if (sLeaser.sprites.Length > vinki.stripesSprite)
            {
                sLeaser.sprites[vinki.stripesSprite].color = vinki.StripesColor;
            }

            // Set color to white if DMS is on (so that skins show the correct color)
            if (ModManager.ActiveMods.Any(mod => mod.id == "dressmyslugcat"))
            {
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    if (i != 11)
                    {
                        sLeaser.sprites[i].color = Color.white;
                    }
                }
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            UpdateTagColors(self.Tag(), sLeaser);

            if (!self.player.IsVinki(out var vinki) || sLeaser.sprites.Length <= vinki.tagIconSprite)
            {
                return;
            }

            //Vector2 animationOffset = GetAnimationOffset(self);
            Color rainPodsColor, shoesColor, glassesColor;
            if (rCam.room.game.IsArenaSession)
            {
                glassesColor = GetCustomVinkiArenaColor(vinki, self.player.JollyOption.playerNumber % 4, vinki.glassesSprite);
                shoesColor = GetCustomVinkiArenaColor(vinki, self.player.JollyOption.playerNumber % 4, vinki.shoesSprite);
                rainPodsColor = GetCustomVinkiArenaColor(vinki, self.player.JollyOption.playerNumber % 4, vinki.rainPodsSprite);
            }
            else
            {
                glassesColor = GetCustomVinkiColor(self.player.JollyOption.playerNumber, 5);
                shoesColor = GetCustomVinkiColor(self.player.JollyOption.playerNumber, 4);
                rainPodsColor = GetCustomVinkiColor(self.player.JollyOption.playerNumber, 3);
            }

            // RainPods
            var headSpriteName = sLeaser.sprites[3].element.name;
            if (!string.IsNullOrWhiteSpace(headSpriteName) && headSpriteName.Contains("HeadA") && sLeaser.sprites.Length > vinki.rainPodsSprite)
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
            if (!string.IsNullOrWhiteSpace(legsSpriteName) && legsSpriteName.Contains("LegsA") && sLeaser.sprites.Length > vinki.shoesSprite)
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
            if (!string.IsNullOrWhiteSpace(faceSpriteName) && faceSpriteName.Contains("Face") && sLeaser.sprites.Length > vinki.glassesSprite)
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

            // TagIcon
            if (sLeaser.sprites.Length > vinki.tagIconSprite)
            {
                float xPos = sLeaser.sprites[9].x + Mathf.Lerp(0f, 30f, vinki.tagIconSize);
                float yPos = sLeaser.sprites[9].y + Mathf.Lerp(0f, 20f, vinki.tagIconSize);
                var iconPos = new Vector2(xPos, yPos);

                bool tagAble = vinki.tagLag <= 0 && (!VinkiConfig.RequireCansTagging.Value || (
                    self.player.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan) != null) && 
                    ((self.player.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan).grabbed as SprayCan).Abstr.uses > 0));

                sLeaser.sprites[vinki.tagIconSprite].rotation = 0f;
                sLeaser.sprites[vinki.tagIconSprite].x = iconPos.x;
                sLeaser.sprites[vinki.tagIconSprite].y = iconPos.y;
                sLeaser.sprites[vinki.tagIconSprite].element = Futile.atlasManager.GetElementWithName("TagIcon");
                sLeaser.sprites[vinki.tagIconSprite].color = tagAble ? Color.white : Color.gray;

                bool tagReady = vinki.tagableBodyChunk != null;
                vinki.tagIconSize = Mathf.Clamp01(tagReady ? vinki.tagIconSize + 0.05f : vinki.tagIconSize - 0.05f);
                sLeaser.sprites[vinki.tagIconSprite].scale = Mathf.Lerp(0f, 0.25f, vinki.tagIconSize);
                sLeaser.sprites[vinki.tagIconSprite].isVisible = tagReady || vinki.tagIconSize > 0f;
            }

            // Tail Stripes
            if (sLeaser.sprites.Length > vinki.stripesSprite && sLeaser.sprites[vinki.stripesSprite] is TriangleMesh tail)
            {
                tail.vertices = (sLeaser.sprites[2] as TriangleMesh).vertices;
                tail.scaleX = sLeaser.sprites[2].scaleX;
                tail.scaleY = sLeaser.sprites[2].scaleY;
                tail.rotation = sLeaser.sprites[2].rotation;
                tail.anchorX = sLeaser.sprites[2].anchorX;
                tail.anchorY = sLeaser.sprites[2].anchorY;
                tail.x = sLeaser.sprites[2].x;
                tail.y = sLeaser.sprites[2].y;
            }

            if (curMsPerBeat > 0)
            {
                VinkiPlayerData v = self.player.Vinki();

                v.beatTimer -= 1000 * Time.deltaTime;

                if (v.beatTimer <= 0f)
                {
                    v.beatTimer += curMsPerBeat;
                    if (v.idleUpdates >= 120)
                    {
                        bool lookingRight = self.lookDirection.x > 0f;
                        self.head.vel.x += lookingRight ? 3f : -3f;
                    }
                }
            }
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (self.player.SlugCatClass != Enums.vinki)
            {
                return;
            }
            VinkiPlayerData v = self.player.Vinki();

            if (v.craftCounter > 0)
            {
                foreach (SlugcatHand hand in self.hands)
                {
                    hand.pos = Vector2.Lerp(hand.pos, self.drawPositions[0, 0], v.craftCounter / 25f);
                }

                float num10 = Mathf.InverseLerp(0f, 110f, v.craftCounter);
                float num11 = v.craftCounter / Mathf.Lerp(30f, 15f, num10);
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
            if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.DEFAULT || (playerNumber == 0 && Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)) && slugName == Enums.vinki)
            {
                return new Color(0.28627450980392155f, 0.3058823529411765f, 0.8274509803921568f);
            }
            else
            {
                return orig(slugName, reference, playerNumber);
            }
        }

        private static string GetDMSFaceSprite(Player player)
        {
            if (DressMySlugcat.Customization.For(player) != null && DressMySlugcat.Customization.For(player).CustomSprite("FACE") != null)
            {
                return DressMySlugcat.Customization.For(player).CustomSprite("FACE").SpriteSheetID;
            }
            return null;
        }

        private static string GetDMSTailSprite(Player player)
        {
            if (DressMySlugcat.Customization.For(player) != null && DressMySlugcat.Customization.For(player).CustomSprite("TAIL") != null)
            {
                return DressMySlugcat.Customization.For(player).CustomSprite("TAIL").SpriteSheetID;
            }
            return null;
        }

        private static Color GetCustomVinkiArenaColor(VinkiPlayerData vinki, int playerNumber, int bodyPartIndex)
        {
            if (bodyPartIndex == vinki.rainPodsSprite)
            {
                switch (playerNumber)
                {
                    case 0: return new Color(0.62f, 0.835f, 1f);
                    case 1: return new Color(1f, 1f, 1f);
                    case 2: return new Color(1f, 1f, 1f);
                    default: return new Color(1f, 1f, 1f);
                }
            }
            else if (bodyPartIndex == vinki.shoesSprite)
            {
                switch (playerNumber)
                {
                    case 0: return new Color(0.62f, 0.835f, 1f);
                    case 1: return new Color(0.549f, 0.749f, 0.141f);
                    case 2: return new Color(0.804f, 0.157f, 0.541f);
                    default: return new Color(1f, 1f, 1f);
                }
            }
            else if (bodyPartIndex == vinki.glassesSprite)
            {
                switch (playerNumber)
                {
                    case 0: return new Color(0.153f, 0.18f, 0.443f);
                    case 1: return new Color(0.384f, 0.192f, 0.035f);
                    case 2: return new Color(0.537f, 0.039f, 0.231f);
                    default: return new Color(1f, 1f, 1f);
                }
            }
            else
            {
                Plugin.VLogger.LogError("Invalid bodyPartIndex!\n" + StackTraceUtility.ExtractStackTrace());
                return Color.white;
            }
        }
    }
}