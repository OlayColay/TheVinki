using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "The Vinki", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "olaycolay.thevinki";
        private int lastXDirection = 1;
        private int lastYDirection = 1;
        private bool grindUpPoleFlag = false;
        private Player.AnimationIndex lastAnimationFrame = Player.AnimationIndex.None;
        private Player.AnimationIndex lastAnimation = Player.AnimationIndex.None;

        public static readonly PlayerFeature<float> GrindXSpeed = PlayerFloat("thevinki/grind_x_speed");
        public static readonly PlayerFeature<float> GrindYSpeed = PlayerFloat("thevinki/grind_y_speed");
        public static readonly PlayerFeature<float> NormalXSpeed = PlayerFloat("thevinki/normal_x_speed");
        public static readonly PlayerFeature<float> NormalYSpeed = PlayerFloat("thevinki/normal_y_speed");
        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("thevinki/super_jump");
        public static readonly PlayerFeature<Color> SparkColor = PlayerColor("thevinki/spark_color");


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Put your custom hooks here!
            On.Player.Jump += Player_Jump;
            On.Player.MovementUpdate += Player_Move;
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }


        // Implement SuperJump
        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);

            if (!SuperJump.TryGet(self, out float power))
            {
                return;
            }

            // If player jumped or coyote jumped from a beam (or grinded to top of pole), then trick jump
            if (((lastAnimation == Player.AnimationIndex.StandOnBeam || lastAnimationFrame == Player.AnimationIndex.StandOnBeam) && 
                self.input[0].pckp) || grindUpPoleFlag)
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
                BodyChunk bodyChunk19 = self.bodyChunks[0];
                self.slideDirection = lastXDirection;
                bodyChunk19.vel.x = bodyChunk19.vel.x - (float)self.slideDirection * 4f * num;
                self.jumpBoost *= power;
                self.animation = Player.AnimationIndex.Flip;
                self.room.PlaySound(SoundID.Slugcat_Flip_Jump, self.mainBodyChunk, false, 1f, 1f);
                self.slideCounter = 0;

                grindUpPoleFlag = false;
            }
        }

        // Implement higher beam speed
        private void Player_Move(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!GrindXSpeed.TryGet(self, out var grindXSpeed) || !NormalXSpeed.TryGet(self, out var normalXSpeed) ||
                !GrindYSpeed.TryGet(self, out var grindYSpeed) || !NormalYSpeed.TryGet(self, out var normalYSpeed) ||
                !SparkColor.TryGet(self, out var sparkColor))
            {
                return;
            }

            // Grind horizontally if holding pckp on a beam
            if (self.animation == Player.AnimationIndex.StandOnBeam && self.input[0].pckp)
            {
                self.slugcatStats.runspeedFac = 0;
                self.bodyChunks[0].vel.x = grindXSpeed * lastXDirection;
                self.bodyChunks[1].vel.x = grindXSpeed * lastXDirection;

                // Sparks from grinding
                Vector2 pos = self.bodyChunks[1].pos;
                Vector2 posB = pos - new Vector2(10f * lastXDirection, 0);
                for (int j = 0; j < 1; j++)
                {
                    Vector2 a = RWCustom.Custom.RNV();
                    a.x = Mathf.Abs(a.x) * -lastXDirection;
                    a.y = Mathf.Abs(a.y);
                    self.room.AddObject(new Spark(pos, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), sparkColor, null, 2, 4));
                    self.room.AddObject(new Spark(posB, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), sparkColor, null, 2, 4));
                }
            }
            else
            {
                self.slugcatStats.runspeedFac = normalXSpeed;
            }

            // Grind vertically if holding pckp on a pole (vertical beam)
            if (self.animation == Player.AnimationIndex.ClimbOnBeam && self.input[0].pckp)
            {
                self.slugcatStats.poleClimbSpeedFac = 0;
                self.bodyChunks[0].vel.y = grindYSpeed * lastYDirection;
                self.bodyChunks[1].vel.y = grindYSpeed * lastYDirection;
            }
            else
            {
                self.slugcatStats.poleClimbSpeedFac = normalYSpeed;
            }

            // Catch beam with feet if holding pckp, and not holding down
            if ((self.animation == Player.AnimationIndex.None || self.animation == Player.AnimationIndex.Flip) && 
                self.input[0].pckp && self.input[0].y >= 0 && self.room.GetTile(self.bodyChunks[1].pos).horizontalBeam &&
                self.bodyChunks[0].vel.y < 0f)
            {
                self.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, self.mainBodyChunk, false, 0.75f, 1f);
                self.noGrabCounter = 15;
                self.animation = Player.AnimationIndex.StandOnBeam;
                self.bodyChunks[1].pos.y = self.room.MiddleOfTile(self.bodyChunks[1].pos).y + 5f;
                self.bodyChunks[1].vel.y = 0f;
                self.bodyChunks[0].vel.y = 0f;
            }

            // Stop flipping when holding pckp, falling fast, and letting go of jmp (so landing on a rail doesn't look weird)
            if (self.animation == Player.AnimationIndex.Flip && self.input[0].pckp)
            {
                if (self.bodyChunks[0].vel.y < -3f && !self.input[0].jmp)
                {
                    self.animation = Player.AnimationIndex.HangFromBeam;
                }
            }

            // If grinding up a pole and reach the top, jump up high
            if (self.animation == Player.AnimationIndex.GetUpToBeamTip && lastAnimationFrame == Player.AnimationIndex.ClimbOnBeam &&
                self.input[0].pckp)
            {
                if (self.input[0].jmp)
                {
                    grindUpPoleFlag = true;
                    self.Jump();
                }
                else
                {
                    self.bodyChunks[1].pos = self.room.MiddleOfTile(self.bodyChunks[1].pos) + new Vector2(0f, 5f);
                    self.bodyChunks[1].vel *= 0f;
                    self.bodyChunks[0].vel = Vector2.ClampMagnitude(self.bodyChunks[0].vel, 9f);
                }
                
            }
            else
            {
                grindUpPoleFlag = false;
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
            // Save the last animation
            if (self.animation != lastAnimationFrame)
            {
                lastAnimation = lastAnimationFrame;
                lastAnimationFrame = self.animation;
            }
        }
    }
}