using System;
using System.Linq;
using static Vinki.Plugin;
using static Conversation;
using static SSOracleBehavior;
using UnityEngine;
using MoreSlugcats;
using RWCustom;

namespace Vinki
{
    public static partial class Hooks
    {
        private static SSOracleBehavior oracleBehavior;

        private static void TriggerSSOracleScene()
        {
            oracleBehavior.NewAction(Enums.SSOracle.Vinki_SSActionGeneral);
        }

        // Add hooks
        private static void ApplySSOracleHooks()
        {
            On.SSOracleBehavior.SeePlayer += SSOracleBehavior_SeePlayer;
            On.SSOracleBehavior.NewAction += SSOracleBehavior_NewAction;

            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
        }

        private static void SSOracleBehavior_SeePlayer(On.SSOracleBehavior.orig_SeePlayer orig, SSOracleBehavior self)
        {
            if (self.oracle.room.game.StoryCharacter == Enums.TheVinki && self.action != Enums.SSOracle.Vinki_SSActionGeneral)
            {
                oracleBehavior = self;

                if (self.timeSinceSeenPlayer < 0)
                    self.timeSinceSeenPlayer = 0;

                self.SlugcatEnterRoomReaction();
                return;
            }

            orig(self);
        }

        private static void SSOracleBehavior_NewAction(On.SSOracleBehavior.orig_NewAction orig, SSOracleBehavior self, SSOracleBehavior.Action nextAction)
        {
            Debug.Log(string.Concat(new string[]
            {
                "Vinki new action: ",
                nextAction.ToString(),
                " (from ",
                self.action.ToString(),
                ")"
            }));
            if (self.oracle.room.game.StoryCharacter == Enums.TheVinki && self.action != Enums.SSOracle.Vinki_SSActionGeneral && nextAction == Enums.SSOracle.Vinki_SSActionGeneral)
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
            if (self.owner.oracle.room.game.StoryCharacter != Enums.TheVinki)
            {
                orig(self);
                return;
            }

            var currentLang = self.owner.rainWorld.inGameTranslator.currentLanguage;
            var l = currentLang == InGameTranslator.LanguageID.Chinese || currentLang == InGameTranslator.LanguageID.Japanese || currentLang == InGameTranslator.LanguageID.Korean ? 8 : 0;

            var id = self.id;
            var e = self.events;

            if (id == Enums.SSOracle.Vinki_SSConvoFirstMeet)
            {
                e.Add(new WaitEvent(self, 160));

                e.Add(new TextEvent(self, 0,
                    self.Translate(".  .  ."), 0));

                e.Add(new TextEvent(self, 0,
                    self.Translate("What in the world is this?"), l * 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("Is that ... scribble supposed to be me?"), l * 80));

                e.Add(new WaitEvent(self, 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("You must think that you are some sort of funny prankster."), l * 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("Well, you are not funny. Do you see me laughing? You are just a stupid beast."), l * 80));

                e.Add(new WaitEvent(self, 120));
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
                owner.getToWorking = 0f;
                owner.SlugcatEnterRoomReaction();
            }

            public override void Update()
            {
                base.Update();
                this.owner.LockShortcuts();
                if (base.action == Enums.SSOracle.Vinki_SSActionGeneral)
                {
                    base.movementBehavior = MovementBehavior.Talk;
                    if (base.inActionCounter == 15 && (this.owner.conversation == null || this.owner.conversation.id != this.convoID))
                    {
                        this.owner.InitateConversation(this.convoID, this);
                    }
                    if (this.owner.conversation != null && this.owner.conversation.id == this.convoID && this.owner.conversation.slatedForDeletion)
                    {
                        Debug.Log("Done with conversation.");
                        this.owner.conversation = null;
                        this.owner.NewAction(Enums.SSOracle.Vinki_SSActionGetOut);
                    }
                }
                else if (base.action == Enums.SSOracle.Vinki_SSActionGetOut)
                {
                    base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad = 1;
                    this.owner.UnlockShortcuts();
                    if (base.inActionCounter == 100 && base.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
                    {
                        base.dialogBox.Interrupt(base.Translate("Get out of my sight."), 60);
                    }
                    this.owner.getToWorking = 1f;
                    if (base.inActionCounter == 80)
                    {
                        this.owner.voice = base.oracle.room.PlaySound(SoundID.SS_AI_Talk_5, base.oracle.firstChunk);
                        this.owner.voice.requireActiveUpkeep = true;
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
                    if (base.inActionCounter > 100)
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
    }
}