using System;
using UnityEngine;
using RWCustom;
using System.Linq;
using ImprovedInput;
using SprayCans;
using static Vinki.Plugin;
using System.Threading.Tasks;
using DevInterface;
using SlugBase.SaveData;
using IL.MoreSlugcats;
using Smoke;
using System.Collections;

namespace Vinki
{
    public static partial class Hooks
    {
        public static async Task SprayGraffiti(Player self, int smokes = 10, int gNum = -1, float alphaPerSmoke = 0.3f)
        {
            string slugcat = graffitis.ContainsKey(self.slugcatStats.name.value) ? self.slugcatStats.name.value : SlugcatStats.Name.White.value;
            if (gNum < 0)
            {
                gNum = UnityEngine.Random.Range(0, graffitis[slugcat].Count);
            }
            else
            {
                slugcat = "Story";

                // Save that we sprayed this story graffiti
                SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
                if (miscSave.TryGet("StoryGraffitisSprayed", out bool[] sprd))
                {
                    storyGraffitisSprayed = sprd;
                }
                storyGraffitisSprayed[gNum] = true;
                miscSave.Set("StoryGraffitisSprayed", storyGraffitisSprayed);

                // If spraying the StoryGraffitiTutorial graffiti, move to the next phase
                if (gNum == 2)
                {
                    Debug.Log("Spraying tutorial story graffiti");
                    miscSave.Set("StoryGraffitiTutorialPhase", 1);
                }

                (self.room.drawableObjects.Find((x) => x is GraffitiHolder && (x as GraffitiHolder).gNum == gNum) as GraffitiHolder)?.RemoveFromRoom();
            }
            Debug.Log("Spraying " + slugcat + " #" + gNum + "\tsize: " + graffitis[slugcat][gNum].handles[1].ToString());

            Vector2 sprayPos = (slugcat == "Story" && storyGraffitiRoomPositions.ContainsKey(gNum)) ? storyGraffitiRoomPositions[gNum].Value : self.mainBodyChunk.pos;
            Room room = self.room;

            room.PlaySound(SoundID.Vulture_Jet_LOOP, self.mainBodyChunk, false, 1f, 2f);

            for (int j = 0; j < 4; j++)
            {
                graffitis[slugcat][gNum].vertices[j, 0] = alphaPerSmoke;
            }

            for (int i = 0; i < smokes; i++)
            {
                PlacedObject graffiti = new PlacedObject(PlacedObject.Type.CustomDecal, graffitis[slugcat][gNum])
                {
                    pos = sprayPos + graffitiOffsets[slugcat][gNum]
                };

                Vector2 smokePos = new Vector2(
                    sprayPos.x + UnityEngine.Random.Range(graffitiOffsets[slugcat][gNum].x, -graffitiOffsets[slugcat][gNum].x),
                    sprayPos.y + UnityEngine.Random.Range(graffitiOffsets[slugcat][gNum].y, -graffitiOffsets[slugcat][gNum].y));
                var smoke = new Explosion.ExplosionSmoke(smokePos, Vector2.zero, 2f);
                smoke.lifeTime = 15f;
                smoke.life = 2f;
                room.AddObject(smoke);
                smoke.colorA = graffitiAvgColors[slugcat][gNum];
                smoke.colorB = Color.gray;

                await Task.Delay(100);
                room.AddObject(new GraffitiObject(graffiti, room.game.GetStorySession != null ? room.game.GetStorySession.saveState : null, gNum, room.abstractRoom.name));
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
                    Plugin.sleeping = true;
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

            orig(self);

            if (!SuperJump.TryGet(self, out float power) || !CoyoteBoost.TryGet(self, out var coyoteBoost) ||
                self.SlugCatClass != Enums.vinki)
            {
                return;
            }

            // If player jumped or coyote jumped from a beam (or grinded to top of pole), then trick jump
            bool coyote = isCoyoteJumping(self);
            if (coyote || v.isGrindingH || v.grindUpPoleFlag || v.vineAtFeet != null)
            {
                // Separate from vine
                v.vineAtFeet = null;
                v.vineGrindDelay = 10;

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
                self.slideDirection = v.lastXDirection;

                // Get risk/reward speedboost when coyote jumping
                if (coyote)
                {
                    Debug.Log("Coyote jump!");
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

                v.grindUpPoleFlag = false;
            }
        }

        private static bool isCoyoteJumping(Player self)
        {
            VinkiPlayerData v = self.Vinki();
            //Debug.Log("Last animation: " + v.lastAnimation.ToString() +
            //    "\t This animation: " + self.animation.ToString() + "\t Body mode: " + self.bodyMode.ToString());
            return (v.lastAnimation == Player.AnimationIndex.StandOnBeam && self.animation == Player.AnimationIndex.None &&
                    self.bodyMode == Player.BodyModeIndex.Default);
        }

        // Implement higher beam speed
        private static void Player_Move(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!GrindXSpeed.TryGet(self, out var grindXSpeed) || !NormalXSpeed.TryGet(self, out var normalXSpeed) ||
                !GrindYSpeed.TryGet(self, out var grindYSpeed) || !NormalYSpeed.TryGet(self, out var normalYSpeed) ||
                !SparkColor.TryGet(self, out var sparkColor) || !GrindVineSpeed.TryGet(self, out var grindVineSpeed) ||
                self.SlugCatClass != Enums.vinki)
            {
                return;
            }
            VinkiPlayerData v = self.Vinki();

            // Save the last direction that Vinki was facing
            if (self.input[0].x != 0)
            {
                v.lastXDirection = self.input[0].x;
            }
            if (self.input[0].y != 0)
            {
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

            // If player isn't holding Grind, no need to do other stuff
            if (!self.IsPressed(Grind) && !v.grindToggle)
            {
                v.isGrindingH = v.isGrindingV = v.isGrindingNoGrav = v.isGrindingVine = false;
                v.vineAtFeet = null;
                self.slugcatStats.runspeedFac = normalXSpeed;
                self.slugcatStats.poleClimbSpeedFac = normalYSpeed;
                return;
            }

            v.isGrindingH = IsGrindingHorizontally(self);
            v.isGrindingV = IsGrindingVertically(self);
            v.isGrindingNoGrav = IsGrindingNoGrav(self);
            v.isGrindingVine = IsGrindingVineNoGrav(self);
            v.isGrinding = v.isGrindingH || v.isGrindingV || v.isGrindingNoGrav || v.isGrindingVine;

            ClimbableVinesSystem.VinePosition vineAtFeet = v.vineAtFeet;
            bool isGrindingAtopVine = vineAtFeet != null;
            bool goodVineState = (v.vineGrindDelay == 0 &&
                self.animation != Player.AnimationIndex.ClimbOnBeam && self.animation != Player.AnimationIndex.HangFromBeam &&
                self.animation != Player.AnimationIndex.StandOnBeam && self.animation != Player.AnimationIndex.ClimbOnBeam &&
                self.animation != Player.AnimationIndex.HangUnderVerticalBeam && self.animation != Player.AnimationIndex.VineGrab && 
                (self.bodyMode == Player.BodyModeIndex.Default || self.bodyMode == Player.BodyModeIndex.Stand) &&
                self.bodyChunks[1].vel.y < 0f
            );
            if (!isGrindingAtopVine && goodVineState)
            {
                vineAtFeet = self.room.climbableVines != null ? self.room.climbableVines.VineOverlap(self.bodyChunks[1].pos, self.bodyChunks[1].rad + 5f) : null;
                isGrindingAtopVine = (vineAtFeet != null);

                // First frame of grinding on vine
                if (isGrindingAtopVine)
                {
                    //Debug.Log("Animation before getting on vine: " + self.animation.ToString() + "\t prev animation: " + lastAnimation.ToString());
                    self.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, self.mainBodyChunk, false, 0.75f, 1f);
                    self.noGrabCounter = 15;
                    self.animation = Player.AnimationIndex.StandOnBeam;
                }
            }

            // Grind horizontally if holding Grind on a beam
            if (v.isGrindingH || (isGrindingAtopVine && goodVineState))
            {
                self.slugcatStats.runspeedFac = 0;
                if (isGrindingAtopVine)
                {
                    self.canJump = 5;
                    self.vineGrabDelay = 3;
                    self.room.climbableVines.VineBeingClimbedOn(vineAtFeet, self);
                    self.room.climbableVines.ConnectChunkToVine(self.bodyChunks[1], vineAtFeet, self.room.climbableVines.VineRad(vineAtFeet));

                    // Move feet
                    self.feetStuckPos = self.bodyChunks[1].pos;

                    Vector2 oldPos = self.room.climbableVines.OnVinePos(vineAtFeet);

                    // vines can "face" either direction, so we need to take that into account
                    Vector2 vineDir = self.room.climbableVines.VineDir(vineAtFeet);
                    float dot = vineDir.normalized.x * v.lastXDirection;
                    if (dot > 0f)
                    {
                        vineAtFeet.floatPos += grindVineSpeed / self.room.climbableVines.TotalLength(vineAtFeet.vine);
                    }
                    else
                    {
                        vineAtFeet.floatPos -= grindVineSpeed / self.room.climbableVines.TotalLength(vineAtFeet.vine);
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
                        Vector2 grindDir = (self.room.climbableVines.OnVinePos(vineAtFeet) - self.bodyChunks[1].pos).normalized;
                        self.room.climbableVines.PushAtVine(vineAtFeet, (oldPos - self.room.climbableVines.OnVinePos(vineAtFeet)) * 0.05f);
                    }
                }
                else
                {
                    self.bodyChunks[1].vel.x = grindXSpeed * v.lastXDirection;
                }
                
                // Sparks from grinding
                Vector2 pos = self.bodyChunks[1].pos;
                Vector2 posB = pos - new Vector2(10f * v.lastXDirection, 0);
                for (int j = 0; j < 2; j++)
                {
                    Vector2 a = RWCustom.Custom.RNV();
                    a.x = Mathf.Abs(a.x) * -v.lastXDirection;
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
            if (v.isGrindingV || v.isGrindingNoGrav || v.isGrindingVine)
            {
                //Debug.Log("Zero G Pole direction: " + self.zeroGPoleGrabDir.x + "," + self.zeroGPoleGrabDir.y);
                self.slugcatStats.poleClimbSpeedFac = 0;

                // Handle vine grinding
                if (v.isGrindingVine)
                {
                    self.vineClimbCursor = grindVineSpeed * Vector2.ClampMagnitude(
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
                }
                else
                {
                    // Handle 0G horizontal beam grinding
                    if (v.isGrindingNoGrav && self.room.GetTile(self.mainBodyChunk.pos).horizontalBeam)
                    {
                        self.bodyChunks[0].vel.x = grindYSpeed * v.lastXDirection;
                    }
                    else
                    {
                        // This works in gravity and no gravity
                        self.bodyChunks[0].vel.y = grindYSpeed * v.lastYDirection;
                    }
                }

                // Sparks from grinding
                Vector2 pos = new Vector2(self.room.MiddleOfTile(self.PlayerGraphics().legs.pos).x, self.PlayerGraphics().legs.pos.y);
                Vector2 posB = pos + new Vector2(0f, -3f);
                for (int j = 0; j < 2; j++)
                {
                    Vector2 a = RWCustom.Custom.RNV();
                    a.x = Mathf.Abs(a.x) * v.lastXDirection;
                    a.y = Mathf.Abs(a.y) * -v.lastYDirection;
                    Vector2 b = new Vector2(-a.x, a.y);
                    self.room.AddObject(new Spark(pos, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), sparkColor, null, 2, 4));
                    self.room.AddObject(new Spark(posB, b * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), sparkColor, null, 2, 4));
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
                ((v.lastYDirection > 0 && self.bodyChunks[1].vel.magnitude > 2f) ||
                (v.lastYDirection < 0 && self.bodyChunks[1].vel.magnitude > 1f)));
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
            orig(self, eu);

            // Spray a random graffiti, or tag a creature if there's one to tag
            if (self.JustPressed(Spray) && IsPressingGraffiti(self))
            {
                if (self.SlugCatClass == Enums.vinki && self.Vinki().tagableCreature != null)
                {
                    TagCreature(self);
                }
                else
                {
                    SprayGraffitiInGame(self);
                }
            }

            if (self.SlugCatClass != Enums.vinki)
            {
                return;
            }
            VinkiPlayerData v = self.Vinki();

            // Update grindToggle if needed
            if (self.JustPressed(ToggleGrind) && !IsPressingGraffiti(self))
            {
                v.grindToggle = !v.grindToggle;
            }

            // Craft SprayCan
            else if (self.IsPressed(Craft) && IsPressingGraffiti(self))
            {
                int sprayCount = CanCraftSprayCan(self.grasps[0], self.grasps[1]);
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
        }

        private static int CanCraftSprayCan(Creature.Grasp a, Creature.Grasp b)
        {
            // You can craft while moving if you're not grinding
            if (a == null || b == null || (a.grabber as Player).Vinki().isGrinding)
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
                if ((a.grabbed as SprayCan).Abstr.uses >= 5 || (b.grabbed as SprayCan).Abstr.uses >= 5)
                {
                    return 0;
                }
                return Math.Min(5, (a.grabbed as SprayCan).Abstr.uses + (b.grabbed as SprayCan).Abstr.uses);
            }
            if (abstractObjectType.ToString() == "SprayCan" && colorfulItems.ContainsKey(abstractObjectType2))
            {
                if ((a.grabbed as SprayCan).Abstr.uses >= 5)
                {
                    return 0;
                }
                return Math.Min(5, (a.grabbed as SprayCan).Abstr.uses + colorfulItems[abstractObjectType2]);
            }
            if (abstractObjectType2.ToString() == "SprayCan" && colorfulItems.ContainsKey(abstractObjectType))
            {
                if ((b.grabbed as SprayCan).Abstr.uses >= 5)
                {
                    return 0;
                }
                return Math.Min(5, (b.grabbed as SprayCan).Abstr.uses + colorfulItems[abstractObjectType]);
            }
            return 0;
        }

        private static bool IsPressingGraffiti(Player self)
        {
            return self.IsPressed(Graffiti) || (VinkiConfig.UpGraffiti.Value && self.input[0].y > 0f);
        }

        private static void SprayGraffitiInGame(Player self)
        {
            var storyGraffitisInRoom = Plugin.storyGraffitiRoomPositions.Where(e => e.Value.Key == self.room.abstractRoom.name);
            SlugBaseSaveData miscWorldSave;
            bool storyGraffitisExist = false;
            bool[] sprayedGNums = null;
            if (self.room.game.GetStorySession != null)
            {
                miscWorldSave = SaveDataExtension.GetSlugBaseData(self.room.game.GetStorySession.saveState.miscWorldSaveData);
                storyGraffitisExist = miscWorldSave.TryGet("StoryGraffitisSprayed", out sprayedGNums);
            }
            int gNum = -1;

            // Check if we are in the right place to spray a story graffiti
            foreach (var storyGraffiti in storyGraffitisInRoom)
            {
                var graf = Plugin.graffitis["Story"][storyGraffiti.Key];
                if (graf != null && (!storyGraffitisExist || !sprayedGNums[storyGraffiti.Key]))
                {
                    Vector2 grafRadius = graf.handles[1] / 2f;
                    Vector2 grafPos = storyGraffiti.Value.Value;
                    if (self.mainBodyChunk.pos.x >= grafPos.x - grafRadius.x && self.mainBodyChunk.pos.x <= grafPos.x + grafRadius.x &&
                        self.mainBodyChunk.pos.y >= grafPos.y - grafRadius.y && self.mainBodyChunk.pos.y <= grafPos.y + grafRadius.y)
                    {
                        gNum = storyGraffiti.Key; 
                        break;
                    }
                }
            }

            if (!VinkiConfig.RequireSprayCans.Value)
            {
                _ = SprayGraffiti(self, gNum: gNum);
            }
            else
            {
                var grasp = self.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan);
                if (grasp != null && (grasp.grabbed as SprayCan).TryUse())
                {
                    _ = SprayGraffiti(self, gNum: gNum);
                }
            }
        }

        private static float boxRadius = 40f;
        private static float boxOffset = 40f;
        private static void CheckForTagging(Player self, VinkiPlayerData v)
        {
            // Wait before being able to tag again
            if (v.tagLag > 0)
            {
                v.tagLag--;
                if (v.tagSmoke.room == self.room)
                {
                    v.tagSmoke.EmitSmoke(0.4f);
                }
                if (v.tagLag == 0)
                {
                    v.tagSmoke.target.graphicsModule.Tag().isBeingTagged = false;
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

                v.poisonedVictims[i].timeLeft--;
                (v.poisonedVictims[i].creature.State as HealthState).health -= v.poisonedVictims[i].damagePerTick;
            }

            if (!IsPressingGraffiti(self) || (VinkiConfig.RequireSprayCans.Value && self.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan) == null) ||
                self.room == null)
            {
                v.tagableCreature = null;
                return;
            }

            // Create box for where Vinki can tag creatures
            Vector2 boxCenter = new(self.mainBodyChunk.pos.x + (v.lastXDirection * boxOffset), self.mainBodyChunk.pos.y);
            float minX = boxCenter.x - boxRadius;
            float maxX = boxCenter.x + boxRadius;
            float minY = boxCenter.y - boxRadius;
            float maxY = boxCenter.y + boxRadius;

            // Find any creatures in the room within the box
            foreach (var creature in self.room.abstractRoom.creatures.Select((absCreature) => absCreature.realizedCreature))
            {
                if (!creature.canBeHitByWeapons || creature.dead || creature == self || (creature is Lizard && (creature as Lizard).AI.friendTracker.friend == self) ||
                    creature is Fly || (creature is Centipede && (creature as Centipede).Small) || creature is Hazer || creature is VultureGrub || creature is SmallNeedleWorm ||
                    (creature is Player && (creature as Player).abstractCreature.abstractAI != null))
                {
                    continue;
                }

                foreach (var chunk in creature.bodyChunks)
                {
                    // Can't spray face to encourage spraying from behind
                    if (!chunk.collideWithObjects || chunk == creature.mainBodyChunk)
                    {
                        continue;
                    }

                    Vector2 cPos = chunk.pos;
                    if (cPos.x > minX && cPos.x < maxX && cPos.y > minY && cPos.y < maxY)
                    {
                        v.tagableCreature = creature;
                        return;
                    }
                }
            }

            v.tagableCreature = null;
        }

        private static void TagCreature(Player self)
        {
            PhysicalObject source = self;
            if (VinkiConfig.RequireSprayCans.Value)
            {
                SprayCan can = self.grasps.FirstOrDefault(g => g?.grabbed is SprayCan).grabbed as SprayCan;
                if (!can.TryUse())
                {
                    return;
                }
                source = can;
            }

            VinkiPlayerData v = self.Vinki();

            if (v.tagLag > 0)
            {
                return;
            }
            v.tagLag = 30;

            float damage = 2f;
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

            self.room.PlaySound(SoundID.Hazer_Squirt_Smoke_LOOP, self.mainBodyChunk, false, 2f, 1f);
            if (damage > 0f)
            {
                if (v.tagableCreature.State is HealthState)
                {
                    v.tagableCreature.Violence(self.firstChunk, null, v.tagableCreature.firstChunk, null, Creature.DamageType.Stab, damage / 2, 0f);
                    v.poisonedVictims.Add(new VinkiPlayerData.PoisonedCreature(v.tagableCreature, 120, damage / 2));
                }
                else
                {
                    v.tagableCreature.Violence(self.firstChunk, null, v.tagableCreature.firstChunk, null, Creature.DamageType.Stab, damage, 0f);
                }
            }

            if(v.tagSmoke != null)
            {
                v.tagSmoke.RemoveFromRoom();
            }

            v.tagSmoke = new Smoke.TagSmoke(self.room, source, v.tagableCreature);
            self.room.AddObject(v.tagSmoke);
            v.tagSmoke.EmitSmoke(0.4f);
            v.tagSmoke.target.graphicsModule.Tag().isBeingTagged = true;
            v.tagSmoke.target.graphicsModule.Tag().tagColor = new HSLColor(v.tagSmoke.hue, 0.5f, 0.8f).rgb;
        }
    }
}