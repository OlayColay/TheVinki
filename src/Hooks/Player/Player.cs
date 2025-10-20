using System;
using UnityEngine;
using RWCustom;
using System.Linq;
using SprayCans;
using static Vinki.Plugin;
using System.Threading.Tasks;
using SlugBase.SaveData;
using Smoke;
using System.Collections.Generic;
using Menu;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Vinki
{
    public static partial class Hooks
    {
        public static async Task SprayGraffiti(Player self, int gNum = -1)
        {
            string slugcat = graffitis.ContainsKey(self.slugcatStats.name.value.ToLowerInvariant()) ? self.slugcatStats.name.value.ToLowerInvariant() : SlugcatStats.Name.White.value.ToLowerInvariant();
            if (gNum < 0)
            {
                // If not spraying a story graffiti, check if we've queued a specific graffiti from the selection menu
                if (queuedGNums[self.JollyOption.playerNumber] != -1)
                {
                    gNum = queuedGNums[self.JollyOption.playerNumber];
                    if (!repeatGraffiti[self.JollyOption.playerNumber])
                    {
                        queuedGNums[self.JollyOption.playerNumber] = -1;
                    }
                }
                else
                {
                    VinkiPlayerData vinki = self.Vinki();
                    do
                    {
                        gNum = UnityEngine.Random.Range(0, graffitis[slugcat].Count);
                    }
                    while (gNum == vinki.lastGraffitiSprayed && graffitis[slugcat].Count > 1);
                    vinki.lastGraffitiSprayed = gNum;
                }
            }
            else
            {
                slugcat = "Story";

                // Save that we sprayed this story graffiti
                SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
                miscSave.TryGet("StoryGraffitisSprayed", out int[] sprd);
                sprd ??= [];
                if (!sprd.Contains(gNum))
                {
                    sprd = [.. sprd, gNum];
                }
                miscSave.Set("StoryGraffitisSprayed", sprd);

                // If spraying the StoryGraffitiTutorial graffiti, move to the next phase
                if (gNum == 2)
                {
                    miscSave.Set("StoryGraffitiTutorialPhase", (int)StoryGraffitiTutorial.Phase.Explore);
                }

                // Flag that the map should open automatically when hibernating
                if (gNum < GraffitiQuestDialog.graffitiSpots.Length)
                {
                    miscSave.Set("AutoOpenMap", true);
                }

                IEnumerable<GraffitiHolder> roomGraffitiHolders = self.room.drawableObjects.OfType<GraffitiHolder>();
                roomGraffitiHolders.FirstOrDefault(x => x.gNum == gNum)?.RemoveFromRoom();

                roomGraffitiHolders.FirstOrDefault(x => x.gNum == storyGraffitiParameters[gNum].enableAfterSpray)?.Enable();

                // Spawn triggered creatures if there are any in this room
                if (roomGraffitiHolders.Count() <= 1)
                {
                    GraffitiCreatureSpawner.TriggerSpawns(self.room.abstractRoom);
                }
            }
            VLogger.LogInfo("Spraying " + slugcat + " #" + gNum + "\tsize: " + graffitis[slugcat][gNum].handles[1].ToString());

            // Trigger Iterator dialogue if there's one in the room
            bool isMoon;
            if ((isMoon = self.room.abstractRoom.name.Equals("DM_AI")) || self.room.abstractRoom.name.Equals("SS_AI"))
            {
                SprayNearIterator(isMoon, SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData), graffitis[slugcat][gNum].imageName);
            }

            bool hasStoryParams = slugcat == "Story" && storyGraffitiParameters.ContainsKey(gNum);
            Vector2 sprayPos = hasStoryParams ? storyGraffitiParameters[gNum].position : self.mainBodyChunk.pos;
            Room room = self.room;

            room.PlaySound(SoundID.Vulture_Jet_LOOP, self.mainBodyChunk, false, 1f, 2f);

            float alphaPerSmoke = hasStoryParams ? storyGraffitiParameters[gNum].alphaPerSmoke : 0.3f;
            for (int j = 0; j < 4; j++)
            {
                graffitis[slugcat][gNum].vertices[j, 0] = alphaPerSmoke;
            }

            int numSmokes = hasStoryParams ? storyGraffitiParameters[gNum].numSmokes : 10;
            int replacesGNum = hasStoryParams ? storyGraffitiParameters[gNum].replacesGNum : -1;
            for (int i = 0; i < numSmokes; i++)
            {
                PlacedObject graffiti = new(PlacedObject.Type.CustomDecal, graffitis[slugcat][gNum])
                {
                    pos = sprayPos - ((hasStoryParams && !storyGraffitiParameters[gNum].anchorToCenter) ? Vector2.zero : graffitiRadii[slugcat][gNum])
                };

                Vector2 smokePos = new(
                    sprayPos.x + UnityEngine.Random.Range(-graffitiRadii[slugcat][gNum].x, graffitiRadii[slugcat][gNum].x),
                    sprayPos.y + UnityEngine.Random.Range(-graffitiRadii[slugcat][gNum].y, graffitiRadii[slugcat][gNum].y));
                var smoke = new Explosion.ExplosionSmoke(smokePos, Vector2.zero, 2f)
                {
                    lifeTime = 15f,
                    life = 2f
                };
                room.AddObject(smoke);
                smoke.colorA = graffitiAvgColors[slugcat][gNum];
                smoke.colorB = Color.gray;

                await Task.Delay(100);
                room.AddObject(new GraffitiObject(graffiti, room.game.GetStorySession?.saveState, gNum, room.abstractRoom.name, slugcat == "Story"));

                // Remove a layer of a graffiti if this story graffiti is replacing it
                if (replacesGNum >= 0)
                {
                    GraffitiObject removedLayer = room.updateList.OfType<GraffitiObject>().FirstOrDefault(graffiti => graffiti.serializableGraffiti.gNum == replacesGNum);
                    if (removedLayer != null)
                    {
                        VLogger.LogInfo("Removing layer of " + graffitis["Story"][removedLayer.serializableGraffiti.gNum].imageName);
                        room.RemoveObject(removedLayer);
                    }
                    else
                    {
                        VLogger.LogError("Could not find layer of " + graffitis["Story"][removedLayer.serializableGraffiti.gNum].imageName + " to replace!");
                    }
                }
            }
        }

        // Add hooks
        private static void ApplyPlayerHooks()
        {
            try
            {
                IL.Player.TerrainImpact += Player_TerrainImpact_IL;
            }
            catch (Exception ex)
            {
                Plugin.VLogger.LogError("Could not apply TerrainImpact IL hook\n" + ex.Message);
            }

            On.Player.Jump += Player_Jump;
            On.Player.MovementUpdate += Player_Move;
            On.Player.Update += Player_Update;
            On.Player.JollyUpdate += Player_JollyUpdate;
            On.Player.CanBeSwallowed += Player_CanBeSwallowed;
            On.Player.TerrainImpact += Player_TerrainImpact_On;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.BiteEdibleObject += Player_BiteEdibleObject;
            On.Player.ThrownSpear += Player_ThrownSpear;
        }


        private static void RemovePlayerHooks()
        {
            On.Player.Jump -= Player_Jump;
            On.Player.MovementUpdate -= Player_Move;
            On.Player.Update -= Player_Update;
            On.Player.JollyUpdate -= Player_JollyUpdate;
            On.Player.CanBeSwallowed -= Player_CanBeSwallowed;
            On.Player.TerrainImpact -= Player_TerrainImpact_On;
            On.Player.GrabUpdate -= Player_GrabUpdate;
            On.Player.BiteEdibleObject -= Player_BiteEdibleObject;
            On.Player.ThrownSpear -= Player_ThrownSpear;
        }

        private static void Player_JollyUpdate(On.Player.orig_JollyUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (intro != null && self.room.abstractRoom.name != "SS_AI")
            {
                self.sleepCounter = 0;
                sleeping = false;
                self.JollyEmoteUpdate();
                self.JollyPointUpdate();
                intro = null;
            }
            else if (intro != null && intro.phase == CutsceneVinkiIntro.Phase.Wait && intro.cutsceneTimer > 2230)
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
            VinkiPlayerData v = self.Vinki();

            // Don't jump off a pole when we're trying to do a trick jump at the top
            if (v.isGrindingV && !v.grindUpPoleFlag && self.input[0].x == 0f && v.lastYDirection > 0)
            {
                return;
            }

            bool coyote = IsCoyoteJumping(self);
            v.canCoyote = 0;

            bool newFlip = self.animation != Player.AnimationIndex.Flip;
            bool newSlide = self.animation != Player.AnimationIndex.BellySlide;
            bool newPounce = self.animation != Player.AnimationIndex.RocketJump;

            orig(self);

            if (!CanTrickJump.TryGet(self, out bool canTrickJump) || !canTrickJump)
            {
                return;
            }

            // If player jumped or coyote jumped from a beam (or grinded to top of pole), then trick jump
            if (coyote || v.isGrindingH || v.grindUpPoleFlag || v.vineAtFeet != null)
            {
                // Separate from vine
                v.vineAtFeet = null;
                v.vineGrindDelay = 10;

                // Get num multiplier
                float num = Mathf.Lerp(1f, 1.15f, self.Adrenaline);
                if (self.grasps[0] != null && self.HeavyCarry(self.grasps[0].grabbed) && self.grasps[0].grabbed is not Cicada)
                {
                    num += Mathf.Min(Mathf.Max(0f, self.grasps[0].grabbed.TotalMass - 0.2f) * 1.5f, 1.3f);
                }

                // Randomly front or back flip
                self.flipDirection = UnityEngine.Random.value < 0.5f ? -1 : 1;

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
                self.slideDirection = v.lastXDirection;

                // Get risk/reward speedboost when coyote jumping
                if (coyote)
                {
                    //VLogger.LogInfo("Coyote jump!");
                    self.mainBodyChunk.vel.x += Enums.Movement.CoyoteBoost * self.slideDirection;
                    self.room.PlaySound(SoundID.Slugcat_Flip_Jump, self.mainBodyChunk, false, 3f, 1f);
                }
                else
                {
                    self.room.PlaySound(SoundID.Slugcat_Flip_Jump, self.mainBodyChunk, false, 1f, 1f);
                }

                self.jumpBoost *= Enums.Movement.SuperJump + (coyote ? 0.2f : 0f);
                self.animation = Player.AnimationIndex.Flip;
                self.slideCounter = 0;

                v.grindUpPoleFlag = false;
            }

            if (self.animation == Player.AnimationIndex.Flip && newFlip)
            {
                v.NewTrick(Enums.TrickType.Flip, false, true);
            }
            else if (self.animation == Player.AnimationIndex.BellySlide && newSlide)
            {
                v.NewTrick(Enums.TrickType.Slide);
            }
            else if (self.animation == Player.AnimationIndex.RocketJump && newPounce)
            {
                v.NewTrick(Enums.TrickType.Pounce);
            }
        }

        private static bool IsCoyoteJumping(Player self)
        {
            VinkiPlayerData v = self.Vinki();
            //VLogger.LogInfo("Last animation: " + v.lastAnimation.ToString() + "\t Can coyote jump: " + v.canCoyote +
            //    "\t This animation: " + self.animation.ToString() + "\t Body mode: " + self.bodyMode.ToString());
            return (self.bodyMode == Player.BodyModeIndex.Default || self.bodyMode == Player.BodyModeIndex.Stand) && v.canCoyote > 0;
        }

        // Implement higher beam speed
        private static void Player_Move(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!CanGrind.TryGet(self, out bool canGrind) || !canGrind)
            {
                return;
            }
            bool isVinki = self.IsVinki(out VinkiPlayerData v);

            // Edit isGrindings from last Update
            v.lastIsGrinding = v.isGrinding;
            v.lastIsGrindingH = v.isGrindingH;
            v.lastIsGrindingV = v.isGrindingV;
            v.lastIsGrindingVine = v.isGrindingVine;
            v.lastIsGrindingVineNoGrav = v.isGrindingVineNoGrav;
            v.lastIsGrindingNoGrav = v.isGrindingNoGrav;

            // Save the last direction that Vinki was facing
            bool switchX = false;
            bool switchY = false;
            if (self.input[0].x != 0)
            {
                switchX = v.lastXDirection != self.input[0].x;
                v.lastXDirection = self.input[0].x;
            }
            if (self.input[0].y != 0)
            {
                switchY = v.lastYDirection != self.input[0].y;
                v.lastYDirection = self.input[0].y;
            }
            if (self.SwimDir(true).magnitude > 0f)
            {
                v.lastVineDir = self.SwimDir(true);
            }
            // Save the last animation
            if (self.animation != v.lastAnimationFrame)
            {
                v.lastAnimation = v.lastAnimationFrame;
                v.lastAnimationFrame = self.animation;
            }
            // Turn Coyote Jump off if we are not in air AND not grinding
            if (self.canWallJump != 0 || (self.animation != Player.AnimationIndex.None && !v.isGrindingH))
            {
                v.canCoyote = 0;
            }
            else if (self.canJump == 5 && v.canCoyote > 0)
            {
                v.canCoyote--;
            }

            // If grinding up a pole and reach the top, jump up high
            if (self.animation == Player.AnimationIndex.GetUpToBeamTip && v.isGrindingV)
            {
                if (self.input[0].jmp)
                {
                    v.grindUpPoleFlag = true;
                    self.Jump();
                }
                else
                {
                    v.grindUpPoleFlag = false;
                    self.bodyChunks[1].pos = self.room.MiddleOfTile(self.bodyChunks[1].pos) + new Vector2(0f, 5f);
                    self.bodyChunks[1].vel *= 0f;
                    self.bodyChunks[0].vel = Vector2.ClampMagnitude(self.bodyChunks[0].vel, 9f);
                }
            }
            else
            {
                v.grindUpPoleFlag = false;
            }

            v.vineGrindDelay = Math.Max(0, v.vineGrindDelay - 1);

            // Add score for flipping/rolling
            if (self.animation == Player.AnimationIndex.Flip)
            {
                v.currentTrickScore += 2;
                v.comboTotalScore += v.comboSize + v.comboSize;
                v.comboTimerSpeed = 1;
            }
            else if (self.animation == Player.AnimationIndex.Roll || self.animation == Player.AnimationIndex.BellySlide || self.animation == Player.AnimationIndex.RocketJump)
            {
                if (self.animation == Player.AnimationIndex.Roll)
                {
                    v.currentTrickScore += 2;
                    v.comboTotalScore += v.comboSize + v.comboSize;
                }
                v.comboTimerSpeed = 0;
            }
            else
            {
                v.comboTimerSpeed = 2;
            }

            // If player isn't holding Grind, no need to do other stuff
            if (!self.IsPressed(Grind) && !v.grindToggle)
            {
                v.isGrindingH = v.isGrindingV = v.isGrindingNoGrav = v.isGrindingVineNoGrav = false;
                v.vineAtFeet = null;
                self.slugcatStats.runspeedFac = Enums.Movement.NormalXSpeed;
                self.slugcatStats.poleClimbSpeedFac = Enums.Movement.NormalYSpeed;
                return;
            }

            ClimbableVinesSystem.VinePosition vineAtFeet = v.vineAtFeet;
            v.isGrindingH = IsGrindingHorizontally(self);
            v.isGrindingV = IsGrindingVertically(self);
            v.isGrindingVine = IsGrindingVine(self, vineAtFeet);
            v.isGrindingNoGrav = IsGrindingNoGrav(self);
            v.isGrindingVineNoGrav = IsGrindingVineNoGrav(self);
            v.isGrinding = v.isGrindingH || v.isGrindingV || v.isGrindingVine || v.isGrindingNoGrav || v.isGrindingVineNoGrav;

            bool goodVineState = (v.vineGrindDelay == 0 &&
                self.animation != Player.AnimationIndex.ClimbOnBeam && self.animation != Player.AnimationIndex.HangFromBeam &&
                self.animation != Player.AnimationIndex.StandOnBeam && self.animation != Player.AnimationIndex.ClimbOnBeam &&
                self.animation != Player.AnimationIndex.HangUnderVerticalBeam && self.animation != Player.AnimationIndex.VineGrab && 
                (self.bodyMode == Player.BodyModeIndex.Default || self.bodyMode == Player.BodyModeIndex.Stand) &&
                self.bodyChunks[1].vel.y < 0f
            );
            if (!v.isGrindingVine && goodVineState)
            {
                vineAtFeet = self.room.climbableVines?.VineOverlap(self.bodyChunks[1].pos, self.bodyChunks[1].rad + 5f);
                v.isGrindingVine = IsGrindingVine(self, vineAtFeet);

                // First frame of grinding on vine
                if (v.isGrindingVine)
                {
                    //VLogger.LogInfo("Animation before getting on vine: " + self.animation.ToString() + "\t prev animation: " + lastAnimation.ToString());
                    self.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, self.mainBodyChunk, false, 0.75f, 1f);
                    self.noGrabCounter = 15;
                    self.animation = Player.AnimationIndex.StandOnBeam;
                }
            }

            // Grind horizontally if holding Grind on a beam
            if (v.isGrindingH || (v.isGrindingVine && goodVineState))
            {
                v.canCoyote = 3;
                self.slugcatStats.runspeedFac = 0;
                self.animationFrame = 0;
                if (v.isGrindingVine)
                {
                    self.canJump = 5;
                    self.vineGrabDelay = 3;
                    self.room.climbableVines.VineBeingClimbedOn(vineAtFeet, self);
                    self.room.climbableVines.ConnectChunkToVine(self.bodyChunks[1], vineAtFeet, self.room.climbableVines.VineRad(vineAtFeet));

                    Vector2 oldPos = self.room.climbableVines.OnVinePos(vineAtFeet);

                    // vines can "face" either direction, so we need to take that into account
                    Vector2 vineDir = self.room.climbableVines.VineDir(vineAtFeet);
                    float dot = vineDir.normalized.x * v.lastXDirection;
                    if (dot > 0f)
                    {
                        vineAtFeet.floatPos += Enums.Movement.GrindVineSpeed / self.room.climbableVines.TotalLength(vineAtFeet.vine);
                    }
                    else
                    {
                        vineAtFeet.floatPos -= Enums.Movement.GrindVineSpeed / self.room.climbableVines.TotalLength(vineAtFeet.vine);
                    }

                    // Fall off the vine if reached the end
                    if (vineAtFeet.floatPos <= 0f || vineAtFeet.floatPos >= 1f)
                    {
                        v.vineGrindDelay = 10;
                        v.vineAtFeet = null;
                    }
                    else
                    {
                        v.vineAtFeet = vineAtFeet;
                        //Vector2 grindDir = (self.room.climbableVines.OnVinePos(vineAtFeet) - self.bodyChunks[1].posA).normalized;
                        self.room.climbableVines.PushAtVine(vineAtFeet, (oldPos - self.room.climbableVines.OnVinePos(vineAtFeet)) * 0.05f);
                    }
                }
                else
                {
                    self.bodyChunks[1].vel.x = Enums.Movement.GrindXSpeed * v.lastXDirection;
                }
                
                // Sparks from grinding
                Vector2 posA;
                Vector2 posB;
                if (v.isGrindingVine)
                {
                    posA = self.room.climbableVines.OnVinePos(vineAtFeet);
                    ClimbableVinesSystem.VinePosition nextVinePos = self.room.climbableVines?.VineOverlap(posA - new Vector2(10f * v.lastXDirection, 0), self.bodyChunks[1].rad + 5f);
                    posB = self.room.climbableVines.OnVinePos(nextVinePos);
                    
                    if (!v.lastIsGrindingVine || switchX)
                    {
                        v.NewTrick(Enums.TrickType.Vine, v.AddVineToCombo(self.room, vineAtFeet.vine));
                    }
                }
                else
                {
                    posA = self.bodyChunks[1].pos;
                    posB = posA - new Vector2(10f * v.lastXDirection, 0);

                    if (isVinki && !v.lastIsGrindingH)
                    {
                        Room.Tile beamTile = self.room.GetTile(self.bodyChunks[1].pos);
                        v.NewTrick(Enums.TrickType.HorizontalBeam, v.AddBeamToCombo(self.room, beamTile));
                    }
                }
                for (int j = 0; j < 2; j++)
                {
                    Vector2 a = Custom.RNV();
                    a.x = Mathf.Abs(a.x) * -v.lastXDirection;
                    a.y = Mathf.Abs(a.y);
                    self.room.AddObject(new Spark(posA, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Enums.SparkColor, null, 2, 4));
                    self.room.AddObject(new Spark(posB, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Enums.SparkColor, null, 2, 4));
                }

                // Looping grind sound
                PlayGrindSound(self);
            }
            else
            {
                self.slugcatStats.runspeedFac = Enums.Movement.NormalXSpeed;
            }

            // Grind if holding Grind on a pole (vertical beam or 0G beam/vine)
            if (v.isGrindingV || v.isGrindingNoGrav || v.isGrindingVineNoGrav)
            {
                //VLogger.LogInfo("Zero G Pole direction: " + self.zeroGPoleGrabDir.x + "," + self.zeroGPoleGrabDir.y);
                self.slugcatStats.poleClimbSpeedFac = 0;
                self.animationFrame = 0;

                // Handle 0G vine grinding
                if (v.isGrindingVineNoGrav)
                {
                    self.vineClimbCursor = Enums.Movement.GrindVineSpeed * Vector2.ClampMagnitude(
                        self.vineClimbCursor + v.lastVineDir * Custom.LerpMap(Vector2.Dot(v.lastVineDir, self.vineClimbCursor.normalized), -1f, 1f, 10f, 3f), 30f
                    );
                    Vector2 a6 = self.room.climbableVines.OnVinePos(self.vinePos);
                    self.vinePos.floatPos += self.room.climbableVines.ClimbOnVineSpeed(self.vinePos, self.mainBodyChunk.pos + self.vineClimbCursor) * 
                        Mathf.Lerp(2.1f, 1.5f, self.EffectiveRoomGravity) / self.room.climbableVines.TotalLength(self.vinePos.vine);
                    self.vinePos.floatPos = Mathf.Clamp(self.vinePos.floatPos, 0f, 1f);
                    self.room.climbableVines.PushAtVine(self.vinePos, (a6 - self.room.climbableVines.OnVinePos(self.vinePos)) * 0.05f);
                    if (self.vineGrabDelay == 0 && (!ModManager.MMF || !self.GrabbedByDaddyCorruption))
                    {
                        ClimbableVinesSystem.VinePosition vinePosition = self.room.climbableVines.VineSwitch(
                            self.vinePos, self.mainBodyChunk.pos + self.vineClimbCursor, self.mainBodyChunk.rad
                        );
                        if (vinePosition != null)
                        {
                            self.vinePos = vinePosition;
                            self.vineGrabDelay = 10;
                        }
                    }

                    if (isVinki && !v.lastIsGrindingVineNoGrav)
                    {
                        v.NewTrick(Enums.TrickType.ZeroGravity, v.AddVineToCombo(self.room, self.vinePos.vine));
                    }
                }
                else
                {
                    if (isVinki && v.isGrindingNoGrav && (!v.lastIsGrindingNoGrav || (self.standing && switchY) || (!self.standing && switchX)))
                    {
                        Room.Tile beamTile = self.room.GetTile(self.mainBodyChunk.pos);
                        v.NewTrick(Enums.TrickType.ZeroGravity, v.AddBeamToCombo(self.room, beamTile, self.standing));
                    }

                    // Handle 0G horizontal beam grinding
                    if (v.isGrindingNoGrav && self.room.GetTile(self.mainBodyChunk.pos).horizontalBeam)
                    {
                        self.bodyChunks[0].vel.x = Enums.Movement.GrindYSpeed * v.lastXDirection;
                    }
                    else
                    {
                        // This works in gravity and no gravity
                        self.bodyChunks[0].vel.y = Enums.Movement.GrindYSpeed * v.lastYDirection;
                    }
                }

                // Sparks from grinding
                Vector2 pos = new(self.room.MiddleOfTile(self.PlayerGraphics().legs.pos).x, self.PlayerGraphics().legs.pos.y);
                Vector2 posB = pos + new Vector2(0f, -3f);
                for (int j = 0; j < 2; j++)
                {
                    Vector2 a = Custom.RNV();
                    a.x = Mathf.Abs(a.x) * v.lastXDirection;
                    a.y = Mathf.Abs(a.y) * -v.lastYDirection;
                    Vector2 b = new(-a.x, a.y);
                    self.room.AddObject(new Spark(pos, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Enums.SparkColor, null, 2, 4));
                    self.room.AddObject(new Spark(posB, b * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Enums.SparkColor, null, 2, 4));
                }

                // Resume the combo timer if the direction is switched on a new beam/vine
                Plugin.VLogger.LogInfo($"{v.isGrindingVine} {v.lastIsGrindingVine} {switchX}");
                if (isVinki && v.isGrindingV && (!v.lastIsGrindingV || switchY))
                {
                    Room.Tile beamTile = self.room.GetTile(self.mainBodyChunk.pos);
                    v.NewTrick(Enums.TrickType.VerticalBeam, v.AddBeamToCombo(self.room, beamTile), v.lastYDirection < 0);
                }

                // Looping grind sound
                PlayGrindSound(self);
            }
            else
            {
                self.slugcatStats.poleClimbSpeedFac = Enums.Movement.NormalYSpeed;
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

            // Update combo states
            if (isVinki && v.isGrinding)
            {
                // Don't swallow/spitup while grinding
                self.swallowAndRegurgitateCounter = 0;

                v.currentTrickScore++;
                v.comboTotalScore += v.comboSize;
                v.comboTimerSpeed = v.currentTrickName.EndsWith("[NEW]") && self.animation != Player.AnimationIndex.VineGrab ? 0 : 1;
            }
        }

        private static void PlayGrindSound(Player self)
        {
            VinkiPlayerData v = self.Vinki();
            if (v.grindSound == null || v.grindSound.currentSoundObject == null || v.grindSound.currentSoundObject.slatedForDeletion)
            {
                v.grindSound = self.room.PlaySound(Enums.Sound.Grind1A, self.mainBodyChunk, true, 1f, 1f, true);
                v.grindSound.requireActiveUpkeep = true;
            }
            v.grindSound.alive = true;
        }

        private static bool IsGrindingHorizontally(Player self)
        {
            return (self.animation == Player.AnimationIndex.StandOnBeam &&
                self.bodyChunks[0].vel.magnitude > 3f);
        }

        private static bool IsGrindingVertically(Player self)
        {
            VinkiPlayerData v = self.Vinki();
            return (self.animation == Player.AnimationIndex.ClimbOnBeam &&
                ((v.lastYDirection > 0 && self.bodyChunks[1].vel.magnitude > 1f) ||
                (v.lastYDirection < 0 && self.bodyChunks[1].vel.magnitude > 2f)));
        }

        private static bool IsGrindingVine(Player self, ClimbableVinesSystem.VinePosition vinePos)
        {
            if (vinePos == null || self.room.climbableVines == null)
            {
                return false;
            }
            IClimbableVine vine = self.room.climbableVines.GetVineObject(vinePos);
            return (self.room.gravity >= 0.1f && (vine is not PoleMimic || (vine as PoleMimic).State.alive));
        }

        private static bool IsGrindingNoGrav(Player self)
        {
            return (self.animation == Player.AnimationIndex.ZeroGPoleGrab &&
                self.bodyChunks[0].vel.magnitude > 1f);
        }

        private static bool IsGrindingVineNoGrav(Player self)
        {
            return (self.animation == Player.AnimationIndex.VineGrab &&
                self.bodyChunks[0].vel.magnitude > 1f &&
                self.room.gravity <= 0.1f);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            // Spray a random graffiti, or tag a creature if there's one to tag
            if (IsPressingGraffiti(self) || !improvedInput)
            {
                if (self.JustPressed(Tag) && self.SlugCatClass == Enums.vinki && self.Vinki().tagableBodyChunk != null)
                {
                    if (!improvedInput)
                    {
                        self.wantToJump = 0;
                    }
                    TagCreature(self);
                }
                else if (self.JustPressed(Spray) && self.stun < 1)
                {
                    if (!improvedInput)
                    {
                        self.wantToJump = 0;
                    }
                    SprayGraffitiInGame(self);
                }
            }

            orig(self, eu);

            if (self.SlugCatClass != Enums.vinki)
            {
                return;
            }
            VinkiPlayerData v = self.Vinki();

            // Update grindToggle if needed
            if (self.JustPressed(ToggleGrind) && (!IsPressingGraffiti(self) || !VinkiConfig.UseGraffitiButton.Value))
            {
                v.grindToggle = !v.grindToggle;
            }

            // Craft SprayCan
            else if (self.IsPressed(Craft) && IsPressingGraffiti(self))
            {
                int sprayCount = CanCraftSprayCan(self.grasps[0], self.grasps[1]);
                //VLogger.LogInfo("Crafted spray count: " + sprayCount);
                if (sprayCount > 0)
                {
                    v.craftCounter++;

                    if (v.craftCounter > 30)
                    {
                        for (int num13 = 0; num13 < 2; num13++)
                        {
                            self.bodyChunks[0].pos += Custom.DirVec(self.grasps[num13].grabbed.firstChunk.pos, self.bodyChunks[0].pos) * 2f;
                            (self.graphicsModule as PlayerGraphics).swallowing = 20;
                        }

                        // If crafting in Moon's room, it could trigger some of her dialogue
                        if (self.room?.abstractRoom?.name == "DM_AI")
                        {
                            CraftNearMoon(self);
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
                                if (apo.stuckObjects[k] is AbstractPhysicalObject.AbstractSpearStick && 
                                    apo.stuckObjects[k].A.type == AbstractPhysicalObject.AbstractObjectType.Spear && 
                                    apo.stuckObjects[k].A.realizedObject != null)
                                {
                                    (apo.stuckObjects[k].A.realizedObject as Spear).ChangeMode(Weapon.Mode.Free);
                                }
                            }
                            apo.LoseAllStuckObjects();
                            apo.realizedObject.RemoveFromRoom();
                            self.room.abstractRoom.RemoveEntity(apo);
                        }

                        self.SlugcatGrab(abstr.realizedObject, self.FreeHand());
                        v.craftCounter = 0;
                    }
                }
                else if (v.craftCounter > 0)
                {
                    v.craftCounter--;
                }
            }
            else if (v.craftCounter > 0)
            {
                v.craftCounter--;
            }

            // Give Vinki Survivor throwing skill if doing fancy tricks
            if (self.animation == Player.AnimationIndex.Flip || self.animation == Player.AnimationIndex.Roll ||
                self.animation == Player.AnimationIndex.BellySlide || v.isGrinding)
            {
                self.slugcatStats.throwingSkill = 1;
            }
            else
            {
                self.slugcatStats.throwingSkill = 0;
            }

            CheckForTagging(self, v);

            // Head bop on song's beat
            if (curMsPerBeat > 0 && self.input[0].x == 0 && 
                (self.bodyMode == Player.BodyModeIndex.Stand || self.animation == Player.AnimationIndex.StandOnBeam || self.animation == Player.AnimationIndex.BeamTip))
            {
                v.idleUpdates++;
            }
            else
            {
                v.idleUpdates = 0;
            }

            // End combo if the timer runs out or you die
            if (self.dead || (v.timeLeftInCombo > -50 && v.timeLeftInCombo <= 0))
            {
                SlugBaseSaveData miscProgression = SaveDataExtension.GetSlugBaseData(self.room.game.rainWorld.progression.miscProgressionData);
                if (!miscProgression.TryGet("VinkiBestCombo", out int bestCombo) || v.comboSize > bestCombo)
                {
                    miscProgression.Set("VinkiBestCombo", v.comboSize);
                    v.OnNewBestCombo?.Invoke();
                }
                if (!miscProgression.TryGet("VinkiBestScore", out int bestScore) || v.comboTotalScore > bestScore)
                {
                    miscProgression.Set("VinkiBestScore", v.comboTotalScore);
                    v.OnNewBestScore?.Invoke();
                }

                v.timeLeftInCombo = -50;
                v.comboSize = v.currentTrickScore = v.comboTotalScore = 0;
                v.beamsInCombo.Clear();
                v.vinesInCombo.Clear();
            }

            // Update combo timer
            if (v.timeLeftInCombo > 0)
            {
                v.timeLeftInCombo -= v.comboTimerSpeed;
            }
        }

        private static int CanCraftSprayCan(Creature.Grasp a, Creature.Grasp b)
        {
            //VLogger.LogInfo("CRAFTING: " + (a == null ? "nothing" : a.grabbed.abstractPhysicalObject.type.ToString()) + " + " + (b == null ? "nothing" : b.grabbed.abstractPhysicalObject.type.ToString()));
            // You can craft while moving
            if (a == null || a.grabbed == null || b == null || b.grabbed == null)
            {
                //VLogger.LogInfo("Item 1 or 2 is null");
                return 0;
            }

            AbstractPhysicalObject.AbstractObjectType abstractObjectTypeA = a.grabbed.abstractPhysicalObject.type;
            AbstractPhysicalObject.AbstractObjectType abstractObjectTypeB = b.grabbed.abstractPhysicalObject.type;

            if (a.grabbed is Rock && colorfulItems.ContainsKey(abstractObjectTypeB))
            {
                //VLogger.LogInfo("Item 1 is rock and Item 2 is " + abstractObjectTypeB.ToString() + " and worth " + colorfulItems[abstractObjectTypeB]);
                return colorfulItems[abstractObjectTypeB];
            }
            if (b.grabbed is Rock && colorfulItems.ContainsKey(abstractObjectTypeA))
            {
                //VLogger.LogInfo("Item 2 is rock and Item 1 is " + abstractObjectTypeA.ToString() + " and worth " + colorfulItems[abstractObjectTypeA]);
                return colorfulItems[abstractObjectTypeA];
            }

            // Upgrade Spray Can
            if (a.grabbed is SprayCan && b.grabbed is SprayCan)
            {
                //VLogger.LogInfo("Crafting two cans with uses: " + (a.grabbed as SprayCan).Abstr.uses + " and " + (b.grabbed as SprayCan).Abstr.uses);
                if ((a.grabbed as SprayCan).Abstr.uses >= 5 || (b.grabbed as SprayCan).Abstr.uses >= 5)
                {
                    return 0;
                }
                return Math.Min(5, (a.grabbed as SprayCan).Abstr.uses + (b.grabbed as SprayCan).Abstr.uses);
            }
            if (a.grabbed is SprayCan && colorfulItems.ContainsKey(abstractObjectTypeB))
            {
                //VLogger.LogInfo("Crafting first hand can with uses: " + (a.grabbed as SprayCan).Abstr.uses + " and " + abstractObjectTypeB.ToString() + " worth " + colorfulItems[abstractObjectTypeB]);
                if (colorfulItems[abstractObjectTypeB] > 9000)
                {
                    return 9001;
                }
                if ((a.grabbed as SprayCan).Abstr.uses >= 5)
                {
                    return 0;
                }
                return Math.Min(5, (a.grabbed as SprayCan).Abstr.uses + colorfulItems[abstractObjectTypeB]);
            }
            if (b.grabbed is SprayCan && colorfulItems.ContainsKey(abstractObjectTypeA))
            {
                //VLogger.LogInfo("Crafting second hand can with uses: " + (b.grabbed as SprayCan).Abstr.uses + " and " + abstractObjectTypeA.ToString() + " worth " + colorfulItems[abstractObjectTypeA]);
                if (colorfulItems[abstractObjectTypeA] > 9000)
                {
                    return 9001;
                }
                if ((b.grabbed as SprayCan).Abstr.uses >= 5)
                {
                    return 0;
                }
                return Math.Min(5, (b.grabbed as SprayCan).Abstr.uses + colorfulItems[abstractObjectTypeA]);
            }
            //VLogger.LogInfo("None of the cases are covered");
            return 0;
        }

        private static bool IsPressingGraffiti(Player self)
        {
            return self.IsPressed(Graffiti) || (VinkiConfig.UpGraffiti.Value && self.input[0].y > 0f) || !VinkiConfig.UseGraffitiButton.Value;
        }

        private static void SprayGraffitiInGame(Player self)
        {
            var storyGraffitisInRoom = storyGraffitiParameters.Where(e => e.Value.room == self.room.abstractRoom.name);
            bool storyGraffitisExist = false;
            bool hologramsExist = false;
            int gNum = -1;

            if (self.room.game.GetStorySession != null)
            {
                SlugBaseSaveData miscWorldSave = SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
                storyGraffitisExist = miscWorldSave.TryGet("StoryGraffitisSprayed", out int[] sprayedGNums);
                sprayedGNums ??= [];
                hologramsExist = HologramsEnabledInRoom(self.room, miscWorldSave);

                // Check if we are in the right place to spray a story graffiti
                foreach (var storyGraffiti in storyGraffitisInRoom)
                {
                    var graf = graffitis["Story"][storyGraffiti.Key];
                    var storyGraffitiParams = storyGraffitiParameters[storyGraffiti.Key];
                    bool replaceeNeeded = storyGraffitiParams.replacesGNum >= 0 && !sprayedGNums.Contains(storyGraffitiParams.replacesGNum);
                    if (graf != null && (!storyGraffitisExist || !sprayedGNums.Contains(storyGraffiti.Key)) && hologramsExist && !replaceeNeeded)
                    {
                        Vector2 grafRadius = graf.handles[1] / 2f;
                        Vector2 grafPos = storyGraffiti.Value.position + (storyGraffitiParams.anchorToCenter ? Vector2.zero : grafRadius);
                        if (self.mainBodyChunk.pos.x >= grafPos.x - grafRadius.x && self.mainBodyChunk.pos.x <= grafPos.x + grafRadius.x &&
                            self.mainBodyChunk.pos.y >= grafPos.y - grafRadius.y && self.mainBodyChunk.pos.y <= grafPos.y + grafRadius.y)
                        {
                            gNum = storyGraffiti.Key;

                            // Progress story graffiti tutorial if it's the right room
                            if (storyGraffiti.Value.room == "SS_D08")
                            { 
                                miscWorldSave.Set("StoryGraffitiTutorialPhase", (int)StoryGraffitiTutorial.Phase.Explore);
                            }

                            break;
                        }
                    }
                }  
            }

            var grasp = self.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan);
            bool hasCan = grasp != null && (grasp.grabbed as SprayCan).TryUse();
            if (hasCan || !VinkiConfig.RequireCansGraffiti.Value)
            {
                _ = SprayGraffiti(self, gNum);

                if (hasCan && self.IsVinki(out VinkiPlayerData vinki))
                {
                    vinki.AddTrickToCombo("New Piece", 500);
                }
            }
        }

        private static readonly float boxRadius = 50f;
        private static readonly float boxOffset = 50f;
        private static void CheckForTagging(Player self, VinkiPlayerData v)
        {
            // Wait before being able to tag again
            if (v.tagLag > 0)
            {
                v.tagLag--;
                v.tagSmoke.target.owner.graphicsModule.Tag().tagLag = v.tagLag;
                if (v.tagSmoke.room == self.room)
                {
                    v.tagSmoke.EmitSmoke(0.4f);
                }
                if (v.tagLag == 0)
                {
                    v.tagSmoke.RemoveFromRoom();
                    v.tagSmoke = null;
                }
            }

            // Go through poisoned creatures
            for (int i = 0; i < v.poisonedVictims.Count; i++)
            {
                if (v.poisonedVictims[i].timeLeft <= 0)
                {
                    v.poisonedVictims.Remove(v.poisonedVictims[i]);
                    i--;
                    continue;
                }

                v.poisonedVictims[i].creature.SetKillTag(self.abstractCreature);
                v.poisonedVictims[i].timeLeft--;
                (v.poisonedVictims[i].creature.State as HealthState).health -= v.poisonedVictims[i].damagePerTick;
            }

            if ((!IsPressingGraffiti(self) && improvedInput) || (VinkiConfig.RequireCansTagging.Value && self.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan) == null) ||
                self.room == null || (!self.Consious && (self.dangerGrasp == null || self.dangerGraspTime >= 30)) || self.dead)
            {
                v.tagableBodyChunk = null;
                return;
            }

            // Create box for where Vinki can tag creatures
            Vector2 boxCenter = new(self.mainBodyChunk.pos.x + (v.lastXDirection * boxOffset), self.mainBodyChunk.pos.y);
            float minX = boxCenter.x - boxRadius;
            float maxX = boxCenter.x + boxRadius;
            float minY = boxCenter.y - boxRadius;
            float maxY = boxCenter.y + boxRadius;

            // Find any creature body chunks in the room within the box
            List<BodyChunk> taggableBodyChunks = [];
            foreach (var creature in self.room.abstractRoom.creatures.Select((absCreature) => absCreature.realizedCreature).Where((c) => c != null))
            {
                if (creature is Spider || !creature.canBeHitByWeapons || creature.dead || creature == self || (creature is Lizard && (creature as Lizard).AI.friendTracker.friend != null) ||
                    creature is Fly || (creature is Centipede && (creature as Centipede).Small) || creature is Hazer || creature is VultureGrub || creature is SmallNeedleWorm ||
                    (creature is Player && (creature as Player).abstractCreature.abstractAI != null) || creature is Leech || creature is TubeWorm)
                {
                    continue;
                }

                foreach (var chunk in creature.bodyChunks)
                {
                    if (!chunk.collideWithObjects)
                    {
                        continue;
                    }

                    Vector2 cPos = chunk.pos;
                    if (cPos.x > minX && cPos.x < maxX && cPos.y > minY && cPos.y < maxY)
                    {
                        // Linecast to check if there's a solid tile that would block the spray
                        if (SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(self.room, self.mainBodyChunk.pos, cPos) == null ||
                            SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(self.room, self.bodyChunks[1].pos, cPos) == null)
                        {
                            taggableBodyChunks.Add(chunk);
                        }
                    }
                }
            }

            if (taggableBodyChunks.Count == 0)
            {
                v.tagableBodyChunk = null;
                v.tagableCreature = null;
            }
            else
            {
                v.tagableBodyChunk = taggableBodyChunks.OrderBy(chunk => Vector2.Distance(chunk.pos, self.mainBodyChunk.pos)).FirstOrDefault();
                v.tagableCreature = v.tagableBodyChunk.owner as Creature;
            }
        }

        private static void TagCreature(Player self)
        {
            VinkiPlayerData v = self.Vinki();
            if (v.tagLag > 0)
            {
                return;
            }

            PhysicalObject source = self;
            if (self.grasps.FirstOrDefault(g => g?.grabbed is SprayCan)?.grabbed is SprayCan can && can.TryUse())
            {
                source = can;

                v.AddTrickToCombo("Tag!", 700);
            }
            else if (VinkiConfig.RequireCansTagging.Value)
            {
                return;
            }

            v.tagLag = 30;

            float damage = 1f;
            if (v.tagableCreature is Player)
            {
                if (!VinkiConfig.TagDamageJolly.Value && self.room.game.IsStorySession)
                {
                    damage = 0f;
                }
                else
                {
                    damage = 0.5f;
                }
            }

            v.tagSmoke?.RemoveFromRoom();

            v.tagSmoke = new TagSmoke(self.room, source, v.tagableBodyChunk);
            self.room.AddObject(v.tagSmoke);
            v.tagSmoke.EmitSmoke(0.4f);
            v.tagSmoke.target.owner.graphicsModule.Tag().tagLag = 30;
            Color tagColor = new HSLColor(v.tagSmoke.hue, 0.8f, 0.5f).rgb;
            v.tagSmoke.target.owner.graphicsModule.Tag().targetedBodyPart = v.tagSmoke.target.index;
            if (v.tagSmoke.target.owner is not Lizard and not Player)
            {
                v.tagSmoke.target.owner.graphicsModule.Tag().tagColor = tagColor;
            }

            self.room.PlaySound(SoundID.Hazer_Squirt_Smoke_LOOP, self.mainBodyChunk, false, 2f, 1f);
            if (damage > 0f)
            {
                if (v.tagableCreature.State is HealthState)
                {
                    v.tagableCreature.Violence(self.firstChunk, null, v.tagableBodyChunk, null, Creature.DamageType.Stab, damage / 4f, 10f);
                    v.poisonedVictims.Add(new VinkiPlayerData.PoisonedCreature(v.tagableCreature, 120, damage * 0.75f));
                }
                else
                {
                    v.tagableCreature.Violence(self.firstChunk, null, v.tagableBodyChunk, null, Creature.DamageType.Stab, damage, 0f);
                    if (ModManager.MSC && v.tagableCreature is Player)
                    {
                        Player player = v.tagableCreature as Player;
                        player.playerState.permanentDamageTracking += damage / player.Template.baseDamageResistance;
                        if (player.playerState.permanentDamageTracking >= 1.0)
                        {
                            player.Die();
                        }
                    }
                }
                v.tagableCreature.InjectPoison(damage / 2f, tagColor);
            }
        }

        private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
        {
            if (orig(self, testObj))
            {
                return true;
            }
            
            // Don't want Spearmaster to be able to swallow a can, sorry Spear :(
            return (!ModManager.MSC || !(self.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear)) && (testObj is SprayCan);
        }

        private static void Player_TerrainImpact_On(On.Player.orig_TerrainImpact orig, Player self, int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            bool wasNotRolling = self.animation != Player.AnimationIndex.Roll;
            bool wasNotPouncing = self.animation != Player.AnimationIndex.RocketJump;

            orig(self, chunk, direction, speed, firstContact);

            if (!self.IsVinki())
            {
                return;
            }

            if (self.animation == Player.AnimationIndex.Roll && wasNotRolling)
            {
                self.Vinki().NewTrick(Enums.TrickType.Roll);
            }
            else if (self.animation == Player.AnimationIndex.RocketJump && wasNotPouncing)
            {
                self.Vinki().NewTrick(Enums.TrickType.Pounce, false, true);
            }
        }

        private static void Player_TerrainImpact_IL(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);

            try
            {
                c.GotoNext(MoveType.Before,
                    x => x.MatchLdcI4(0),
                    x => x.MatchStloc(5)
                );

                // Dying from fall damage requires speed to be 80 as Vinki (same as Gourmand)
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate((Player player, float deathSpeed) => player.IsVinki() ? 80f : deathSpeed);
                c.Emit(OpCodes.Stloc_0);

                // Getting stunned from fall damage requires speed to be 45 as Vinki
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_1);
                c.EmitDelegate((Player player, float stunSpeed) => player.IsVinki() ? 45f : stunSpeed);
                c.Emit(OpCodes.Stloc_1);
            }
            catch (Exception e)
            {
                Plugin.VLogger.LogError("Could not complete TerrainImpact IL Hook\n" + e.Message + '\n' + e.StackTrace);
            }
        }

        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            int prevCounter = self.eatCounter;

            orig(self, eu);

            if (!self.IsVinki(out VinkiPlayerData vinki))
            {
                return;
            }

            // Reset shake head counter if we stopped trying to eat
            if (self.eatCounter == prevCounter && vinki.shakeHeadTimes > 0)
            {
                vinki.shakeHeadTimes = 0;
                (self.graphicsModule as PlayerGraphics).LookAtNothing();
            }
        }

        private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player self, bool eu)
        {
            // Shake head instead of eating mushrooms
            for (int i = 0; i < 2; i++)
            {
                if (self.IsVinki(out VinkiPlayerData vinki) && self.grasps[i] != null && self.grasps[i].grabbed is Mushroom)
                {
                    if (vinki.shakeHeadTimes < 4)
                    {
                        if (vinki.shakeHeadDir == 0)
                        {
                            vinki.shakeHeadDir = 1;
                        }
                        else
                        {
                            vinki.shakeHeadDir *= -1;
                        }

                        self.eatCounter = 7;
                        self.PlayerGraphics().LookAtPoint(self.mainBodyChunk.pos + new Vector2(vinki.shakeHeadDir * 1000f, 0f), 10f);
                        vinki.shakeHeadTimes++;
                    }
                    else
                    {
                        self.ThrowObject(i, eu);
                    }

                    return;
                }
            }

            orig(self, eu);
        }
        private static void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
        {
            orig(self, spear);

            if (!self.IsVinki(out VinkiPlayerData vinki))
            {
                return;
            }

            if (vinki.timeLeftInCombo > 0f)
            {
                float comboDamageBonus = 0.02f * vinki.comboSize;
                spear.spearDamageBonus += comboDamageBonus;
                if (ModManager.MMF && MoreSlugcats.MMF.cfgUpwardsSpearThrow.Value &&
                    spear.setRotation.Value.y == 1f && self.bodyMode != Player.BodyModeIndex.ZeroG)
                {
                    spear.firstChunk.vel.y += comboDamageBonus * 0.67f;
                }
                else
                {
                    spear.firstChunk.vel.x += comboDamageBonus * 0.67f;
                }
            }
        }

        public static void PushToMeowMain_DoMeow(Action<object, Player, bool> orig, object self, Player player, bool isShortMeow = false)
        {
            if (player.SlugCatClass != Enums.vinki)
            {
                orig(self, player, isShortMeow);
                return;
            }

            if (VinkiConfig.UseGraffitiButton.Value && IsPressingGraffiti(player))
            {
                return;
            }

            orig(self, player, isShortMeow);

            player.Vinki().idleUpdates = 0;
        }
    }
}