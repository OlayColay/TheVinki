using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ArenaBehaviors;
using BepInEx;
using BepInEx.Logging;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RWCustom;
using UnityEngine;

namespace vinki
{
    // Token: 0x02000004 RID: 4
    [BepInPlugin("com.henpemaz.spawnmenu", "Spawn Menu", "0.1.0")]
    public class SpawnMenu : BaseUnityPlugin
    {
        // Token: 0x06000003 RID: 3 RVA: 0x00002069 File Offset: 0x00000269
        public void OnEnable()
        {
            Logger.LogInfo("OnEnable");
            sLogger = Logger;
            On.RainWorld.OnModsInit += OnModsInit;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x0000209C File Offset: 0x0000029C
        public void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            try
            {
                bool flag = init;
                if (!flag)
                {
                    init = true;
                    Logger.LogInfo("OnModsInit");
                    On.Menu.PauseMenu.ctor += PauseMenu_ctor;
                    On.Menu.PauseMenu.Update += PauseMenu_Update;
                    On.Menu.PauseMenu.GrafUpdate += PauseMenu_GrafUpdate;
                    On.Menu.PauseMenu.ShutDownProcess += PauseMenu_ShutDownProcess;
                    new Hook(typeof(ArenaGameBehavior).GetProperty("room").GetGetMethod(), typeof(SpawnMenu).GetMethod("get_room"), this);
                    On.ArenaBehaviors.SandboxEditor.LoadConfig += SandboxEditor_LoadConfig;
                    On.ArenaBehaviors.SandboxEditor.AddIcon_IconSymbolData_Vector2_EntityID_bool_bool += SandboxEditor_AddIcon_IconSymbolData_Vector2_EntityID_bool_bool;
                    On.ArenaBehaviors.SandboxEditor.CreatureOrItemIcon.DrawSprites += CreatureOrItemIcon_DrawSprites;
                    On.ArenaBehaviors.SandboxEditor.EditCursor.Update += EditCursor_Update;
                    On.ArenaBehaviors.SandboxEditor.EditCursor.OverseerEyePos += EditCursor_OverseerEyePos;
                    new Hook(typeof(SandboxEditor.EditCursor).GetProperty("OverseerActive").GetGetMethod(), typeof(SpawnMenu).GetMethod("get_OverseerActive"), this);
                    IL.ArenaBehaviors.SandboxEditor.EditCursor.Update += new ILContext.Manipulator(EditCursor_Update1);
                    Logger.LogInfo("OnModsInit done");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
            finally
            {
                orig.Invoke(self);
            }
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002254 File Offset: 0x00000454
        private void EditCursor_Update1(ILContext il)
        {
            try
            {
                ILCursor ilcursor = new ILCursor(il);
                ILLabel skip = null;
                ILCursor ilcursor2 = ilcursor;
                Func<Instruction, bool>[] array = new Func<Instruction, bool>[1];
                array[0] = (i) => i.MatchLdfld<SandboxEditor.EditCursor>("overseer");
                ilcursor2.GotoNext(array);
                _ = ilcursor.GotoNext(new Func<Instruction, bool>[]
                {
                    (i) => ILPatternMatchingExt.MatchBrfalse(i, out skip)
                });
                ILCursor ilcursor3 = ilcursor;
                Func<Instruction, bool>[] array2 = new Func<Instruction, bool>[1];
                array2[0] = (i) => i.MatchLdfld<SandboxEditor.EditCursor>("overseer");
                ilcursor3.GotoPrev(array2);
                ILCursor ilcursor4 = ilcursor;
                int index = ilcursor4.Index;
                ilcursor4.Index = index - 1;
                ilcursor.Emit(OpCodes.Ldarg_0);
                ilcursor.Emit<SandboxEditor.EditCursor>(OpCodes.Ldfld, "overseer");
                ilcursor.Emit(OpCodes.Brfalse, skip);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002358 File Offset: 0x00000558
        private Vector2 EditCursor_OverseerEyePos(On.ArenaBehaviors.SandboxEditor.EditCursor.orig_OverseerEyePos orig, SandboxEditor.EditCursor self, float timeStacker)
        {
            bool flag = self.room.game.session is StoryGameSession;
            Vector2 result;
            if (flag)
            {
                Creature realizedCreature = null;
                bool flag2;
                if (self.room.game.Players.Count > 0)
                {
                    realizedCreature = self.room.game.Players[0].realizedCreature;
                    flag2 = realizedCreature != null;
                }
                else
                {
                    flag2 = false;
                }
                bool flag3 = flag2;
                if (flag3)
                {
                    result = realizedCreature.firstChunk.pos;
                }
                else
                {
                    result = Vector2.zero;
                }
            }
            else
            {
                result = orig.Invoke(self, timeStacker);
            }
            return result;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000023E4 File Offset: 0x000005E4
        private void EditCursor_Update(On.ArenaBehaviors.SandboxEditor.EditCursor.orig_Update orig, SandboxEditor.EditCursor self, bool eu)
        {
            orig.Invoke(self, eu);
            bool flag = self.room.game.session is StoryGameSession;
            if (flag)
            {
                self.quality = 1f;
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002424 File Offset: 0x00000624
        public bool get_OverseerActive(orig_OverseerActive orig, SandboxEditor.EditCursor self)
        {
            bool flag = self.room.game.session is StoryGameSession;
            return !flag && orig(self);
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002460 File Offset: 0x00000660
        private void CreatureOrItemIcon_DrawSprites(On.ArenaBehaviors.SandboxEditor.CreatureOrItemIcon.orig_DrawSprites orig, SandboxEditor.CreatureOrItemIcon self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            bool flag = self.symbol == null;
            if (flag)
            {
                sLeaser.CleanSpritesAndRemove();
            }
            else
            {
                orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            }
        }

        // Token: 0x0600000A RID: 10 RVA: 0x00002494 File Offset: 0x00000694
        private void SandboxEditor_LoadConfig(On.ArenaBehaviors.SandboxEditor.orig_LoadConfig orig, SandboxEditor self)
        {
            bool isStorySession = self.sandboxSession.game.IsStorySession;
            if (isStorySession)
            {
                self.ClearAll();
            }
            else
            {
                orig.Invoke(self);
            }
        }

        // Token: 0x0600000B RID: 11 RVA: 0x000024C8 File Offset: 0x000006C8
        private SandboxEditor.PlacedIcon SandboxEditor_AddIcon_IconSymbolData_Vector2_EntityID_bool_bool(On.ArenaBehaviors.SandboxEditor.orig_AddIcon_IconSymbolData_Vector2_EntityID_bool_bool orig, SandboxEditor self, IconSymbol.IconSymbolData iconData, Vector2 pos, EntityID ID, bool fadeCircle, bool updatePerfEstimate)
        {
            SandboxEditor.PlacedIcon placedIcon = orig.Invoke(self, iconData, pos, ID, fadeCircle, updatePerfEstimate);
            bool isStorySession = self.sandboxSession.game.IsStorySession;
            if (isStorySession)
            {
                SpawnExtra spawnExtra = new SpawnExtra();
                spawnExtras.Add(placedIcon, spawnExtra);
                bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                if (flag)
                {
                    spawnExtra.like = true;
                }
                bool flag2 = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
                if (flag2)
                {
                    spawnExtra.dead = true;
                }
                bool flag3 = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                if (flag3)
                {
                    self.room.AddObject(new SeedPicker(self.room, placedIcon, this));
                }
            }
            return placedIcon;
        }

        // Token: 0x0600000C RID: 12 RVA: 0x000025B0 File Offset: 0x000007B0
        private void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, PauseMenu self, ProcessManager manager, RainWorldGame game)
        {
            orig.Invoke(self, manager, game);
            Logger.LogDebug("PauseMenu_ctor");
            bool flag = !game.IsStorySession || self.game.cameras[0].room == null;
            if (!flag)
            {
                try
                {
                    bool flag2 = manager.arenaSetup == null;
                    if (flag2)
                    {
                        manager.arenaSetup = new ArenaSetup(manager);
                    }
                    Room room = self.game.cameras[0].room;
                    SandboxGameSession sandboxGameSession = FormatterServices.GetUninitializedObject(typeof(SandboxGameSession)) as SandboxGameSession;
                    sandboxGameSession.arenaSitting = new ArenaSitting(manager.arenaSetup.GetOrInitiateGameTypeSetup(ArenaSetup.GameTypeID.Sandbox), new MultiplayerUnlocks(manager.rainWorld.progression, new List<string>()));
                    sandboxGameSession.arenaSitting.gameTypeSetup.saveCreatures = true;
                    sandboxGameSession.game = game;
                    room.AddObject(new SandboxOverlayOwner(room, sandboxGameSession, !sandboxGameSession.PlayMode));
                    GameSession session = game.session;
                    game.session = sandboxGameSession;
                    sandboxGameSession.overlay.Initiate(false);
                    game.session = session;
                    for (int i = 0; i < SandboxEditorSelector.Width; i++)
                    {
                        for (int j = 0; j < SandboxEditorSelector.Height; j++)
                        {
                            SandboxEditorSelector.Button button = sandboxGameSession.overlay.sandboxEditorSelector.buttons[i, j];
                            SandboxEditorSelector.CreatureOrItemButton creatureOrItemButton = sandboxGameSession.overlay.sandboxEditorSelector.buttons[i, j] as SandboxEditorSelector.CreatureOrItemButton;
                            bool flag3;
                            if (creatureOrItemButton == null || !(creatureOrItemButton.data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature) || !(creatureOrItemButton.data.critType == CreatureTemplate.Type.Slugcat))
                            {
                                SandboxEditorSelector.ActionButton actionButton = sandboxGameSession.overlay.sandboxEditorSelector.buttons[i, j] as SandboxEditorSelector.ActionButton;
                                flag3 = actionButton != null && (actionButton.action == SandboxEditorSelector.ActionButton.Action.Play || actionButton.action == SandboxEditorSelector.ActionButton.Action.Randomize || actionButton.action == SandboxEditorSelector.ActionButton.Action.ConfigA || actionButton.action == SandboxEditorSelector.ActionButton.Action.ConfigB || actionButton.action == SandboxEditorSelector.ActionButton.Action.ConfigC);
                            }
                            else
                            {
                                flag3 = true;
                            }
                            bool flag4 = flag3;
                            if (flag4)
                            {
                                sandboxGameSession.overlay.sandboxEditorSelector.buttons[i, j] = null;
                                sandboxGameSession.overlay.sandboxEditorSelector.RemoveSubObject(button);
                                button.RemoveSprites();
                            }
                        }
                    }
                    sandboxGameSession.editor = new SandboxEditor(sandboxGameSession);
                    sandboxGameSession.editor.currentConfig = -1;
                    SandboxEditor.EditCursor editCursor = new SandboxEditor.EditCursor(sandboxGameSession.editor, null, 0, new Vector2(-1000f, -1000f));
                    sandboxGameSession.editor.cursors.Add(editCursor);
                    room.AddObject(editCursor);
                    sandboxGameSession.overlay.sandboxEditorSelector.ConnectToEditor(sandboxGameSession.editor);
                    sandboxGameSession.sandboxInitiated = true;
                    sandboxGameSession.overlay.fadingOut = true;
                    Logger.LogDebug("PauseMenu_ctor trully really fully done");
                }
                catch (Exception ex)
                {
                    Logger.LogError("SpawnMenuMod failed to create sandbox menu");
                    Logger.LogError(ex);
                }
            }
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002920 File Offset: 0x00000B20
        private void PauseMenu_Update(On.Menu.PauseMenu.orig_Update orig, PauseMenu self)
        {
            bool flag = !self.game.IsStorySession || self.game.pauseMenu == null || self.game.cameras[0].room == null;
            if (flag)
            {
                orig.Invoke(self);
            }
            else
            {
                PauseMenu pauseMenu = self.game.pauseMenu;
                self.game.pauseMenu = null;
                Room room = self.game.cameras[0].room;
                SandboxOverlayOwner sandboxOverlayOwner = room.updateList.First((o) => o is SandboxOverlayOwner) as SandboxOverlayOwner;
                foreach (UpdatableAndDeletable updatableAndDeletable in room.updateList.ToList())
                {
                    bool flag2 = updatableAndDeletable is SandboxEditor.PlacedIcon || updatableAndDeletable is SeedPicker || updatableAndDeletable is SandboxOverlayOwner || updatableAndDeletable is SandboxEditor.EditCursor;
                    if (flag2)
                    {
                        updatableAndDeletable.Update(false);
                    }
                }
                self.game.pauseMenu = pauseMenu;
                bool flag3 = (sandboxOverlayOwner.overlay.sandboxEditorSelector.currentlyVisible || sandboxOverlayOwner.overlay.sandboxEditorSelector.editor.cursors[0].homeInIcon != null || sandboxOverlayOwner.overlay.sandboxEditorSelector.editor.cursors[0].dragIcon != null) && self.manager.upcomingProcess == null;
                if (flag3)
                {
                    self.pressButton = false;
                    self.manager.upcomingProcess = self.ID;
                    orig.Invoke(self);
                    self.manager.upcomingProcess = null;
                }
                else
                {
                    orig.Invoke(self);
                }
                bool flag4 = self.controlMap != null && (sandboxOverlayOwner.selector.visFac > 0f || sandboxOverlayOwner.selector.lastVisFac > 0f);
                if (flag4)
                {
                    self.controlMap.fade = Mathf.Clamp01(self.controlMap.fade - sandboxOverlayOwner.selector.visFac);
                    self.controlMap.lastFade = Mathf.Clamp01(self.controlMap.lastFade - sandboxOverlayOwner.selector.lastVisFac);
                    bool flag5 = self.controlMap.controlsMap != null;
                    if (flag5)
                    {
                        self.controlMap.controlsMap.setAlpha = new float?(self.controlMap.fade);
                    }
                    bool flag6 = self.controlMap.controlsMap2 != null;
                    if (flag6)
                    {
                        self.controlMap.controlsMap2.setAlpha = new float?(Mathf.Min(self.controlMap.fade, Custom.SCurve(Mathf.InverseLerp(5f, 80f, (float)self.controlMap.counter), 0.8f)));
                    }
                    bool flag7 = self.controlMap.controlsMap3 != null;
                    if (flag7)
                    {
                        self.controlMap.controlsMap3.setAlpha = new float?(Mathf.Min(self.controlMap.fade, Custom.SCurve(Mathf.InverseLerp(5f, 80f, (float)self.controlMap.counter), 0.8f)) * 0.5f);
                    }
                    bool flag8 = self.controlMap.pickupButtonInstructions != null;
                    if (flag8)
                    {
                        self.controlMap.pickupFade = Mathf.Clamp01(Custom.SCurve(Mathf.InverseLerp(40f, 120f, (float)self.controlMap.counter), 0.5f) - sandboxOverlayOwner.selector.visFac);
                        self.controlMap.lastPickupFade = Mathf.Clamp01(Custom.SCurve(Mathf.InverseLerp(40f, 120f, (float)self.controlMap.counter - 1f), 0.5f) - sandboxOverlayOwner.selector.lastVisFac);
                    }
                }
            }
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002D30 File Offset: 0x00000F30
        private void PauseMenu_GrafUpdate(On.Menu.PauseMenu.orig_GrafUpdate orig, PauseMenu self, float timeStacker)
        {
            orig.Invoke(self, timeStacker);
            bool flag = !self.game.IsStorySession || self.game.pauseMenu == null;
            if (!flag)
            {
                foreach (RoomCamera roomCamera in self.game.cameras)
                {
                    roomCamera.DrawUpdate(0f, 1f);
                }
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002D9C File Offset: 0x00000F9C
        private void PauseMenu_ShutDownProcess(On.Menu.PauseMenu.orig_ShutDownProcess orig, PauseMenu self)
        {
            try
            {
                bool isStorySession = self.game.IsStorySession;
                if (isStorySession)
                {
                    Room room = self.game.cameras[0].room;
                    SandboxOverlayOwner sandboxOverlayOwner = room.updateList.FirstOrDefault((o) => o is SandboxOverlayOwner && !o.slatedForDeletetion) as SandboxOverlayOwner;
                    bool flag = sandboxOverlayOwner == null;
                    if (!flag)
                    {
                        SandboxEditor editor = sandboxOverlayOwner.gameSession.editor;
                        sandboxOverlayOwner.gameSession.PlayMode = true;
                        On.World.GetAbstractRoom_int += World_GetAbstractRoom_int;
                        doDetour = false;
                        On.WorldCoordinate.ctor_int_int_int_int += WorldCoordinate_ctor;
                        currentRoomIndex = room.abstractRoom.index;
                        foreach (SandboxEditor.PlacedIcon placedIcon in editor.icons)
                        {
                            SandboxEditor.CreatureOrItemIcon creatureOrItemIcon = placedIcon as SandboxEditor.CreatureOrItemIcon;
                            bool flag2 = creatureOrItemIcon != null;
                            if (flag2)
                            {
                                SandboxEditor.PlacedIconData placedIconData = new SandboxEditor.PlacedIconData(creatureOrItemIcon.pos, creatureOrItemIcon.iconData, creatureOrItemIcon.ID);
                                SpawnExtra orCreateValue = spawnExtras.GetOrCreateValue(placedIcon);
                                bool flag3 = orCreateValue != null && orCreateValue.seed != null;
                                if (flag3)
                                {
                                    placedIconData.ID.number = orCreateValue.seed.Value;
                                }
                                Logger.LogInfo(string.Format("SpawnMenu spawning {0} {1}", creatureOrItemIcon.iconData.itemType, creatureOrItemIcon.iconData.critType));
                                doDetour = true;
                                room.world.singleRoomWorld = true;
                                sandboxOverlayOwner.gameSession.SpawnEntity(placedIconData);
                                room.world.singleRoomWorld = false;
                                doDetour = false;
                                AbstractPhysicalObject abstractPhysicalObject = room.abstractRoom.entities.Last() as AbstractPhysicalObject;
                                bool flag4 = abstractPhysicalObject != null;
                                if (flag4)
                                {
                                    AbstractCreature abstractCreature = null;
                                    bool flag5;
                                    if (orCreateValue != null)
                                    {
                                        abstractCreature = abstractPhysicalObject as AbstractCreature;
                                        flag5 = abstractCreature != null;
                                    }
                                    else
                                    {
                                        flag5 = false;
                                    }
                                    bool flag6 = flag5;
                                    if (flag6)
                                    {
                                        if (!orCreateValue.like)
                                        {
                                            goto IL_215;
                                        }
                                        CreatureState state = abstractCreature.state;
                                        if (state == null)
                                        {
                                            goto IL_215;
                                        }
                                        bool flag7 = state.socialMemory != null;
                                    IL_216:
                                        bool flag8 = flag7;
                                        goto IL_295;
                                    IL_215:
                                        flag7 = false;
                                        goto IL_216;
                                    }
                                IL_295:
                                    abstractPhysicalObject.RealizeInRoom();
                                    AbstractCreature abstractCreature3 = null;
                                    bool flag9;
                                    if (orCreateValue != null)
                                    {
                                        abstractCreature3 = abstractPhysicalObject as AbstractCreature;
                                        flag9 = abstractCreature3 != null;
                                    }
                                    else
                                    {
                                        flag9 = false;
                                    }
                                    bool flag10 = flag9;
                                    if (flag10)
                                    {
                                        bool flag11 = orCreateValue.dead && abstractCreature3.realizedCreature != null;
                                        if (flag11)
                                        {
                                            abstractCreature3.realizedCreature.Die();
                                        }
                                    }
                                }
                            }
                            placedIcon.Fade();
                        }
                        sandboxOverlayOwner.Destroy();
                        sandboxOverlayOwner.overlay.ShutDownProcess();
                        SandboxEditor.EditCursor editCursor = Enumerable.FirstOrDefault(room.updateList, (o) => o is SandboxEditor.EditCursor && !o.slatedForDeletetion) as SandboxEditor.EditCursor;
                        if (editCursor != null)
                        {
                            editCursor.Destroy();
                        }
                        foreach (RoomCamera roomCamera in self.game.cameras)
                        {
                            foreach (RoomCamera.SpriteLeaser spriteLeaser in roomCamera.spriteLeasers.ToList())
                            {
                                SandboxEditor.EditCursor editCursor2 = spriteLeaser.drawableObject as SandboxEditor.EditCursor;
                                bool flag12;
                                if (editCursor2 == null || !editCursor2.slatedForDeletetion)
                                {
                                    SandboxOverlayOwner sandboxOverlayOwner2 = spriteLeaser.drawableObject as SandboxOverlayOwner;
                                    flag12 = sandboxOverlayOwner2 != null && sandboxOverlayOwner2.slatedForDeletetion;
                                }
                                else
                                {
                                    flag12 = true;
                                }
                                bool flag13 = flag12;
                                if (flag13)
                                {
                                    spriteLeaser.CleanSpritesAndRemove();
                                    roomCamera.spriteLeasers.Remove(spriteLeaser);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                self.game.world.singleRoomWorld = false;
                On.WorldCoordinate.ctor_int_int_int_int -= WorldCoordinate_ctor;
                On.World.GetAbstractRoom_int -= World_GetAbstractRoom_int;
                doDetour = false;
                orig.Invoke(self);
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00003274 File Offset: 0x00001474
        private void WorldCoordinate_ctor(On.WorldCoordinate.orig_ctor_int_int_int_int orig, ref WorldCoordinate self, int room, int x, int y, int abstractNode)
        {
            bool flag = doDetour;
            if (flag)
            {
                orig.Invoke(ref self, currentRoomIndex, x, y, abstractNode);
            }
            else
            {
                orig.Invoke(ref self, room, x, y, abstractNode);
            }
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000032B4 File Offset: 0x000014B4
        private AbstractRoom World_GetAbstractRoom_int(On.World.orig_GetAbstractRoom_int orig, World self, int room)
        {
            bool flag = doDetour;
            AbstractRoom result;
            if (flag)
            {
                result = self.game.cameras[0].room.abstractRoom;
            }
            else
            {
                result = orig.Invoke(self, room);
            }
            return result;
        }

        // Token: 0x06000012 RID: 18 RVA: 0x000032F4 File Offset: 0x000014F4
        public Room get_room(orig_get_room orig, ArenaGameBehavior self)
        {
            bool isStorySession = self.gameSession.game.IsStorySession;
            Room result;
            if (isStorySession)
            {
                result = self.gameSession.game.cameras[0].room;
            }
            else
            {
                result = orig(self);
            }
            return result;
        }

        // Token: 0x04000002 RID: 2
        public bool init;

        // Token: 0x04000003 RID: 3
        public static ManualLogSource sLogger;

        // Token: 0x04000004 RID: 4
        private int currentRoomIndex;

        // Token: 0x04000005 RID: 5
        public ConditionalWeakTable<SandboxEditor.PlacedIcon, SpawnExtra> spawnExtras = new ConditionalWeakTable<SandboxEditor.PlacedIcon, SpawnExtra>();

        // Token: 0x04000006 RID: 6
        private bool doDetour;

        // Token: 0x02000005 RID: 5
        // (Invoke) Token: 0x06000015 RID: 21
        public delegate bool orig_OverseerActive(SandboxEditor.EditCursor self);

        // Token: 0x02000006 RID: 6
        public class SpawnExtra
        {
            // Token: 0x04000007 RID: 7
            public bool dead;

            // Token: 0x04000008 RID: 8
            public bool like;

            // Token: 0x04000009 RID: 9
            public int? seed;
        }

        // Token: 0x02000007 RID: 7
        // (Invoke) Token: 0x0600001A RID: 26
        public delegate Room orig_get_room(ArenaGameBehavior bhv);

        // Token: 0x02000008 RID: 8
        internal class SeedPicker : UpdatableAndDeletable, IDrawable
        {
            // Token: 0x0600001D RID: 29 RVA: 0x00003358 File Offset: 0x00001558
            public SeedPicker(Room room, SandboxEditor.PlacedIcon ico, SpawnMenu spawnMenuMod)
            {
                this.room = room;
                this.ico = ico;
                this.spawnMenuMod = spawnMenuMod;
                lastAlpha = 1f;
                alpha = 1f;
                text = "";
                active = true;
                foreach (UpdatableAndDeletable updatableAndDeletable in room.updateList)
                {
                    SeedPicker seedPicker = updatableAndDeletable as SeedPicker;
                    bool flag = seedPicker != null;
                    if (flag)
                    {
                        seedPicker.active = false;
                    }
                }
                iconOffset = new Vector2(22f, 0f);
                room.game.rainWorld.StartCoroutine(TextUpdater());
            }

            // Token: 0x0600001E RID: 30 RVA: 0x00003430 File Offset: 0x00001630
            public override void Update(bool eu)
            {
                base.Update(eu);
                bool slatedForDeletetion = base.slatedForDeletetion;
                if (!slatedForDeletetion)
                {
                    counter++;
                    lastAlpha = alpha;
                    alpha = 0.5f * alpha + 0.5f * (active ? Mathf.Clamp01(Mathf.Pow(UnityEngine.Random.Range(0.5f, 1.4f), 0.75f)) : Mathf.Pow(UnityEngine.Random.Range(0.6f, 0.9f), 0.5f));
                    iconOffset = new Vector2(22f, 0f);
                    bool flag = active && alpha < 0.85f;
                    if (flag)
                    {
                        iconOffset += UnityEngine.Random.insideUnitCircle * 1.5f;
                    }
                    SpawnExtra orCreateValue = spawnMenuMod.spawnExtras.GetOrCreateValue(ico);
                    bool flag2 = orCreateValue == null;
                    if (flag2)
                    {
                        Destroy();
                    }
                    else
                    {
                        int value;
                        bool flag3 = int.TryParse(text, out value);
                        if (flag3)
                        {
                            orCreateValue.seed = new int?(value);
                        }
                        else
                        {
                            orCreateValue.seed = null;
                        }
                    }
                }
            }

            // Token: 0x0600001F RID: 31 RVA: 0x00003572 File Offset: 0x00001772
            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
            }

            // Token: 0x06000020 RID: 32 RVA: 0x00003575 File Offset: 0x00001775
            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
            }

            // Token: 0x06000021 RID: 33 RVA: 0x00003578 File Offset: 0x00001778
            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                bool flag = slatedForDeletetion || room != rCam.room;
                if (flag)
                {
                    label = null;
                    sLeaser.CleanSpritesAndRemove();
                }
                else
                {
                    bool slatedForDeletetion = ico.slatedForDeletetion;
                    if (slatedForDeletetion)
                    {
                        Destroy();
                    }
                    else
                    {
                        float num = Mathf.Lerp(lastAlpha, alpha, timeStacker);
                        Vector2 vector = -camPos + ico.DrawPos(timeStacker) + iconOffset;
                        FSprite fsprite = sLeaser.sprites[0];
                        FSprite fsprite2 = sLeaser.sprites[1];
                        fsprite.SetPosition(vector);
                        fsprite.alpha = active ? 0.92f : 0.4f;
                        fsprite2.isVisible = active && counter / 20 % 2 == 0;
                        fsprite2.SetPosition(vector + new Vector2(text.Length * 12f, 0f));
                        fsprite2.alpha = num;
                        label.text = text;
                        label.SetPosition(vector);
                        label.alpha = num;
                    }
                }
            }

            // Token: 0x06000022 RID: 34 RVA: 0x000036C4 File Offset: 0x000018C4
            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                FContainer fcontainer = rCam.ReturnFContainer("HUD");
                FSprite fsprite = new FSprite("Futile_White", true)
                {
                    scaleX = 0.125f,
                    scaleY = 1.5f,
                    shader = rCam.game.rainWorld.Shaders["Hologram"]
                };
                fcontainer.AddChild(fsprite);
                FSprite fsprite2 = new FSprite("Futile_White", true)
                {
                    color = new Color(0f, 0f, 0f),
                    alpha = 0.8f,
                    anchorX = 0.2f,
                    anchorY = 0.5f,
                    scaleX = 6.25f,
                    scaleY = 2f,
                    shader = rCam.game.rainWorld.Shaders["FlatLight"]
                };
                fcontainer.AddChild(fsprite2);
                sLeaser.sprites = new FSprite[]
                {
                    fsprite2,
                    fsprite
                };
                FContainer[] array = new FContainer[1];
                FContainer fcontainer2 = array[0] = new FContainer();
                sLeaser.containers = array;
                fcontainer.AddChild(fcontainer2);
                label = new FLabel("DisplayFont", "ASDF")
                {
                    shader = rCam.game.rainWorld.Shaders["Hologram"],
                    alignment = FLabelAlignment.Left,
                    color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey)
                };
                fcontainer2.AddChild(label);
            }

            // Token: 0x06000023 RID: 35 RVA: 0x00003845 File Offset: 0x00001A45
            private IEnumerator TextUpdater()
            {
                yield return null;
                while (!slatedForDeletetion && room.game.processActive)
                {
                    bool flag = active;
                    if (flag)
                    {
                        foreach (char c in Input.inputString)
                        {
                            bool flag2 = c == '\b';
                            if (flag2)
                            {
                                bool flag3 = this.text.Length != 0;
                                if (flag3)
                                {
                                    this.text = this.text.Substring(0, this.text.Length - 1);
                                }
                            }
                            else
                            {
                                bool flag4 = c == '\n' || c == '\r';
                                if (flag4)
                                {
                                    active = false;
                                }
                                else
                                {
                                    bool flag5 = char.IsDigit(c) || this.text.Length == 0 && c == '-';
                                    if (flag5)
                                    {
                                        this.text += c.ToString();
                                    }
                                }
                            }
                        }
                    }
                    yield return null;
                }
                yield break;
            }

            // Token: 0x0400000A RID: 10
            private SandboxEditor.PlacedIcon ico;

            // Token: 0x0400000B RID: 11
            private readonly SpawnMenu spawnMenuMod;

            // Token: 0x0400000C RID: 12
            private float lastAlpha;

            // Token: 0x0400000D RID: 13
            private float alpha;

            // Token: 0x0400000E RID: 14
            private Vector2 iconOffset;

            // Token: 0x0400000F RID: 15
            private int counter;

            // Token: 0x04000010 RID: 16
            private FLabel label;

            // Token: 0x04000011 RID: 17
            private string text;

            // Token: 0x04000012 RID: 18
            private bool active;
        }
    }
}
