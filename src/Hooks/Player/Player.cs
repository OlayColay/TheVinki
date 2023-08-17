using System;
using UnityEngine;
using RWCustom;
using System.Linq;
using ImprovedInput;
using SprayCans;
using static Vinki.Plugin;
using System.Threading.Tasks;

namespace Vinki
{
    public static partial class Hooks
    {
        public static async Task SprayGraffiti(Player self, int smokes = 10, int gNum = -1, float alphaPerSmoke = 1f)
        {
            if (gNum < 0)
            {
                gNum = UnityEngine.Random.Range(storyGraffitiCount, graffitis.Count);
            }

            Vector2 sprayPos = storyGraffitiRoomPositions.ContainsKey(gNum) ? storyGraffitiRoomPositions[gNum] : self.mainBodyChunk.pos;

            self.room.PlaySound(SoundID.Vulture_Jet_LOOP, self.mainBodyChunk, false, 1f, 2f);

            for (int j = 0; j < 4; j++)
            {
                graffitis[gNum].vertices[j, 0] = alphaPerSmoke;
            }

            for (int i = 0; i < smokes; i++)
            {
                PlacedObject graffiti = new PlacedObject(PlacedObject.Type.CustomDecal, graffitis[gNum]);
                graffiti.pos = sprayPos + graffitiOffsets[gNum];

                Vector2 smokePos = new Vector2(
                    sprayPos.x + UnityEngine.Random.Range(graffitiOffsets[gNum].x, -graffitiOffsets[gNum].x),
                    sprayPos.y + UnityEngine.Random.Range(graffitiOffsets[gNum].y, -graffitiOffsets[gNum].y));
                var smoke = new Explosion.ExplosionSmoke(smokePos, Vector2.zero, 2f);
                smoke.lifeTime = 15f;
                smoke.life = 2f;
                self.room.AddObject(smoke);
                smoke.colorA = graffitiAvgColors[gNum];
                smoke.colorB = Color.gray;

                await Task.Delay(100);
                self.room.AddObject(new CustomDecal(graffiti));
            }
        }

        // Add hooks
        private static void ApplyPlayerHooks()
        {
            On.Player.Jump += Player_Jump;
            On.Player.MovementUpdate += Player_Move;
            On.Player.Update += Player_Update;
            On.Player.JollyUpdate += Player_JollyUpdate;
        }

        private static void Player_JollyUpdate(On.Player.orig_JollyUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (intro != null && intro.phase == CutsceneVinkiIntro.Phase.Wait && intro.cutsceneTimer > 2230)
            {
                self.input[0].y = -1;
                self.JollyEmoteUpdate();
                self.sleepCounter = 99;
                self.JollyPointUpdate();

                if (intro.cutsceneTimer == 2260)
                {
                    sleeping = true;
                }
            }
        }

        // Implement SuperJump
        private static void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            // Don't jump off a pole when we're trying to do a trick jump at the top
            if (isGrindingV && !grindUpPoleFlag && self.input[0].x == 0f && lastYDirection > 0)
            {
                return;
            }

            orig(self);

            if (!SuperJump.TryGet(self, out float power) || !CoyoteBoost.TryGet(self, out var coyoteBoost) ||
                self.SlugCatClass != Enums.TheVinki)
            {
                return;
            }

