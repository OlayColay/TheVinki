﻿using System.Linq;
using static Conversation;
using static SSOracleBehavior;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using SprayCans;
using Random = UnityEngine.Random;
using SlugBase.SaveData;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

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

        public static void SprayNearIterator(bool isMoon, SlugBaseSaveData miscSave, string imageName)
        {
            if (oracleBehavior == null)
            {
                return;
            }

            if (isMoon)
            {
                //VLogger.LogInfo("Sprayed near Moon: " + imageName);   
                if (oracleBehavior.conversation != null)
                {
                    oracleBehavior.conversation.paused = true;
                    oracleBehavior.restartConversationAfterCurrentDialoge = true;
                    if (!miscSave.TryGet("MoonInterruptTimes", out int moonInterruptTimes) || moonInterruptTimes == 0)
                    {
                        oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("Were you even listening to me? It appears to me that you find painting in my chamber more interesting than my words, I suppose. I would appreciate it if you did not interrupt me in the future, little friend."), 0);
                    }
                    else if (moonInterruptTimes == 1)
                    {
                        oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("Little creature... I asked you to please not interrupt me. I know you love art, but could you settle down and listen for a moment?"), 0);
                    }
                    else
                    {
                        float rand = Random.value;
                        if (rand < 0.3f)
                        {
                            oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("And now you are painting again..."), -10);
                        }
                        else if (rand < 0.7f)
                        {
                            oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("Am I boring you, little creature?"), -10);
                        }
                        else
                        {
                            oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("You are not the most attentive creature, are you?"), -10);
                        }
                    }
                    miscSave.Set("MoonInterruptTimes", moonInterruptTimes + 1);
                }
                else
                {
                    if (!miscSave.TryGet("MoonSprayTimes", out int moonSprayTimes) || moonSprayTimes == 0)
                    {
                        oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("Ah... I would appreciate you not painting over my chamber walls. Feel free to do so outside of it, but I require clear walls to project holographic graphs over. Your art, as beautiful as it is, might distort them."), 0);
                    }
                    else if (moonSprayTimes == 1)
                    {
                        oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("I already told you to not paint over my chamber walls. I'm sure you can find plenty of significantly better canvas outside of my chamber, and I don't want to have to destroy your hard work, so please do so outside."), 0);
                    }
                    else
                    {
                        float rand = Random.value;
                        if (rand < 0.3f)
                        {
                            oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("Are you ignoring my request on purpose? Please paint outside."), -10);
                        }
                        else if (rand < 0.7f)
                        {
                            oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("Little friend, I truly need my walls clean for my systems to work correctly. Please stop."), -10);
                        }
                        else
                        {
                            oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("You are wasting your materials. Your paintings cannot stay here. Please listen."), -10);
                        }
                    }
                    miscSave.Set("MoonSprayTimes", moonSprayTimes + 1);
                }

                GraffitiSpecificDialogue(imageName, miscSave);

                if (oracleBehavior.conversation != null)
                {
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("As for what I was saying before you rudely interrupted me..."), -10);
                }
            }
        }

        public static void CraftNearMoon(Player player)
        {
            Creature.Grasp pearlGrasp;
            if ((pearlGrasp = player.grasps.FirstOrDefault((Creature.Grasp grasp) => grasp?.grabbed is DataPearl)) == null)
            {
                return;
            }

            DataPearl pearl = pearlGrasp.grabbed as DataPearl;
            SlugBaseSaveData miscWorldSave = SaveDataExtension.GetSlugBaseData(player.room.game.GetStorySession.saveState.miscWorldSaveData);

            if (pearl.AbstractPearl.dataPearlType.value == "Vinki_Pearl_1")
            {
                oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("...Ah. I see you did not appreciate my gift for you."), -10);
                oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I will accept your choice, though. It is not my place to judge, as disrespectful as it felt to my efforts."), 0);
                miscWorldSave.Set("VinkiPearlCrushed", true);
            }
            else if (player.room.physicalObjects[0].Where((PhysicalObject obj) => obj is PebblesPearl).Count() <= 1)
            {
                oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("...What have you done?!"), -10);
                oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("So much data lost... Please leave."), -10);
                oracleBehavior.conversation = null;
                return;
            }
            else if (pearl is PebblesPearl)
            {
                if (!miscWorldSave.TryGet("MoonPearlCrushCount", out int crushedPearlCount) || crushedPearlCount < 1)
                {
                    oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("...Little creature? Could you not use my data pearls to create your tools? I might not miss one or two, but I would like to save the information which is held within them."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Maybe you could find some empty pearls outside of my chamber, or use something else?"), -10);
                }
                else if (crushedPearlCount == 1)
                {
                    oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("Little creature. I asked you once, please do not use my pearls as painting supplies. Losing data is very unpleasant, please find other materials."), 0);
                }
                else
                {
                    oracleBehavior.dialogBox.Interrupt(oracleBehavior.Translate("Please stop destroying my data pearls. Find other materials."), -10);
                }
                miscWorldSave.Set("MoonPearlCrushCount", crushedPearlCount + 1);
            }

            if (oracleBehavior.conversation != null)
            {
                oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("As for what I was saying before you rudely interrupted me..."), -10);
            }
        }

        // Add hooks
        private static void ApplySSOracleHooks()
        {
            On.SSOracleBehavior.SeePlayer += SSOracleBehavior_SeePlayer;
            On.SSOracleBehavior.NewAction += SSOracleBehavior_NewAction;
            On.SSOracleBehavior.Update += SSOracleBehavior_Update;
            On.SSOracleBehavior.UpdateStoryPearlCollection += SSOracleBehavior_UpdateStoryPearlCollection;

            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
        }
        private static void RemoveSSOracleHooks()
        {
            On.SSOracleBehavior.SeePlayer -= SSOracleBehavior_SeePlayer;
            On.SSOracleBehavior.NewAction -= SSOracleBehavior_NewAction;
            On.SSOracleBehavior.Update -= SSOracleBehavior_Update;
            On.SSOracleBehavior.UpdateStoryPearlCollection -= SSOracleBehavior_UpdateStoryPearlCollection;

            On.SSOracleBehavior.PebblesConversation.AddEvents -= PebblesConversation_AddEvents;
        }

        private static void SSOracleBehavior_UpdateStoryPearlCollection(On.SSOracleBehavior.orig_UpdateStoryPearlCollection orig, SSOracleBehavior self)
        {
            DataPearl.AbstractDataPearl vPearl;
            if ((vPearl = self.readDataPearlOrbits.Find((pearl) => pearl.dataPearlType.value == "Vinki_Pearl_1")) != null)
            {
                if (!self.readPearlGlyphs.ContainsKey(vPearl))
                {
                    self.readPearlGlyphs.Add(vPearl, new GlyphLabel(vPearl.realizedObject.firstChunk.pos, GlyphLabel.RandomString(1, 1, 12842 + vPearl.dataPearlType.Index, false)));
                    self.oracle.room.AddObject(self.readPearlGlyphs[vPearl]);
                }
            }
            orig(self);
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
                            Plugin.VLogger.LogInfo(str + (dataPearlType?.ToString()));
                        }
                        if (self.inspectPearl.AbstractPearl.dataPearlType.value == "Vinki_Pearl_1")
                        {
                            _ = ShowImage(self);
                        }
                        break;
                    }
                }
            }
        }

        private static async Task ShowImage(SSOracleBehavior self)
        {
            await Task.Delay(12000);

            if (SaveDataExtension.GetSlugBaseData(self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData).TryGet("VinkiPearlCrushed", out bool crushed) && crushed)
            {
                return;
            }

            self.oracle.room.PlaySound(SoundID.SS_AI_Image, 0f, 1f, 1f);
            ProjectedImage image = self.oracle.myScreen.AddImage("True Victory");
            image.pos = new Vector2(490, 350);
            image.setAlpha = 0.9f;
            await Task.Delay(11000);

            self.oracle.myScreen.RemoveImage("True Victory");
        }

        private static void SSOracleBehavior_SeePlayer(On.SSOracleBehavior.orig_SeePlayer orig, SSOracleBehavior self)
        {
            oracleBehavior = self;
            if (self.oracle.room.game.StoryCharacter == Enums.vinki && ModManager.MSC && self.oracle.ID == MoreSlugcatsEnums.OracleID.DM)
            {
                self.NewAction(Enums.DMOracle.Vinki_DMActionGeneral);
                return;
            }
            else if (self.oracle.room.game.StoryCharacter == Enums.vinki && self.action != Enums.SSOracle.Vinki_SSActionGeneral &&
                self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0)
            {
                if (self.timeSinceSeenPlayer < 0)
                    self.timeSinceSeenPlayer = 0;

                self.movementBehavior = MovementBehavior.Talk;

                self.SlugcatEnterRoomReaction();
                return;
            }

            orig(self);
        }

        private static void SSOracleBehavior_NewAction(On.SSOracleBehavior.orig_NewAction orig, SSOracleBehavior self, Action nextAction)
        {
            //VLogger.LogInfo(string.Concat(new string[]
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
            else if (id == Enums.DMOracle.Vinki_DMConvoSecondMeet)
            {
                self.LoadEventsFromFile(420, Enums.vinki, false, 0);
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

                if (owner == null || oracle == null || player == null || player.room == null)
                {
                    return;
                }

                if (Plugin.sleeping)
                {
                    owner.NewAction(Enums.SSOracle.Vinki_SSActionTriggered);
                    Plugin.sleeping = false;
                }

                if (base.action == Enums.SSOracle.Vinki_SSActionGeneral)
                {
                    owner.LockShortcuts();
                    if (base.inActionCounter == 15 && (owner.conversation == null || owner.conversation.id != convoID))
                    {
                        base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad = 1;
                        owner.InitateConversation(convoID, this);
                    }
                }
                else if (base.action == Enums.SSOracle.Vinki_SSActionTriggered)
                {
                    if (base.inActionCounter == 15)
                    {
                        owner.conversation.paused = true;
                        owner.restartConversationAfterCurrentDialoge = false;

                        base.dialogBox.Interrupt(base.Translate(". . ."), 0);
                        base.dialogBox.NewMessage(base.Translate("You disrespectful little cretin."), 0);
                    }
                    if (base.inActionCounter > 175)
                    {
                        Plugin.VLogger.LogInfo("Done with conversation.");
                        owner.conversation = null;
                        owner.NewAction(Enums.SSOracle.Vinki_SSActionGetOut);
                    }
                }
                else if (base.action == Enums.SSOracle.Vinki_SSActionGetOut)
                {
                    owner.UnlockShortcuts();
                    owner.getToWorking = 1f;
                    if (base.inActionCounter == 100 && base.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
                    {
                        base.dialogBox.Interrupt(base.Translate("Get out of my sight! Do not return until you have accomplished something worthwhile."), 60);
                        owner.voice = base.oracle.room.PlaySound(SoundID.SS_AI_Talk_5, base.oracle.firstChunk);
                        owner.voice.requireActiveUpkeep = true;
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
                        owner.voice = base.oracle.room.PlaySound(SoundID.SS_AI_Talk_3, base.oracle.firstChunk);
                        owner.voice.requireActiveUpkeep = true;
                    }
                    if (base.inActionCounter > 550)
                    {
                        owner.NewAction(SSOracleBehavior.Action.ThrowOut_KillOnSight);
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
                        owner.NewAction(SSOracleBehavior.Action.ThrowOut_KillOnSight);
                    }
                    return;
                }
            }
        }

        private static void GraffitiSpecificDialogue(string imageName, SlugBaseSaveData miscSave)
        {
            // Graffiti specific dialogue
            switch (imageName)
            {
                case "StorySpoilers/True Victory":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...Oh. That is an incredibly detailed piece of artwork, I see."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("As much as I am still quite unhappy about you painting on my chamber walls... I can’t help but admire your skill and determination, little friend."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("You likely spent a lot of time learning how to draw all those creatures, and your perseverance alone is incredibly admirable!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Especially considering that you had to spend enough time near all of those dangerous predators to learn how they look... Truly admirable."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("You must’ve traveled and seen quite a lot in your life, didn’t you?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I feel so incredibly regretful about having to erase it, even if I need my walls clean..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...Would you like me to immortalize it for you? I could take a picture and print it inside a pearl for you. You might not be able to read it, but if you give it to me, I could project the mural a second time through holograms."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("That seems like a good idea, doesn’t it?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Come back next cycle, and I will have it done by then."), 0);
                    miscSave.Set("SpawnUnlockablePearl", 1);
                    break;
                case "VinkiGraffiti/vinki/Beep - 5P or QT":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("That is... surely an interesting piece of art. Although I would highly prefer not to see it. As Five Pebbles' senior and close neighbor, it feels disrespectful to have such images of him on my chamber walls."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I apologize, but I have to remove it."), 0);
                    break;
                case "VinkiGraffiti/vinki/hoko - overseercut":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Ah... I see you do not enjoy the presence of our overseers? Despite your distaste for them, I would appreciate it if you did not advocate for violence against beings that are a part of me."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("They are our eyes, after all, and scavengers already hunt them plenty without your guidance."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Do not encourage them further."), 0);
                    break;
                case "VinkiGraffiti/vinki/JayDee - SRSside Up":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Little creature, how did you manage to replicate the exact likeness of one of our neighboring iterators so well? Have you traveled from afar, or is it a lucky coincidence?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("This particular piece is... Surely interesting. I am unsure of the meaning of it, but I can freely admit I do not enjoy the implications."), 0);
                    break;
                case "VinkiGraffiti/vinki/MagicaJaphet - Explosive Trick":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a picture of you. Are you trying to ensure that I remember you forever by putting portraits of yourself over my walls?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I do have to admit that your ability to capture bodies in motion is incredible. However... I would rather keep my walls clean, please."), 0);
                    break;
                case "VinkiGraffiti/vinki/RW - smileycreature":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("The creature that you drew is oddly shaped. Is it based on anything in particular?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Salami_Hunter - Jeffry Squeeze":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...I do not enjoy the look of this piece of art. The implications might be not intended, but I will be removing it from my walls despite that."), 0);
                    break;
                case "VinkiGraffiti/vinki/Salami_Hunter - Ouroboros":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I find it impressive how well you adapt to various painting styles. I think Pebbles would like this one. It reminds me of an ancient piece of art which symbolizes unity of all existing things within the cycle."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Did you know about it when you painted it?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Jaws":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I see that you had the unfortunate pleasure of meeting a Miros Bird? I do not advise you to continue visiting the area where they live."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("The memory crypts were considered sacred ground by my creators, which is the reason behind the existence of those beings. They are incredibly vicious, aggressively protecting the wealth of those laid to rest underneath."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Due to the nature of this art piece... I suppose you already know of their weakness to harsh light? I'm glad it helped you get past them safely."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I suppose living in near-constant darkness does make even purposed organisms have weaknesses."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Garbage Alarm":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("That is... Such an interesting depiction of one of many purposed organisms used for recycling waste materials. Is that a spear, stuck to the back of their head?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...Is this picture based on first-hand experience?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Those beings can hold grudges for quite a while. I don't envy you, little friend."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Yeeked Up":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Such an accurate depiction of a little creature! Did you know that they have much stronger muscles within their tails than any other creature? Such interesting purposed organisms!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I wish I could keep this art piece up, but unfortunately, I have to keep my walls clean."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I apologize once again."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Squidburger":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...Is that a squidcada covered by bread?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I suspect that Five Pebbles' overseers still broadcast quick meal ads, despite the audience being long since gone. Have you gotten your inspiration from there?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I do find this depiction amusing. I might even send it to a few friends, if you don't mind...?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Salami_Hunter - Blek le Scav":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is it the face of a particular scavenger you met on your travels, or did you take artistic liberty with the depiction?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I'm sure they would enjoy having the portrait somewhere they could actually see it, little friend."), 0);
                    break;
                case "VinkiGraffiti/vinki/Salami_Hunter - Five Cans":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I don't even know what to think of this..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("What prompted you to come up with such a concept?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - City View":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("What a pleasing picture! The colors are quite vibrant and work together well."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is this Five Pebbles' city, viewed from afar?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I have a feeling he would enjoy this one... If it was painted over proper canvas, and not my chamber walls."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Crybaby":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...What a poor little thing! This creature looks very sad."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is it just how you view those organisms, or is it based on something? Maybe you are silently projecting your feelings onto them?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Or... You are simply mean to them."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("They are purposed organisms, previously kept within silver cages all their lives to light up otherwise dark areas, so they might not be the most stable now that they were freed from their duty."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I suppose their life might be better with freedom, although I can't help but wonder what their quality of life is like, outside of their initial purpose."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Poor creatures. And yet... They survived for so long."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Dropwig Thief":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Ah... Did those creatures steal your tools, and you felt the need to immortalize them forever?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("They are known for picking up items and food they find, not to use, but to lure unlucky scavengers into a trap. They use their incredibly heavy bodies to drop onto their prey and stun it, before injecting it with venom!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Did you know that the small oval plates over their back are as heavy and strong as metal?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I would advise you to be careful around randomly placed, useful objects in an otherwise empty area, but I'm sure you already know that."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Jellyfish":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a very accurate depiction of a jellyfish, swimming with pearls."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("If you find any pearl that you deem interesting enough, could you bring it to me? I would love to see what you may find."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Jetfish Friend":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("This one looks adorable!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Those creatures are usually quite mean, pushing other organisms underwater, which quickly becomes a risk of drowning for those unlucky few. Although I wouldn't mind becoming a friend of this one in particular."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Neuron Dance":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a very accurate depiction of a part of our systems."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("As much as I appreciate the attention to detail, I would prefer it if you didn't spend too long inside of our systems, snacking on our neurons."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Neither I nor Pebbles enjoy the feeling of having pieces of our bodies scratched up and eaten, I hope you understand."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Overseer Kiss":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...I'm afraid I don't understand?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I can tell that those are meant to be overseers of my neighbor, Five Pebbles, exchanging information."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("However... I can't tell what those red symbols are meant to portray in this context."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I have a feeling I don't want to know."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Holy Gooieduck":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I believe that this piece is based on an ancient species of dangling mold?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("The spores they squirt out were said to have an overwhelming odor, which works as great repellent for the carnivorous colonies of stationary worm species."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is that what you were trying to depict? If yes, I suppose you succeeded."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Gold Skull":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Do you know the meaning of the symbol you just painted?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("In this particular context, it means that the creature on the art piece is violent and dangerous, as it wishes harm upon you."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I wonder where you saw that symbol, and what exactly made you want to recreate it. Maybe my overseers?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("They are known for helping friendly creatures out every so often."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Dandelion Peaches":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It looks to me like the fruit of a feathery plant. Have you imagined it, or got inspired by an existing plant?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It appears to me this species might have evolved recently, as I have never seen one before."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is familiar, so with a little bit of cross-referencing I could possibly find its ancestors, which would be a nice little project to pass some time."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Where have you seen those, little creature? I would like to refresh my database with new entries."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Spore Bombs":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Why would you draw a centipede next to this specific species of fungus? It is incredibly toxic to all insects!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("That poor creature... It appears to me they know that if their scared eyes are any indicator."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Poor little thing."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Lilypuck Karma":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Oh... Huh. Those flowers definitely hold some resemblance to an ancient karma symbol, I wonder if it was intended?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("These flowers are a form of organic lighting for underground machinery, so I wonder where you saw them!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("They are also very sharp, so I advise you to be careful next time you see them."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Lil Moon":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...Is that me? It is quite adorable, thank you!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I'm touched that you would take time out of your cycle just to draw my puppet, little friend."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("However... I find it unfortunate that you had to paint it in such an unlucky spot. Maybe you could recreate it outside of my chamber next time?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Stolen Face":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("This amount of detail is quite incredible, for a little creature such as you!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("So far, it seems to be one of your best works, and it imitates ancient art quite well!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I definitely see the inspiration, and it truly makes vultures look scary, that's for sure. I'm glad I don't have to encounter them myself!"), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Batnip Lure":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a simple depiction of a fern-like plant. I believe that it is a fly lure, which was used often to lure away pests from areas where their continuous presence would be damaging."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I believe it is beneficial for you, however, as it definitely makes catching them easier!"), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Noodle Spears":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a depiction of an angered mother with two children, ready to attack. Or maybe they are teaching their children how to fight?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I imagine it would really hurt being pierced by that! Hopefully, you were never a target for their anger or their training."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("You probably know better what this was meant to depict than I do, as you are the artist of this piece."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Angry Pinecone":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Oh. I would advise you to be careful around those if you see them around, little creature."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Despite looking like plants, they are actually beehives, full of incredibly aggressive insects that will attack any nearby creature."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I highly recommend taking another route when you see one in your path."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Weapon Pile":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I believe that it is a pile of makeshift weapons, crafted by scavengers?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Those beings are incredibly smart, but I can't help but feel deeply concerned about the fact they have freely available explosives."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Hopefully, they won't try to use them near any of my exposed systems."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...And hopefully, you won't do so either, if you happen to trade them for any."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("This image also reminds me of something else, but you shouldn't worry about it, as you likely wouldn't understand anyway. Just a coincidence, I believe."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Gummy Lizard":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("This one looks both menacing and adorable at the same time! Is it a friend? Or an enemy? I wonder..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I surely hope that the liquid pouring from this poor lizard's mouth is not meant to be void fluid, as that would surely be unpleasant for the creature."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Leviathan Jaws":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Oh! Is this the maw of a leviathan? I wonder where you saw it, considering they live in really deep waters! They are quite big, after all."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I truly hope you never swam in deep waters too close to them."), 0);
                    break;
                case "VinkiGraffiti/vinki/ItsZeroThree - Seraphim":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I'm not entirely sure what it is meant to depict. A triangle? Maybe a prism?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...Do you know what a prism is? I would explain, but I'm not sure how much you would understand."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I'm most likely wrong about my interpretation as well."), 0);
                    break;
                case "VinkiGraffiti/vinki/JayDee - Pipe Slug":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("What a fun-looking creature! I don't really recognize it, but it looks quite interesting."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Have you seen it somewhere, or created it? If it was created by you, then you have a great imagination, little friend."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...Although I am a bit curious about the choice of symbol you drew."), 0);
                    break;
                case "VinkiGraffiti/vinki/NoneHereF - KARMA":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("An interesting piece, that's for sure. Based on the symbol, I believe the creature you drew is searching for ascension?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a difficult goal to achieve. I can only hope that they succeeded in what they wanted to do."), 0);
                    break;
                case "VinkiGraffiti/vinki/NoneHereF - Monk Yolk":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I'm unsure of what this piece is meant to portray. I apologize, but I do not recognize the symbols."), 0);
                    break;
                case "VinkiGraffiti/vinki/NoneHereF - Paint Lizard":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a lizard that had spray paint thrown at it. I hope that their eyes are alright, as paint in places it is not meant to be can be quite harmful, and might even cause loss of vision."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Although... I have a feeling you are quite proud of what you did, aren't you?"), 0);
                    break;
                case "VinkiGraffiti/vinki/NoneHereF - Vink and Vroom":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is your face, drawn over a green flame."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is it a self-portrait? It definitely is more odd than regular portraits."), 0);
                    break;
                case "VinkiGraffiti/vinki/Salami_Hunter - Cute as Fearsome":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a very adorable depiction of a lizard."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I don't believe it is very accurate to their anatomy, however. Isn't the head a bit too large?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Salami_Hunter - Observing Worms":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is it... An overseer? They usually aren't this colorful, you see."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("You definitely had some creative ideas while making this one, didn't you?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Salami_Hunter - Worm Thinketh":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I don't really recognize this creature, but it is very close to various abstract paintings I have seen my creators make."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I believe you got inspired by them at least a little bit, didn't you? It is surprisingly close in style."), 0);
                    break;
                case "VinkiGraffiti/vinki/Skrybl - Just a Lil Doodle":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I apologize, but I do not recognize any of those symbols."), 0);
                    break;
                case "VinkiGraffiti/vinki/Still-Icarus - Death be Upon Ye":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("This piece is... Incredibly chaotic, with how many colors were used."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I do believe it was intended, however, due to the nature of this particular picture."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I am impressed by the details on the can as well."), 0);
                    break;
                case "VinkiGraffiti/vinki/Still-Icarus - Night Terrors":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is it a lantern mouse? It does look vaguely similar, but there's something odd about this depiction. I do find this piece mildly unsettling."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Was it intended?"), 0);
                    break;
                case "VinkiGraffiti/vinki/SunnyBoi - Surprise":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I'm surprised you know about the existence of those, little friend."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("They hide so incredibly well, usually in really dark areas, it must've been hard to get a good look at one."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tiny Slugcat - Vinki Approved":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It's you. I cannot say that I recognize the gesture your hand is making, however."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Given the smile on your face, is it something pleasant?"), 0);
                    break;
                case "VinkiGraffiti/vinki/tealppup - Say No":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I'm quite confused by this art piece, I apologize. It appears that you do not approve of the use of mushrooms."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("They can indeed become dangerous after repeated ingestion, but I wonder why you dislike them so strongly. Do you not enjoy their taste?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Pop Pop Snails":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a depiction of a species of aquatic snail that lives in the watery areas under my can. They have a very unusual ability to let out a burst of energy, which might stun any nearby creature."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Are the golden lines around them a depiction of that process?"), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Living Paintbrush":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is it a hazer? I truly hope you are not planning on using their ink to create your paintings, as it is significantly more difficult to remove than your current painting materials."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...Please do not take this as a suggestion."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Acid Bath":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("This is... Concerning."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Please do not swim in any chemicals you might find inside or outside our structures, little friend! While the cycle will let you wake right back up again, it is truly an unpleasant way to go."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Please be careful."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Vulture Remote":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a depiction of a vulture grub, summoning multiple vultures."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Those two beings are truly interesting. Did you know they have a close symbiotic relationship? You depicted it, so of course you must be at least aware of it."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("When one of them gets grabbed, it will emit a laser, which causes any nearby vulture to be aware of a potential meal and fly over to check!"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("If they find no meal, however, they will grab and eat the grub, which is truly unfortunate for the grub."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Slugcat Moon":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is... Is it me?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("As one of you?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("While I appreciate the thought, I must deeply apologize, as I am unfortunately a completely different organism. "), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("My puppet might look mildly similar to you - which I truly do not see - but it is not my actual body. It is simply what I was given to express myself and speak to my creators directly."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("My true body is this entire structure that you are walking through, which might be difficult for you to comprehend."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I'm sorry I cannot be one of you, little friend."), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Questionable Centipede":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a red centipede, looking at a pearl. From the color, I can tell that it is the aggressive kind."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I would advise you to avoid coming near them, as they can be quite dangerous to a little creature such as you!"), 0);
                    break;
                case "VinkiGraffiti/vinki/Tsuno - Loud Pebbles":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("...I truly do not understand your insistence on drawing Five Pebbles with those ears."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("You are aware that neither of us have ears, aren't you? I do not see any reason why you would continue painting him with those elements."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Unless you managed to find some... Questionable outfits, left behind by our creators, and used them as an inspiration. In that case, please find different inspirations."), 0);
                    break;
                case "VinkiGraffiti/vinki/4slugcats - Speed of Sound":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("That is... Incredibly detailed. More so than most of your regular pieces. You seem incredibly proud of it, and I can understand why."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Are you sure you wouldn’t enjoy painting with proper canvas more, instead of my chamber walls? I can’t imagine this background was pleasant to create over segmented tiles."), 0);
                    break;
                case "VinkiGraffiti/vinki/ArtisticDragon1292 - Illegible Neon Signs":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Oh... I see you tried to imitate letters of an ancient language, which you likely encountered at some point during your time outside."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("While this is a great attempt, little scholar, what you wrote here is illegible. Would you like me to teach you how to write properly? You appear intelligent enough to understand a few words."), 0);
                    break;
                case "VinkiGraffiti/vinki/ArtisticDragon1292 - big shoe lmfao":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("I... I’m afraid I do not understand the meaning of this. Are you trying to tell me that you like your apparel? It is very nice, yes."), 0);
                    break;
                case "VinkiGraffiti/vinki/BreadwardBolero - Loss of Karma":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is this an attempt at replicating karma signs? I do not recognize this particular placement, so I will take it as an act of artistic freedom you took to make it look more pleasant."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("While not entirely accurate, I admire your dedication, little scholar."), 0);
                    break;
                case "VinkiGraffiti/vinki/BreadwardBolero - RIP Bozo":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is the head of a white lizard, possibly portraying it as deceased. Now, why would you draw it, I wonder? Have you won any battles recently that you are proud of?"), 0);
                    break;
                case "VinkiGraffiti/vinki/JayDee - VinKee":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Is it a self portrait I see? It is very accurate, I must admit."), 0);
                    break;
                case "VinkiGraffiti/vinki/Wheatley - Snail":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It’s a very cute, simple depiction of a colorful snail. Adorable."), 0);
                    break;
                case "VinkiGraffiti/vinki/fallingivyy - Colorful":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Have you based this one on a prism, mixed with a living being?"), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Unfortunately, I do not think your species is capable of reflecting light in such a manner. I wonder what might’ve made you come up with such an idea?"), 0);
                    break;
                case "VinkiGraffiti/vinki/raintailed - Green Vulture":
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Now let me take a look..."), 0);
                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("It is a simplistic drawing of a green vulture. I can applaud you for the complex perspective you managed to portray with the head, but unfortunately, there is not much more I can say about it."), 0);
                    break;
            }
        }

        public class DMSleepoverBehavior : ConversationBehavior
        {
            public DMSleepoverBehavior(SSOracleBehavior owner) : base(owner, Enums.DMOracle.Vinki_DMSlumberParty, Enums.DMOracle.Vinki_DMConvoFirstMeet)
            {
                lowGravity = -1f;
                if (this.owner.conversation != null)
                {
                    this.owner.conversation.Destroy();
                    this.owner.conversation = null;
                    return;
                }
                this.owner.TurnOffSSMusic(true);
                owner.getToWorking = 1f;

                // If this is picking up the pearl, or visit 3+ to Moon
                met = (base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.smPearlTagged || base.oracle.room.game.rainWorld.ExpeditionMode);
                SlugBaseSaveData miscSave = SaveDataExtension.GetSlugBaseData(base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData);
                if (miscSave.TryGet("SpawnUnlockablePearl", out int phase) && phase == 1)
                {
                    base.dialogBox.NewMessage(base.Translate("Hello, little friend!"), 0);
                    base.dialogBox.NewMessage(base.Translate("I have the pearl for you, with your mural safely stored within it."), 0);
                    base.dialogBox.NewMessage(base.Translate("If you would like to see it again, just hand me the pearl, and I will project it once again."), 0);
                    miscSave.Set("SpawnUnlockablePearl", 2);
                    return;
                }
                else if (miscSave.TryGet("MetMoonTwice", out int met2) || base.oracle.room.game.rainWorld.ExpeditionMode)
                {
                    if (met2 > 1 || base.oracle.room.game.rainWorld.ExpeditionMode)
                    {
                        float rand = Random.value;
                        if (rand < 0.2f)
                        {
                            base.dialogBox.NewMessage(base.Translate("Hello, " + owner.NameForPlayer(false) + "."), 0);
                            base.dialogBox.NewMessage(base.Translate("It is good to see you again, even if I have nothing to give you."), 0);
                        }
                        else if (rand < 0.4f)
                        {
                            base.dialogBox.NewMessage(base.Translate("Hello again, " + owner.NameForPlayer(false) + "."), 0);
                            base.dialogBox.NewMessage(base.Translate("How have you been?"), 0);
                        }
                        else if (rand < 0.6f)
                        {
                            base.dialogBox.NewMessage(base.Translate("Ah... " + owner.NameForPlayer(false) + ", you're back!"), 0);
                        }
                        else if (rand < 0.8f)
                        {
                            base.dialogBox.NewMessage(base.Translate("Hello, " + owner.NameForPlayer(false) + ". You're here again."), 0);
                        }
                        else
                        {
                            base.dialogBox.NewMessage(base.Translate("Hello again, " + owner.NameForPlayer(false) + "."), 0);
                        }
                    }
                    else
                    {
                        base.dialogBox.NewMessage(base.Translate("Welcome back unusual creature."), 0);
                        base.dialogBox.NewMessage(base.Translate("It seems to me like you were quite busy with your work, and yet you still made some time to visit me."), 0);
                        base.dialogBox.NewMessage(base.Translate("I wonder what is it that you want? Is it just to say hello?"), 0);
                        base.dialogBox.NewMessage(base.Translate("Feel free to visit anytime you wish, little creature. I don't mind."), 0);
                    }
                    miscSave.Set("MetMoonTwice", met2 + 1);
                    return;
                }

                // If this is a return visit to Moon
                if (met)
                {
                    convoID = Enums.DMOracle.Vinki_DMConvoSecondMeet;
                    owner.InitateConversation(convoID, this);
                    miscSave.Set("MetMoonTwice", 1);
                }
            }

            public override void Activate(Action oldAction, Action newAction)
            {
                base.Activate(oldAction, newAction);
            }

            public override void NewAction(Action oldAction, Action newAction)
            {
                base.NewAction(oldAction, newAction);
                if (newAction == SSOracleBehavior.Action.ThrowOut_KillOnSight && owner.conversation != null)
                {
                    owner.conversation.Destroy();
                    owner.conversation = null;
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
                else if (owner.oracle.room.game.StoryCharacter == Enums.vinki && owner.action == Enums.DMOracle.Vinki_DMActionGeneral && owner.conversation?.id == Enums.DMOracle.Vinki_DMConvoSecondMeet)
                {
                    owner.LockShortcuts();
                    owner.movementBehavior = SSOracleBehavior.MovementBehavior.KeepDistance;
                    //owner.gravOn = true;
                    if (owner.inActionCounter > 80 && (owner.conversation == null || (owner.conversation != null && owner.conversation.slatedForDeletion)))
                    {
                        owner.UnlockShortcuts();
                        owner.conversation = null;
                        owner.getToWorking = 1f;
                        met = base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.smPearlTagged = true;
                    }
                    return;
                }
                if (tagTimer > 0f && owner.inspectPearl != null)
                {
                    owner.killFac = Mathf.Clamp(tagTimer / 120f, 0f, 1f);
                    tagTimer -= 1f;
                    if (tagTimer <= 0f)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            base.oracle.room.AddObject(new Spark(owner.inspectPearl.firstChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                        }
                        base.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.inspectPearl.firstChunk.pos, 1f, 0.5f + Random.value * 0.5f);
                        owner.killFac = 0f;
                    }
                }
                if (holdPlayer && base.player.room == base.oracle.room)
                {
                    base.player.mainBodyChunk.vel *= Custom.LerpMap((float)base.inActionCounter, 0f, 30f, 1f, 0.95f);
                    base.player.bodyChunks[1].vel *= Custom.LerpMap((float)base.inActionCounter, 0f, 30f, 1f, 0.95f);
                    base.player.mainBodyChunk.vel += Custom.DirVec(base.player.mainBodyChunk.pos, HoldPlayerPos) * Mathf.Lerp(0.5f, Custom.LerpMap(Vector2.Distance(base.player.mainBodyChunk.pos, HoldPlayerPos), 30f, 150f, 2.5f, 7f), base.oracle.room.gravity) * Mathf.InverseLerp(0f, 10f, (float)base.inActionCounter) * Mathf.InverseLerp(0f, 30f, Vector2.Distance(base.player.mainBodyChunk.pos, HoldPlayerPos));
                }
                else
                {
                    owner.getToWorking = 1f;
                    if (lowGravity < 0f)
                    {
                        lowGravity = 0f;
                    }
                    owner.SetNewDestination(base.oracle.firstChunk.pos);
                }
            }

            private Vector2 HoldPlayerPos
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
                    return gravOn;
                }
            }

            public override float LowGravity
            {
                get
                {
                    return lowGravity;
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