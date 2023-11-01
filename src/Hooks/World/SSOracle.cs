using System;
using System.Linq;
using static Vinki.Plugin;
using static Conversation;
using static SSOracleBehavior;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using System.Diagnostics.Eventing.Reader;
using SprayCans;
using Random = UnityEngine.Random;
using SlugBase.SaveData;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vinki
{
    public static partial class Hooks
    {
        private static SSOracleBehavior oracleBehavior;

        public static void TriggerSSOracleScene()
        {
            if (oracleBehavior.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0)
            {
                oracleBehavior.NewAction(Enums.SSOracle.Vinki_SSActionGeneral);
            }
        }

        // Add hooks
        private static void ApplySSOracleHooks()
        {
            On.SSOracleBehavior.SeePlayer += SSOracleBehavior_SeePlayer;
            On.SSOracleBehavior.NewAction += SSOracleBehavior_NewAction;
            On.SSOracleBehavior.Update += SSOracleBehavior_Update;

            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
        }

        private static void SSOracleBehavior_Update(On.SSOracleBehavior.orig_Update orig, SSOracleBehavior self, bool eu)
        {
            orig(self, eu);
            if (self.oracle.ID != MoreSlugcatsEnums.OracleID.DM || self.player == null || self.player.room != self.oracle.room || self.oracle.room.game.GetStorySession.saveStateNumber != Enums.vinki)
            {
                return;
            }

            List<PhysicalObject>[] physicalObjects = self.oracle.room.physicalObjects;
            for (int num6 = 0; num6 < physicalObjects.Length; num6++)
            {
                for (int num7 = 0; num7 < physicalObjects[num6].Count; num7++)
                {
                    PhysicalObject physicalObject = physicalObjects[num6][num7];
                    bool readingAction = self.action == Enums.DMOracle.Vinki_DMActionGeneral;
                    if (self.inspectPearl == null && self.conversation == null && physicalObject is DataPearl && (physicalObject as DataPearl).grabbedBy.Count == 0 && ((physicalObject as DataPearl).AbstractPearl.dataPearlType != DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl || (self.oracle.ID == MoreSlugcatsEnums.OracleID.DM && ((physicalObject as DataPearl).AbstractPearl as PebblesPearl.AbstractPebblesPearl).color >= 0)) && !self.readDataPearlOrbits.Contains((physicalObject as DataPearl).AbstractPearl) && readingAction && self.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark && !self.talkedAboutThisSession.Contains(physicalObject.abstractPhysicalObject.ID))
                    {
                        self.inspectPearl = (physicalObject as DataPearl);
                        if (RainWorld.ShowLogs)
                        {
                            string str = "---------- INSPECT PEARL TRIGGERED: ";
                            DataPearl.AbstractDataPearl.DataPearlType dataPearlType = self.inspectPearl.AbstractPearl.dataPearlType;
                            Debug.Log(str + ((dataPearlType != null) ? dataPearlType.ToString() : null));
                        }
                        break;
                    }
                }
            }
        }

        private static void SSOracleBehavior_SeePlayer(On.SSOracleBehavior.orig_SeePlayer orig, SSOracleBehavior self)
        {
            if (self.oracle.room.game.StoryCharacter == Enums.vinki && ModManager.MSC && self.oracle.ID == MoreSlugcatsEnums.OracleID.DM)
            {
                self.NewAction(Enums.DMOracle.Vinki_DMActionGeneral);
                return;
            }
            else if (self.oracle.room.game.StoryCharacter == Enums.vinki && self.action != Enums.SSOracle.Vinki_SSActionGeneral &&
                self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0)
            {
                oracleBehavior = self;

                if (self.timeSinceSeenPlayer < 0)
                    self.timeSinceSeenPlayer = 0;

                self.movementBehavior = MovementBehavior.Talk;

                self.SlugcatEnterRoomReaction();
                return;
            }

            orig(self);
        }

        private static void SSOracleBehavior_NewAction(On.SSOracleBehavior.orig_NewAction orig, SSOracleBehavior self, SSOracleBehavior.Action nextAction)
        {
            //Debug.Log(string.Concat(new string[]
            //{
            //    "Vinki new action: ",
            //    nextAction.ToString(),
            //    " (from ",
            //    self.action.ToString(),
            //    ")"
            //}));
            if (nextAction == Enums.DMOracle.Vinki_DMActionGeneral)
            {
                if (self.currSubBehavior.ID == Enums.DMOracle.Vinki_DMSlumberParty) return;

                self.inActionCounter = 0;
                self.action = nextAction;

                self.currSubBehavior.Deactivate();

                var subBehavior = new DMSleepoverBehavior(self);
                subBehavior.Activate(self.action, nextAction);
                self.currSubBehavior = subBehavior;
                return;
            }
            if (self.oracle.room.game.StoryCharacter == Enums.vinki && self.action != Enums.SSOracle.Vinki_SSActionGeneral && nextAction == Enums.SSOracle.Vinki_SSActionGeneral)
            {
                if (self.currSubBehavior.ID == Enums.SSOracle.Vinki_SSSubBehavGeneral) return;

                self.inActionCounter = 0;
                self.action = nextAction;

                var subBehavior = self.allSubBehaviors.FirstOrDefault(x => x.ID == Enums.SSOracle.Vinki_SSSubBehavGeneral);

                if (subBehavior == null)
                    self.allSubBehaviors.Add(subBehavior = new SSOracleMeetVinki(self));

                self.currSubBehavior.Deactivate();

                subBehavior.Activate(self.action, nextAction);
                self.currSubBehavior = subBehavior;
                return;
            }

            orig(self, nextAction);
        }

        private static void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, PebblesConversation self)
        {
            if (self.owner.oracle.room.game.StoryCharacter != Enums.vinki)
            {
                orig(self);
                return;
            }

            var l = 60;

            var id = self.id;
            var e = self.events;

            if (id == Enums.SSOracle.Vinki_SSConvoFirstMeet)
            {
                e.Add(new WaitEvent(self, 160));

                e.Add(new TextEvent(self, 0,
                    self.Translate(".  .  ."), 0));

                e.Add(new TextEvent(self, 0,
                    self.Translate("And what do we have here, on the once-pristine walls of my chamber?"), l));

                e.Add(new TextEvent(self, 0,
                    self.Translate("Is that ... scribble supposed to be me? Are those ... ears I'm wearing?"), l));

                e.Add(new WaitEvent(self, 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("I get the impression that you are proud of this, and I cannot understand why."), l));

                e.Add(new TextEvent(self, 0,
                    self.Translate("Is this really an accomplishment to celebrate? Terrorizing me with an atrocity like this?"), l));

                e.Add(new TextEvent(self, 0,
                    self.Translate("Someone must have put you up to this. Was it No Significant Harassment? His name has always been considerably ironic."), l));

                e.Add(new WaitEvent(self, 60));

                e.Add(new TextEvent(self, 0,
                    self.Translate(". . ."), 0));

                e.Add(new TextEvent(self, 0,
                    self.Translate("Your blank stare behind that ridiculous headpiece gives me no answers."), l));

                e.Add(new TextEvent(self, 0,
                    self.Translate("It is simply laughable that a little beast climbed all the way up my exterior just to insult me in this way. I hope you realize that I am NOT<LINE>offended in any way, shape, or form. After all, I am a being who is, if you excuse me, godlike in compar-"), 0));
            }
            else if (id == Enums.DMOracle.Vinki_DMConvoFirstMeet)
            {
                self.LoadEventsFromFile(8675309);
            }
            else
            {
                orig(self);
            }
        }

        public class SSOracleMeetVinki : ConversationBehavior
        {
            public SSOracleMeetVinki(SSOracleBehavior owner) : base(owner, Enums.SSOracle.Vinki_SSSubBehavGeneral, Enums.SSOracle.Vinki_SSConvoFirstMeet)
            {
                owner.getToWorking = 1f;
                owner.SlugcatEnterRoomReaction();
            }

            public override void Update()
            {
                base.Update();

                if (this.owner == null || oracle == null || player == null || player.room == null)
                {
                    return;
                }

                if (Plugin.sleeping)
                {
                    this.owner.NewAction(Enums.SSOracle.Vinki_SSActionTriggered);
                    Plugin.sleeping = false;
                }

                if (base.action == Enums.SSOracle.Vinki_SSActionGeneral)
                {
                    this.owner.LockShortcuts();
                    if (base.inActionCounter == 15 && (this.owner.conversation == null || this.owner.conversation.id != this.convoID))
                    {
                        base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad = 1;
                        this.owner.InitateConversation(this.convoID, this);
                    }
                }
                else if (base.action == Enums.SSOracle.Vinki_SSActionTriggered)
                {
                    if (base.inActionCounter == 15)
                    {
                        this.owner.conversation.paused = true;
                        this.owner.restartConversationAfterCurrentDialoge = false;

                        base.dialogBox.Interrupt(base.Translate(". . ."), 0);
                        base.dialogBox.NewMessage(base.Translate("You disrespectful little cretin."), 0);
                    }
                    if (base.inActionCounter > 175)
                    {
                        Debug.Log("Done with conversation.");
                        this.owner.conversation = null;
                        this.owner.NewAction(Enums.SSOracle.Vinki_SSActionGetOut);
                    }
                }
                else if (base.action == Enums.SSOracle.Vinki_SSActionGetOut)
                {
                    this.owner.UnlockShortcuts();
                    this.owner.getToWorking = 1f;
                    if (base.inActionCounter == 100 && base.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
                    {
                        base.dialogBox.Interrupt(base.Translate("Get out of my sight! Do not return until you have accomplished something worthwhile."), 60);
                        this.owner.voice = base.oracle.room.PlaySound(SoundID.SS_AI_Talk_5, base.oracle.firstChunk);
                        this.owner.voice.requireActiveUpkeep = true;
                    }
                    if (base.inActionCounter == 220)
                    {
                        var grasp = player.grasps?.FirstOrDefault(g => g?.grabbed is SprayCan);
                        if (grasp != null && (grasp.grabbed as SprayCan).TryUse())
                        {
                            _ = Hooks.SprayGraffiti(player, 4, 1, 1f);
                        }
                    }
                    if (base.inActionCounter == 500)
                    {
                        this.owner.voice = base.oracle.room.PlaySound(SoundID.SS_AI_Talk_3, base.oracle.firstChunk);
                        this.owner.voice.requireActiveUpkeep = true;
                    }
                    if (base.inActionCounter > 550)
                    {
                        this.owner.NewAction(SSOracleBehavior.Action.ThrowOut_KillOnSight);
                    }
                    if (base.inActionCounter > 200)
                    {
                        if (base.player.room == base.oracle.room)
                        {
                            if (!base.oracle.room.aimap.getAItile(base.player.mainBodyChunk.pos).narrowSpace)
                            {
                                base.player.mainBodyChunk.vel += Custom.DirVec(base.player.mainBodyChunk.pos, base.oracle.room.MiddleOfTile(28, 32)) * 2f * (1f - base.oracle.room.gravity) * Mathf.InverseLerp(20f, 150f, (float)base.inActionCounter);
                            }
                            if (base.oracle.room.GetTilePosition(base.player.mainBodyChunk.pos) == new IntVector2(28, 32) && base.player.enteringShortCut == null)
                            {
                                base.player.enteringShortCut = new IntVector2?(base.oracle.room.ShortcutLeadingToNode(1).StartTile);
                                return;
                            }
                            return;
                        }
                        this.owner.NewAction(SSOracleBehavior.Action.ThrowOut_KillOnSight);
                    }
                    return;
                }
            }
        }

        public class DMSleepoverBehavior : ConversationBehavior
        {
            public DMSleepoverBehavior(SSOracleBehavior owner) : base(owner, Enums.DMOracle.Vinki_DMSlumberParty, Enums.DMOracle.Vinki_DMConvoFirstMeet)
            {
                this.lowGravity = -1f;
                if (this.owner.conversation != null)
                {
                    this.owner.conversation.Destroy();
                    this.owner.conversation = null;
                    return;
                }
                this.owner.TurnOffSSMusic(true);
                owner.getToWorking = 1f;

                // If this is a return visit to Moon
                met = (base.oracle.room.game.rainWorld.ExpeditionMode || base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.smPearlTagged);
                if (met)
                {
                    if (Random.value < 0.3f)
                    {
                        base.dialogBox.NewMessage(base.Translate("It is nice to see you again little frined!"), 0);
                        return;
                    }
                    if (Random.value < 0.5f)
                    {
                        base.dialogBox.NewMessage(base.Translate("Thank you for visiting me, but I'm afraid there is nothing here for you."), 0);
                        return;
                    }
                    base.dialogBox.NewMessage(base.Translate("Welcome back little friend."), 0);
                }
            }

            public override void Activate(SSOracleBehavior.Action oldAction, SSOracleBehavior.Action newAction)
            {
                base.Activate(oldAction, newAction);
            }

            public override void NewAction(SSOracleBehavior.Action oldAction, SSOracleBehavior.Action newAction)
            {
                base.NewAction(oldAction, newAction);
                if (newAction == SSOracleBehavior.Action.ThrowOut_KillOnSight && this.owner.conversation != null)
                {
                    this.owner.conversation.Destroy();
                    this.owner.conversation = null;
                }
            }

            public override void Update()
            {
                base.Update();
                if (base.player == null)
                {
                    return;
                }
                if (owner.oracle.room.game.StoryCharacter == Enums.vinki && owner.action == Enums.DMOracle.Vinki_DMActionGeneral && !met)
                {
                    owner.LockShortcuts();
                    owner.movementBehavior = SSOracleBehavior.MovementBehavior.KeepDistance;
                    //owner.gravOn = true;
                    if (owner.inActionCounter == 80 && (owner.conversation == null || owner.conversation.id != Enums.DMOracle.Vinki_DMConvoFirstMeet))
                    {
                        owner.InitateConversation(Enums.DMOracle.Vinki_DMConvoFirstMeet, this);
                    }
                    if (owner.inActionCounter > 80 && (owner.conversation == null || (owner.conversation != null && owner.conversation.id == Enums.DMOracle.Vinki_DMConvoFirstMeet && owner.conversation.slatedForDeletion)))
                    {
                        owner.UnlockShortcuts();
                        owner.conversation = null;
                        owner.getToWorking = 1f;
                        met = base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.smPearlTagged = true;
                    }
                    return;
                }
                if (this.tagTimer > 0f && this.owner.inspectPearl != null)
                {
                    this.owner.killFac = Mathf.Clamp(this.tagTimer / 120f, 0f, 1f);
                    this.tagTimer -= 1f;
                    if (this.tagTimer <= 0f)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            base.oracle.room.AddObject(new Spark(this.owner.inspectPearl.firstChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                        }
                        base.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, this.owner.inspectPearl.firstChunk.pos, 1f, 0.5f + Random.value * 0.5f);
                        this.owner.killFac = 0f;
                    }
                }
                if (this.holdPlayer && base.player.room == base.oracle.room)
                {
                    base.player.mainBodyChunk.vel *= Custom.LerpMap((float)base.inActionCounter, 0f, 30f, 1f, 0.95f);
                    base.player.bodyChunks[1].vel *= Custom.LerpMap((float)base.inActionCounter, 0f, 30f, 1f, 0.95f);
                    base.player.mainBodyChunk.vel += Custom.DirVec(base.player.mainBodyChunk.pos, this.holdPlayerPos) * Mathf.Lerp(0.5f, Custom.LerpMap(Vector2.Distance(base.player.mainBodyChunk.pos, this.holdPlayerPos), 30f, 150f, 2.5f, 7f), base.oracle.room.gravity) * Mathf.InverseLerp(0f, 10f, (float)base.inActionCounter) * Mathf.InverseLerp(0f, 30f, Vector2.Distance(base.player.mainBodyChunk.pos, this.holdPlayerPos));
                }
                else
                {
                    this.owner.getToWorking = 1f;
                    if (this.lowGravity < 0f)
                    {
                        this.lowGravity = 0f;
                    }
                    this.owner.SetNewDestination(base.oracle.firstChunk.pos);
                }
            }

            private Vector2 holdPlayerPos
            {
                get
                {
                    return new Vector2(668f, 268f + Mathf.Sin((float)base.inActionCounter / 70f * 3.1415927f * 2f) * 4f);
                }
            }

            public override bool Gravity
            {
                get
                {
                    return this.gravOn;
                }
            }

            public override float LowGravity
            {
                get
                {
                    return this.lowGravity;
                }
            }

            public bool holdPlayer;

            public bool gravOn;

            public bool met;

            public float lowGravity;

            public float lastGetToWork;

            public float tagTimer;
        }
    }
}