            // If player jumped or coyote jumped from a beam (or grinded to top of pole), then trick jump
            bool coyote = isCoyoteJumping(self);
            if (coyote || isGrindingH || grindUpPoleFlag)
            {
                // Get num multiplier
                float num = Mathf.Lerp(1f, 1.15f, self.Adrenaline);
                if (self.grasps[0] != null && self.HeavyCarry(self.grasps[0].grabbed) && !(self.grasps[0].grabbed is Cicada))
                {
                    num += Mathf.Min(Mathf.Max(0f, self.grasps[0].grabbed.TotalMass - 0.2f) * 1.5f, 1.3f);
                }

                // Initiate flip
                if (self.PainJumps)
                {
                    self.bodyChunks[0].vel.y = 4f * num;
                    self.bodyChunks[1].vel.y = 3f * num;
                }
                else
                {
                    self.bodyChunks[0].vel.y = 9f * num;
                    self.bodyChunks[1].vel.y = 7f * num;
                }
                self.slideDirection = lastXDirection;

                // Get risk/reward speedboost when coyote jumping
                if (coyote)
                {
                    self.mainBodyChunk.vel.x += coyoteBoost * self.slideDirection;
                    self.room.PlaySound(SoundID.Slugcat_Flip_Jump, self.mainBodyChunk, false, 3f, 1f);
                }
                else
                {
                    self.room.PlaySound(SoundID.Slugcat_Flip_Jump, self.mainBodyChunk, false, 1f, 1f);
                }

                self.jumpBoost *= power;
                self.animation = Player.AnimationIndex.Flip;
                self.slideCounter = 0;

                grindUpPoleFlag = false;
            }
        }

        private static bool isCoyoteJumping(Player self)
        {
            return (lastAnimation == Player.AnimationIndex.StandOnBeam && self.animation == Player.AnimationIndex.None &&
                    self.bodyMode == Player.BodyModeIndex.Default);
        }

        // Implement higher beam speed
        private static void Player_Move(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!GrindXSpeed.TryGet(self, out var grindXSpeed) || !NormalXSpeed.TryGet(self, out var normalXSpeed) ||
                !GrindYSpeed.TryGet(self, out var grindYSpeed) || !NormalYSpeed.TryGet(self, out var normalYSpeed) ||
                !SparkColor.TryGet(self, out var sparkColor) || !GrindVineSpeed.TryGet(self, out var grindVineSpeed) ||
                self.SlugCatClass != Enums.TheVinki)
            {
                return;
            }

            // Save the last direction that Vinki was facing
            if (self.input[0].x != 0)
            {
                lastXDirection = self.input[0].x;
            }
            if (self.input[0].y != 0)
            {
                lastYDirection = self.input[0].y;
            }
            if (self.SwimDir(true).magnitude > 0f)
            {
                lastVineDir = self.SwimDir(true);
            }
            // Save the last animation
            if (self.animation != lastAnimationFrame)
            {
                lastAnimation = lastAnimationFrame;
                lastAnimationFrame = self.animation;
            }

            // If grinding up a pole and reach the top, jump up high
            if (self.animation == Player.AnimationIndex.GetUpToBeamTip && isGrindingV)
            {
                if (self.input[0].jmp)
                {
                    grindUpPoleFlag = true;
                    self.Jump();
                }
                else
                {
                    grindUpPoleFlag = false;
                    self.bodyChunks[1].pos = self.room.MiddleOfTile(self.bodyChunks[1].pos) + new Vector2(0f, 5f);
                    self.bodyChunks[1].vel *= 0f;
                    self.bodyChunks[0].vel = Vector2.ClampMagnitude(self.bodyChunks[0].vel, 9f);
                }
            }
            else
            {
                grindUpPoleFlag = false;
            }

            // If player isn't holding Grind, no need to do other stuff
            if (!self.IsPressed(Grind) && !grindToggle)
            {
                isGrindingH = isGrindingV = isGrindingNoGrav = isGrindingVine = false;
                self.slugcatStats.runspeedFac = normalXSpeed;
                self.slugcatStats.poleClimbSpeedFac = normalYSpeed;
                return;
            }

            isGrindingH = IsGrindingHorizontally(self);
            isGrindingV = IsGrindingVertically(self);
            isGrindingNoGrav = IsGrindingNoGrav(self);
            isGrindingVine = IsGrindingVine(self);
            isGrinding = isGrindingH || isGrindingV || isGrindingNoGrav || isGrindingVine;

            // Grind horizontally if holding Grind on a beam
            if (isGrindingH)
            {
                self.slugcatStats.runspeedFac = 0;
                self.bodyChunks[1].vel.x = grindXSpeed * lastXDirection;

                // Sparks from grinding
                Vector2 pos = self.bodyChunks[1].pos;
                Vector2 posB = pos - new Vector2(10f * lastXDirection, 0);
                for (int j = 0; j < 2; j++)
                {
                    Vector2 a = RWCustom.Custom.RNV();
                    a.x = Mathf.Abs(a.x) * -lastXDirection;
                    a.y = Mathf.Abs(a.y);
                    self.room.AddObject(new Spark(pos, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), sparkColor, null, 2, 4));
                    self.room.AddObject(new Spark(posB, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), sparkColor, null, 2, 4));
                }

                // Looping grind sound
                PlayGrindSound(self);
            }
            else
            {
                self.slugcatStats.runspeedFac = normalXSpeed;
            }

            // Grind if holding Grind on a pole (vertical beam or 0G beam or vine)
            if (isGrindingV || isGrindingNoGrav || isGrindingVine)
            {
                //Debug.Log("Zero G Pole direction: " + self.zeroGPoleGrabDir.x + "," + self.zeroGPoleGrabDir.y);
                self.slugcatStats.poleClimbSpeedFac = 0;

                // Handle vine grinding
                if (isGrindingVine)
                {
                    self.vineClimbCursor = grindVineSpeed * Vector2.ClampMagnitude(self.vineClimbCursor + lastVineDir * Custom.LerpMap(Vector2.Dot(lastVineDir, self.vineClimbCursor.normalized), -1f, 1f, 10f, 3f), 30f);
                    Vector2 a6 = self.room.climbableVines.OnVinePos(self.vinePos);
                    self.vinePos.floatPos += self.room.climbableVines.ClimbOnVineSpeed(self.vinePos, self.mainBodyChunk.pos + self.vineClimbCursor) * Mathf.Lerp(2.1f, 1.5f, self.EffectiveRoomGravity) / self.room.climbableVines.TotalLength(self.vinePos.vine);
                    self.vinePos.floatPos = Mathf.Clamp(self.vinePos.floatPos, 0f, 1f);
                    self.room.climbableVines.PushAtVine(self.vinePos, (a6 - self.room.climbableVines.OnVinePos(self.vinePos)) * 0.05f);
                    if (self.vineGrabDelay == 0 && (!ModManager.MMF || !self.GrabbedByDaddyCorruption))
                    {
                        ClimbableVinesSystem.VinePosition vinePosition = self.room.climbableVines.VineSwitch(self.vinePos, self.mainBodyChunk.pos + self.vineClimbCursor, self.mainBodyChunk.rad);
                        if (vinePosition != null)
                        {
                            self.vinePos = vinePosition;
                            self.vineGrabDelay = 10;
                        }
                    }
                }
                else
                {
                    // Handle 0G horizontal beam grinding
                    if (isGrindingNoGrav && self.room.GetTile(self.mainBodyChunk.pos).horizontalBeam)
                    {
                        self.bodyChunks[0].vel.x = grindYSpeed * lastXDirection;
                    }
                    else
                    {
                        // This works in gravity and no gravity
                        self.bodyChunks[0].vel.y = grindYSpeed * lastYDirection;
                    }
                }

                // Sparks from grinding
                Vector2 pos = (self.graphicsModule as PlayerGraphics).hands[0].pos;
                Vector2 posB = (self.graphicsModule as PlayerGraphics).hands[1].pos;
                for (int j = 0; j < 2; j++)
                {
                    Vector2 a = RWCustom.Custom.RNV();
                    a.x = Mathf.Abs(a.x) * lastXDirection;
                    a.y = Mathf.Abs(a.y) * -lastYDirection;
                    self.room.AddObject(new Spark(pos, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), sparkColor, null, 2, 4));
                    self.room.AddObject(new Spark(posB, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), sparkColor, null, 2, 4));
                }

                // Looping grind sound
                PlayGrindSound(self);
            }
            else
            {
                self.slugcatStats.poleClimbSpeedFac = normalYSpeed;
            }

            // Catch beam with feet if not holding down
            if (self.bodyMode == Player.BodyModeIndex.Default && 
                self.input[0].y >= 0 && self.room.GetTile(self.bodyChunks[1].pos).horizontalBeam &&
                self.bodyChunks[0].vel.y < 0f)
            {
                self.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, self.mainBodyChunk, false, 0.75f, 1f);
                self.noGrabCounter = 15;
                self.animation = Player.AnimationIndex.StandOnBeam;
                self.bodyChunks[1].pos.y = self.room.MiddleOfTile(self.bodyChunks[1].pos).y + 5f;
                self.bodyChunks[1].vel.y = 0f;
                self.bodyChunks[0].vel.y = 0f;
            }

            // Stop flipping when falling fast and letting go of jmp (so landing on a rail doesn't look weird)
            if (self.animation == Player.AnimationIndex.Flip)
            {
                if (self.bodyChunks[0].vel.y < -3f && !self.input[0].jmp)
                {
                    self.animation = Player.AnimationIndex.HangFromBeam;
                }
            }
        }

        private static void PlayGrindSound(Player self)
        {
            if (grindSound == null || grindSound.currentSoundObject == null || grindSound.currentSoundObject.slatedForDeletion)
            {
                grindSound = self.room.PlaySound(SoundID.Shelter_Gasket_Mover_LOOP, self.mainBodyChunk, true, 0.15f, 10f);
                grindSound.requireActiveUpkeep = true;
            }
            grindSound.alive = true;
        }

        private static bool IsGrindingHorizontally(Player self)
        {
            return (self.animation == Player.AnimationIndex.StandOnBeam && 
                self.bodyChunks[0].vel.magnitude > 3f);
        }

        private static bool IsGrindingVertically(Player self)
        {
            return (self.animation == Player.AnimationIndex.ClimbOnBeam && 
                ((lastYDirection > 0 && self.bodyChunks[1].vel.magnitude > 2f) ||
                (lastYDirection < 0 && self.bodyChunks[1].vel.magnitude > 1f)));
        }

        private static bool IsGrindingNoGrav(Player self)
        {
            return (self.animation == Player.AnimationIndex.ZeroGPoleGrab &&
                self.bodyChunks[0].vel.magnitude > 1f);
        }

        private static bool IsGrindingVine(Player self)
        {
            return (self.animation == Player.AnimationIndex.VineGrab && 
                self.bodyChunks[0].vel.magnitude > 1f);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (self.SlugCatClass != Enums.TheVinki)
            {
                return;
            }

            // Update grindToggle if needed
            if (self.JustPressed(ToggleGrind) && VinkiConfig.ToggleGrind.Value && !IsPressingGraffiti(self))
            {
                grindToggle = !grindToggle;
            }

            // Spray a random graffiti
            if (self.JustPressed(Spray) && IsPressingGraffiti(self))
            {
                if (!VinkiConfig.RequireSprayCans.Value)
                {
                    _ = SprayGraffiti(self);
                }
                else
                {
                    var grasp = self.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan);
                    if (grasp != null && (grasp.grabbed as SprayCan).TryUse())
                    {
                        _ = SprayGraffiti(self);
                    }
                }
            }
            // Craft SprayCan
            else if (self.IsPressed(Craft) && IsPressingGraffiti(self))
            {
                int sprayCount = CanCraftSprayCan(self.grasps[0], self.grasps[1]);
                if (sprayCount > 0)
                {
                    craftCounter++; 

                    if (craftCounter > 30)
                    {
                        for (int num13 = 0; num13 < 2; num13++)
                        {
                            self.bodyChunks[0].pos += Custom.DirVec(self.grasps[num13].grabbed.firstChunk.pos, self.bodyChunks[0].pos) * 2f;
                            (self.graphicsModule as PlayerGraphics).swallowing = 20;
                        }

                        var tilePosition = self.room.GetTilePosition(self.mainBodyChunk.pos);
                        var pos = new WorldCoordinate(self.room.abstractRoom.index, tilePosition.x, tilePosition.y, 0);
                        var abstr = new SprayCanAbstract(self.room.world, pos, self.room.game.GetNewID(), sprayCount);
                        abstr.Realize();
                        self.room.abstractRoom.AddEntity(abstr);
                        self.room.AddObject(abstr.realizedObject);

                        // Remove grabbed objects used for crafting
                        for (int j = 0; j < self.grasps.Length; j++)
                        {
                            AbstractPhysicalObject apo = self.grasps[j].grabbed.abstractPhysicalObject;
                            if (self.room.game.session is StoryGameSession)
                            {
                                (self.room.game.session as StoryGameSession).RemovePersistentTracker(apo);
                            }
                            self.ReleaseGrasp(j);
                            for (int k = apo.stuckObjects.Count - 1; k >= 0; k--)
                            {
                                if (apo.stuckObjects[k] is AbstractPhysicalObject.AbstractSpearStick && apo.stuckObjects[k].A.type == AbstractPhysicalObject.AbstractObjectType.Spear && apo.stuckObjects[k].A.realizedObject != null)
                                {
                                    (apo.stuckObjects[k].A.realizedObject as Spear).ChangeMode(Weapon.Mode.Free);
                                }
                            }
                            apo.LoseAllStuckObjects();
                            apo.realizedObject.RemoveFromRoom();
                            self.room.abstractRoom.RemoveEntity(apo);
                        }

                        self.SlugcatGrab(abstr.realizedObject, self.FreeHand());
                        craftCounter = 0;
                    }
                }
                else if (craftCounter > 0)
                {
                    craftCounter--;
                }
            }
            else if (craftCounter > 0)
            {
                craftCounter--;
            }
        }

        private static int CanCraftSprayCan(Creature.Grasp a, Creature.Grasp b)
        {
            // You can craft while moving if you're not grinding
            if (a == null || b == null || isGrinding)
            {
                return 0;
            }

            AbstractPhysicalObject.AbstractObjectType abstractObjectType = a.grabbed.abstractPhysicalObject.type;
            AbstractPhysicalObject.AbstractObjectType abstractObjectType2 = b.grabbed.abstractPhysicalObject.type;

            if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Rock && 
                colorfulItems.ContainsKey(abstractObjectType2))
            {
                return colorfulItems[abstractObjectType2];
            }
            if (abstractObjectType2 == AbstractPhysicalObject.AbstractObjectType.Rock &&
                colorfulItems.ContainsKey(abstractObjectType))
            {
                return colorfulItems[abstractObjectType];
            }

            // Upgrade Spray Can
            if (abstractObjectType.ToString() == "SprayCan" && abstractObjectType2.ToString() == "SprayCan")
            {
                return Math.Min(3, (a.grabbed as SprayCan).Abstr.uses + (b.grabbed as SprayCan).Abstr.uses);
            }
            if (abstractObjectType.ToString() == "SprayCan" && colorfulItems.ContainsKey(abstractObjectType2))
            {
                return Math.Min(3, (a.grabbed as SprayCan).Abstr.uses + colorfulItems[abstractObjectType2]);
            }
            if (abstractObjectType2.ToString() == "SprayCan" && colorfulItems.ContainsKey(abstractObjectType))
            {
                return Math.Min(3, (b.grabbed as SprayCan).Abstr.uses + colorfulItems[abstractObjectType]);
            }
            return 0;
        }

        private static bool IsPressingGraffiti(Player self)
        {
            return self.IsPressed(Graffiti) || (VinkiConfig.UpGraffiti.Value && self.input[0].y > 0f);
        }
    }
}