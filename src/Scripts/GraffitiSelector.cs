using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ArenaBehaviors;
using RWCustom;
using UnityEngine;
using Vinki;

namespace Menu
{
    public class GraffitiSelector : RectangularMenuObject
    {
        public static GraffitiSelector Instance;

        public GraffitiSelector(Menu menu, MenuObject owner) : base(menu, owner, new Vector2(-1000f, -1000f), new Vector2((float)GraffitiSelector.Width, (float)GraffitiSelector.Height) * GraffitiSelector.ButtonSize)
        {
            this.lastPos = new Vector2(-1000f, -1000f);
            //overlayOwner.selector = this;
            this.bkgRect = new RoundedRect(menu, this, new Vector2(-10f, -30f), this.size + new Vector2(20f, 60f), true);
            this.subObjects.Add(this.bkgRect);
            this.infoLabel = new MenuLabel(menu, this, "", new Vector2(this.size.x / 2f - 100f, 0f), new Vector2(200f, 20f), false, null);
            this.subObjects.Add(this.infoLabel);
            this.buttons = new GraffitiSelector.Button[GraffitiSelector.Width, GraffitiSelector.Height];
            int num = 0;
            this.AddButton(new GraffitiSelector.RectButton(menu, this, GraffitiSelector.ActionButton.Action.ClearAll), ref num);
            if (!ModManager.MSC)
            {
                for (int i = 0; i < 2; i++)
                {
                    this.AddButton(null, ref num);
                }
            }
            this.AddButton(new GraffitiSelector.RectButton(menu, this, GraffitiSelector.ActionButton.Action.Play), GraffitiSelector.Width - 1, 0);
            this.AddButton(new GraffitiSelector.RandomizeButton(menu, this), ModManager.MSC ? (GraffitiSelector.Width - 5) : (GraffitiSelector.Width - 6), 0);
            this.AddButton(new GraffitiSelector.ConfigButton(menu, this, GraffitiSelector.ActionButton.Action.ConfigA, 0), ModManager.MSC ? (GraffitiSelector.Width - 4) : (GraffitiSelector.Width - 5), 0);
            this.AddButton(new GraffitiSelector.ConfigButton(menu, this, GraffitiSelector.ActionButton.Action.ConfigB, 1), ModManager.MSC ? (GraffitiSelector.Width - 3) : (GraffitiSelector.Width - 4), 0);
            this.AddButton(new GraffitiSelector.ConfigButton(menu, this, GraffitiSelector.ActionButton.Action.ConfigC, 2), ModManager.MSC ? (GraffitiSelector.Width - 2) : (GraffitiSelector.Width - 3), 0);
            for (int j = 0; j < GraffitiSelector.Width; j++)
            {
                for (int k = 0; k < GraffitiSelector.Height; k++)
                {
                    if (this.buttons[j, k] != null)
                    {
                        this.buttons[j, k].Initiate(new IntVector2(j, k));
                    }
                }
            }
            this.cursors = new List<GraffitiSelector.ButtonCursor>();
        }
        //public void ConnectToEditor(SandboxEditor editor)
        //{
        //    this.editor = editor;
        //    for (int i = 0; i < editor.cursors.Count; i++)
        //    {
        //        GraffitiSelector.ButtonCursor item = new GraffitiSelector.ButtonCursor(this.menu, this, new IntVector2(i, GraffitiSelector.Height - 1), editor.cursors[i]);
        //        this.cursors.Add(item);
        //        this.subObjects.Add(item);
        //    }
        //}
        private void AddButton(GraffitiSelector.Button button, ref int counter)
        {
            int num = counter / this.buttons.GetLength(0);
            int x = counter - num * this.buttons.GetLength(0);
            this.AddButton(button, x, this.buttons.GetLength(1) - 1 - num);
            counter++;
        }
        private void AddButton(GraffitiSelector.Button button, int x, int y)
        {
            if (x < 0 || x >= this.buttons.GetLength(0) || y < 0 || y >= this.buttons.GetLength(1))
            {
                return;
            }
            this.buttons[x, y] = button;
            if (button != null)
            {
                button.intPos = new IntVector2(x, y);
                this.subObjects.Add(button);
            }
        }
        public void ActionButtonClicked(GraffitiSelector.ActionButton actionButton)
        {
            if (actionButton.action == GraffitiSelector.ActionButton.Action.ClearAll)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Clear_All);
                this.editor.ClearAll();
                this.editor.UpdatePerformanceEstimate();
                return;
            }
            if (actionButton.action == GraffitiSelector.ActionButton.Action.Play)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Play);
                this.editor.Play();
                return;
            }
            if (actionButton.action == GraffitiSelector.ActionButton.Action.ConfigA)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Switch_Config);
                this.editor.SwitchConfig(0);
                return;
            }
            if (actionButton.action == GraffitiSelector.ActionButton.Action.ConfigB)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Switch_Config);
                this.editor.SwitchConfig(1);
                return;
            }
            if (actionButton.action == GraffitiSelector.ActionButton.Action.ConfigC)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Switch_Config);
                this.editor.SwitchConfig(2);
                return;
            }
            if (actionButton.action == GraffitiSelector.ActionButton.Action.Randomize)
            {
                (actionButton as GraffitiSelector.RandomizeButton).Active = !(actionButton as GraffitiSelector.RandomizeButton).Active;
                this.menu.PlaySound((actionButton as GraffitiSelector.RandomizeButton).Active ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
                this.UpdateInfoLabel(actionButton.intPos.x, actionButton.intPos.y);
            }
        }
        public void UpdateInfoLabel(int x, int y)
        {
            if (this.editor.performanceWarning > 0 || x < 0 || x >= GraffitiSelector.Width || y < 0 || y >= GraffitiSelector.Height)
            {
                return;
            }
            this.infoLabel.text = ((this.buttons[x, y] != null) ? this.buttons[x, y].DescriptorText : "");
        }
        public void MouseCursorEnterMenuMode(SandboxEditor.EditCursor cursor)
        {
            if (cursor.ScreenPos.y < this.size.y + 20f)
            {
                this.mouseModeClickedDownLow = true;
            }
        }
        public override void Update()
        {
            this.lastVisFac = this.visFac;
            this.counter++;
            base.Update();
            this.pos.x = this.player.room.game.cameras[0].sSize.x * 0.5f - this.size.x * 0.5f + Menu.HorizontalMoveToGetCentered(this.menu.manager);
            if (this.editor.performanceWarning > 0)
            {
                if (this.editor.performanceWarning == 1)
                {
                    this.infoLabel.text = this.menu.Translate("Warning, too many creatures may result in poor game performance.");
                }
                else if (this.editor.performanceWarning == 2)
                {
                    this.infoLabel.text = this.menu.Translate("WARNING! Too many creatures may result in bad game performance or crashes.");
                }
            }
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            for (int i = 0; i < this.editor.cursors.Count; i++)
            {
                if (this.editor.cursors[i].menuMode)
                {
                    flag = true;
                }
                if (this.editor.cursors[i].mouseMode)
                {
                    if (this.editor.cursors[i].menuMode)
                    {
                        flag4 = true;
                    }
                }
                else
                {
                    if (this.editor.cursors[i].ScreenPos.y < this.size.y + 20f)
                    {
                        flag2 = true;
                    }
                    if (this.editor.cursors[i].ScreenPos.y > 768f - (this.size.y + 20f))
                    {
                        flag3 = true;
                    }
                }
            }
            if (this.mouseModeClickedDownLow)
            {
                flag2 = true;
                if (!flag4)
                {
                    this.mouseModeClickedDownLow = false;
                }
            }
            if (this.editor.gameSession.game.pauseMenu != null)
            {
                flag = false;
            }
            bool flag5 = flag2 && !flag3;
            if (this.upTop != flag5)
            {
                this.currentlyVisible = false;
                this.lingerCounter = 0;
                if (this.visFac == 0f && this.lastVisFac == 0f)
                {
                    this.upTop = flag5;
                    this.pos.y = (this.upTop ? 808f : (-40f - this.size.y));
                    this.lastPos.y = this.pos.y;
                }
            }
            else if (flag)
            {
                this.currentlyVisible = true;
                this.lingerCounter = 15;
            }
            else
            {
                this.lingerCounter--;
                if (this.lingerCounter < 1)
                {
                    this.currentlyVisible = false;
                }
            }
            this.visFac = Custom.LerpAndTick(this.visFac, this.currentlyVisible ? 1f : 0f, 0.03f, 0.05f);
            if (this.upTop)
            {
                this.pos.y = Mathf.Lerp(808f, 768f - this.size.y - 5f, Custom.SCurve(Mathf.Pow(this.visFac, 0.6f), 0.75f));
                this.infoLabel.pos.y = -20f;
                return;
            }
            this.pos.y = Mathf.Lerp(-40f - this.size.y, 5f, Custom.SCurve(Mathf.Pow(this.visFac, 0.6f), 0.75f));
            this.infoLabel.pos.y = this.size.y;
        }
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (this.editor.performanceWarning == 0)
            {
                this.infoLabel.label.color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
            }
            else if (this.editor.performanceWarning == 1)
            {
                this.infoLabel.label.color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Color.red, 0.5f + 0.5f * Mathf.Sin(((float)this.counter + timeStacker) / 30f * 3.1415927f * 2f));
            }
            else if (this.editor.performanceWarning == 2)
            {
                this.infoLabel.label.color = ((this.counter % 10 < 4) ? Menu.MenuRGB(Menu.MenuColors.White) : Color.red);
            }
            for (int i = 0; i < 9; i++)
            {
                this.bkgRect.sprites[i].color = Color.black;
                this.bkgRect.sprites[i].alpha = 0.75f;
                this.bkgRect.sprites[i].isVisible = true;
            }
            Color color = Menu.MenuRGB(Menu.MenuColors.DarkGrey);
            for (int j = 9; j < 17; j++)
            {
                this.bkgRect.sprites[j].color = color;
                this.bkgRect.sprites[j].alpha = 1f;
                this.bkgRect.sprites[j].isVisible = true;
            }
        }

        private RoundedRect bkgRect;
        public SandboxEditor editor;
        public bool currentlyVisible;
        public int lingerCounter;
        public float visFac;
        public float lastVisFac;
        public GraffitiSelector.Button[,] buttons;
        public static float ButtonSize = 50f;
        public static int Width = 19;
        public static int Height = 4;
        public bool upTop;
        public bool mouseModeClickedDownLow;
        public MenuLabel infoLabel;
        public List<GraffitiSelector.ButtonCursor> cursors;
        private int counter;
        public Player player;

        public class ButtonCursor : RectangularMenuObject
        {
            public GraffitiSelector EditorSelector
            {
                get
                {
                    return this.owner as GraffitiSelector;
                }
            }
            public ButtonCursor(Menu menu, MenuObject owner, IntVector2 intPos, Player roomCursor) : base(menu, owner, intPos.ToVector2() * GraffitiSelector.ButtonSize, new Vector2(GraffitiSelector.ButtonSize, GraffitiSelector.ButtonSize))
            {
                this.intPos = intPos;
                this.roundedRect = new RoundedRect(menu, this, new Vector2(-2f, -2f), this.size + new Vector2(4f, 4f), false);
                this.subObjects.Add(this.roundedRect);
                this.extraRect = new RoundedRect(menu, this, new Vector2(-2f, -2f), this.size + new Vector2(4f, 4f), false);
                this.subObjects.Add(this.extraRect);
                this.selectedButton = this.EditorSelector.buttons[intPos.x, intPos.y];
                this.player = roomCursor;
            }
            public void Move(int xAdd, int yAdd)
            {
                if (xAdd == 0 && yAdd == 0)
                {
                    return;
                }
                this.menu.PlaySound(SoundID.SANDBOX_Move_Library_Cursor);
                this.intPos.x = this.intPos.x + xAdd;
                if (this.intPos.x < 0)
                {
                    this.intPos.x = GraffitiSelector.Width - 1;
                }
                else if (this.intPos.x >= GraffitiSelector.Width)
                {
                    this.intPos.x = 0;
                }
                this.intPos.y = this.intPos.y + yAdd;
                if (this.intPos.y < 0)
                {
                    this.intPos.y = GraffitiSelector.Height - 1;
                }
                else if (this.intPos.y >= GraffitiSelector.Height)
                {
                    this.intPos.y = 0;
                }
                this.stillCounter = 0;
                this.clickOnRelease = false;
                this.selectedButton = this.EditorSelector.buttons[this.intPos.x, this.intPos.y];
                if (this.selectedButton != null)
                {
                    this.selectedButton.Flash();
                }
            }
            public void Click()
            {
                this.clickOnRelease = false;
                if (this.intPos.x < 0 || this.intPos.x >= GraffitiSelector.Width || this.intPos.y < 0 || this.intPos.y >= GraffitiSelector.Height || this.EditorSelector.buttons[this.intPos.x, this.intPos.y] == null)
                {
                    this.menu.PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
                    return;
                }
                this.EditorSelector.buttons[this.intPos.x, this.intPos.y].Clicked(this);
            }
            public override void Update()
            {
                base.Update();
                this.lastActive = this.active;
                this.lastSin = this.sin;
                this.lastInnerVisible = this.innerVisible;
                this.lastMouseIntPos = IntVector2.FromVector2(this.player.mainBodyChunk.pos);
                this.active = Custom.LerpAndTick(this.active, this.isActive ? 1f : 0f, 0.03f, 0.06666667f);
                Vector2 vector = this.intPos.ToVector2() * GraffitiSelector.ButtonSize;
                this.pos = Vector2.Lerp(Custom.MoveTowards(this.pos, vector, 5f), vector, 0.4f);
                float t = ((Custom.DistLess(this.pos, vector, 10f) && !this.player.input[0].thrw) ? 1f : 0f) * this.active;
                this.roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, 0f, t);
                this.extraRect.addSize = new Vector2(1f, 1f) * -8f;
                this.stillCounter++;
                this.sin += this.innerVisible;
                this.innerVisible = Custom.LerpAndTick(this.innerVisible, Mathf.InverseLerp(10f, 30f, (float)this.stillCounter), 0.06f, 0.04f);
            }
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                float num = Custom.SCurve(Mathf.Lerp(this.lastActive, this.active, timeStacker), 0.65f);
                Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), PlayerGraphics.DefaultSlugcatColor(player.slugcatStats.name), 0.2f + 0.8f * num);
                for (int i = 0; i < this.roundedRect.sprites.Length; i++)
                {
                    this.roundedRect.sprites[i].color = color;
                    this.roundedRect.sprites[i].alpha = 1f;
                    this.roundedRect.sprites[i].isVisible = true;
                }
                float num2 = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(this.lastSin, this.sin, timeStacker) / 30f * 3.1415927f * 2f);
                num2 *= num * Mathf.Lerp(this.lastInnerVisible, this.innerVisible, timeStacker);
                for (int j = 0; j < this.extraRect.sprites.Length; j++)
                {
                    this.extraRect.sprites[j].color = color;
                    this.extraRect.sprites[j].alpha = num2;
                    this.extraRect.sprites[j].isVisible = true;
                }
            }
            public override void RemoveSprites()
            {
                base.RemoveSprites();
            }
            public IntVector2 intPos;
            private RoundedRect roundedRect;
            private RoundedRect extraRect;
            public GraffitiSelector.Button selectedButton;
            public Player player;
            public bool isActive;
            private float active;
            private float lastActive;
            private float sin;
            private float lastSin;
            private float innerVisible;
            private float lastInnerVisible;
            private int stillCounter;
            public bool clickOnRelease;
            public IntVector2 lastMouseIntPos;
        }
        public class Button : RectangularMenuObject
        {
            public GraffitiSelector EditorSelector
            {
                get
                {
                    return this.owner as GraffitiSelector;
                }
            }
            public virtual string DescriptorText
            {
                get
                {
                    return "";
                }
            }
            public override bool Selected
            {
                get
                {
                    for (int i = 0; i < this.EditorSelector.cursors.Count; i++)
                    {
                        if (this.EditorSelector.cursors[i].isActive && this.EditorSelector.cursors[i].selectedButton == this)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            public virtual float White(float timeStacker)
            {
                return Mathf.Lerp(this.lastSin, this.sin, timeStacker) * (0.5f + 0.5f * Mathf.Sin(((float)this.counter + timeStacker) / 30f * 3.1415927f * 2f));
            }
            public Button(Menu menu, MenuObject owner) : base(menu, owner, default(Vector2), new Vector2(GraffitiSelector.ButtonSize, GraffitiSelector.ButtonSize))
            {
            }
            public virtual void Initiate(IntVector2 intPos)
            {
                this.intPos = intPos;
                this.pos = intPos.ToVector2() * GraffitiSelector.ButtonSize;
                this.lastPos = this.pos;
                if (intPos.x < GraffitiSelector.Width - 1 && this.EditorSelector.buttons[intPos.x + 1, intPos.y] != null)
                {
                    this.rightDivider = new FSprite("pixel", true);
                    this.rightDivider.scaleX = 2f;
                    this.rightDivider.scaleY = GraffitiSelector.ButtonSize - 15f;
                    this.rightDivider.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
                    this.Container.AddChild(this.rightDivider);
                }
                if (intPos.y < GraffitiSelector.Height - 1 && this.EditorSelector.buttons[intPos.x, intPos.y + 1] != null)
                {
                    this.upDivider = new FSprite("pixel", true);
                    this.upDivider.scaleX = GraffitiSelector.ButtonSize - 15f;
                    this.upDivider.scaleY = 2f;
                    this.upDivider.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
                    this.Container.AddChild(this.upDivider);
                }
            }
            public virtual void Flash()
            {
            }
            public virtual void Clicked(GraffitiSelector.ButtonCursor cursor)
            {
            }
            public override void Update()
            {
                base.Update();
                this.lastSin = this.sin;
                this.counter++;
                this.sin = Custom.LerpAndTick(this.sin, this.Selected ? 1f : 0f, 0.03f, 0.033333335f);
            }
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                Vector2 vector = this.DrawPos(timeStacker);
                if (this.rightDivider != null)
                {
                    this.rightDivider.x = vector.x + this.size.x;
                    this.rightDivider.y = vector.y + this.size.y / 2f;
                }
                if (this.upDivider != null)
                {
                    this.upDivider.x = vector.x + this.size.x / 2f;
                    this.upDivider.y = vector.y + this.size.y;
                }
            }
            public override void RemoveSprites()
            {
                if (this.rightDivider != null)
                {
                    this.rightDivider.RemoveFromContainer();
                }
                if (this.upDivider != null)
                {
                    this.upDivider.RemoveFromContainer();
                }
                base.RemoveSprites();
            }
            public IntVector2 intPos;
            public FSprite rightDivider;
            public FSprite upDivider;
            protected int counter;
            public float lastSin;
            public float sin;
        }
        public class CreatureOrItemButton : GraffitiSelector.Button
        {
            public IconSymbol.IconSymbolData data
            {
                get
                {
                    return this.symbol.iconData;
                }
            }
            public override string DescriptorText
            {
                get
                {
                    return this.menu.Translate((this.data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature) ? "Add creature" : "Add item");
                }
            }
            public CreatureOrItemButton(Menu menu, MenuObject owner, IconSymbol.IconSymbolData data) : base(menu, owner)
            {
                this.symbol = IconSymbol.CreateIconSymbol(data, this.Container);
                this.symbol.Show(true);
            }
            public override void Flash()
            {
                base.Flash();
                this.symbol.showFlash = 1f;
            }
            public override void Clicked(GraffitiSelector.ButtonCursor cursor)
            {
                base.Clicked(cursor);
                //cursor.roomCursor.SpawnObject(this.data, cursor.roomCursor.room.game.GetNewID());
            }
            public override void Update()
            {
                base.Update();
                this.symbol.Update();
                this.symbol.showFlash = Mathf.Max(this.symbol.showFlash, this.White(1f));
            }
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                this.symbol.Draw(timeStacker, this.DrawPos(timeStacker) + base.DrawSize(timeStacker) / 2f);
            }
            public override void RemoveSprites()
            {
                base.RemoveSprites();
                this.symbol.RemoveSprites();
            }
            public IconSymbol symbol;
        }
        public abstract class ActionButton : GraffitiSelector.Button
        {
            public override string DescriptorText
            {
                get
                {
                    return this.descriptorText;
                }
            }
            public override float White(float timeStacker)
            {
                return Mathf.Max(Mathf.Lerp(this.lastBump, this.bump, timeStacker), base.White(timeStacker));
            }
            public virtual Color MyColor(float timeStacker)
            {
                if (this.action == GraffitiSelector.ActionButton.Action.Play && base.EditorSelector.editor.performanceWarning > 0)
                {
                    return Color.Lerp(this.symbolSpriteColor, Color.red, 0.5f + 0.5f * Mathf.Sin(((float)this.counter + timeStacker) / 30f * 3.1415927f * 2f));
                }
                return Color.Lerp(this.symbolSpriteColor, Color.white, this.White(timeStacker));
            }
            public override void Flash()
            {
                base.Flash();
                this.bump = 1f;
            }
            public ActionButton(Menu menu, MenuObject owner, GraffitiSelector.ActionButton.Action action) : base(menu, owner)
            {
                this.action = action;
                this.descriptorText = "";
                this.symbolSpriteName = "Futile_White";
                this.symbolSpriteColor = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
                if (action == GraffitiSelector.ActionButton.Action.ClearAll)
                {
                    this.symbolSpriteName = "Sandbox_ClearAll";
                    this.descriptorText = menu.Translate("Clear all");
                }
                else if (action == GraffitiSelector.ActionButton.Action.Play)
                {
                    this.symbolSpriteName = "Sandbox_Play";
                    this.descriptorText = menu.Translate("Play!");
                }
                else if (action == GraffitiSelector.ActionButton.Action.ConfigA)
                {
                    this.symbolSpriteName = "Sandbox_A";
                    this.descriptorText = menu.Translate("Configuration") + " A";
                }
                else if (action == GraffitiSelector.ActionButton.Action.ConfigB)
                {
                    this.symbolSpriteName = "Sandbox_B";
                    this.descriptorText = menu.Translate("Configuration") + " B";
                }
                else if (action == GraffitiSelector.ActionButton.Action.ConfigC)
                {
                    this.symbolSpriteName = "Sandbox_C";
                    this.descriptorText = menu.Translate("Configuration") + " C";
                }
                else if (action == GraffitiSelector.ActionButton.Action.Locked)
                {
                    this.symbolSpriteName = "Sandbox_QuestionMark";
                }
                else if (action == GraffitiSelector.ActionButton.Action.Randomize)
                {
                    this.symbolSpriteName = "Sandbox_Randomize";
                }
                this.shadow1 = new FSprite(this.symbolSpriteName, true);
                this.shadow1.color = Color.black;
                this.Container.AddChild(this.shadow1);
                this.shadow2 = new FSprite(this.symbolSpriteName, true);
                this.shadow2.color = Color.black;
                this.Container.AddChild(this.shadow2);
                this.symbolSprite = new FSprite(this.symbolSpriteName, true);
                this.symbolSprite.color = this.symbolSpriteColor;
                this.Container.AddChild(this.symbolSprite);
            }
            public override void Clicked(GraffitiSelector.ButtonCursor cursor)
            {
                base.Clicked(cursor);
                base.EditorSelector.ActionButtonClicked(this);
                this.bump = 1f;
            }
            public override void Update()
            {
                this.lastBump = this.bump;
                this.bump = Mathf.Max(0f, this.bump - 0.05f);
                base.Update();
            }
            public override void GrafUpdate(float timeStacker)
            {
                Vector2 vector = this.DrawPos(timeStacker) + base.DrawSize(timeStacker) / 2f;
                this.shadow1.x = vector.x - 2f;
                this.shadow1.y = vector.y - 1f;
                this.shadow2.x = vector.x - 1f;
                this.shadow2.y = vector.y + 1f;
                this.symbolSprite.x = vector.x;
                this.symbolSprite.y = vector.y;
                this.symbolSprite.color = this.MyColor(timeStacker);
                base.GrafUpdate(timeStacker);
            }
            public override void RemoveSprites()
            {
                this.shadow1.RemoveFromContainer();
                this.shadow2.RemoveFromContainer();
                this.symbolSprite.RemoveFromContainer();
                base.RemoveSprites();
            }
            public GraffitiSelector.ActionButton.Action action;
            public string symbolSpriteName;
            public string descriptorText;
            public Color symbolSpriteColor;
            public FSprite symbolSprite;
            public FSprite shadow1;
            public FSprite shadow2;
            public float bump;
            public float lastBump;
            public class Action : ExtEnum<GraffitiSelector.ActionButton.Action>
            {
                public Action(string value, bool register = false) : base(value, register)
                {
                }
                public static readonly GraffitiSelector.ActionButton.Action ClearAll = new GraffitiSelector.ActionButton.Action("ClearAll", true);
                public static readonly GraffitiSelector.ActionButton.Action Play = new GraffitiSelector.ActionButton.Action("Play", true);
                public static readonly GraffitiSelector.ActionButton.Action Randomize = new GraffitiSelector.ActionButton.Action("Randomize", true);
                public static readonly GraffitiSelector.ActionButton.Action ConfigA = new GraffitiSelector.ActionButton.Action("ConfigA", true);
                public static readonly GraffitiSelector.ActionButton.Action ConfigB = new GraffitiSelector.ActionButton.Action("ConfigB", true);
                public static readonly GraffitiSelector.ActionButton.Action ConfigC = new GraffitiSelector.ActionButton.Action("ConfigC", true);
                public static readonly GraffitiSelector.ActionButton.Action Locked = new GraffitiSelector.ActionButton.Action("Locked", true);
            }
        }
        public class LockedButton : GraffitiSelector.ActionButton
        {
            public override Color MyColor(float timeStacker)
            {
                return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(this.lastBump, this.bump, timeStacker));
            }
            public LockedButton(Menu menu, MenuObject owner) : base(menu, owner, GraffitiSelector.ActionButton.Action.Locked)
            {
            }
            public override void Update()
            {
                base.Update();
            }
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
            }
        }
        public class RandomizeButton : GraffitiSelector.ActionButton
        {
            public bool Active
            {
                get
                {
                    return !base.EditorSelector.editor.gameSession.GameTypeSetup.saveCreatures;
                }
                set
                {
                    base.EditorSelector.editor.gameSession.GameTypeSetup.saveCreatures = !value;
                }
            }
            public override string DescriptorText
            {
                get
                {
                    return this.menu.Translate(this.Active ? "Random creatures" : "Persistent creatures");
                }
            }
            public override Color MyColor(float timeStacker)
            {
                if (this.Active)
                {
                    return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(this.lastSin, this.sin, timeStacker));
                }
                return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(this.lastBump, this.bump, timeStacker));
            }
            public RandomizeButton(Menu menu, MenuObject owner) : base(menu, owner, GraffitiSelector.ActionButton.Action.Randomize)
            {
            }
            public override void Update()
            {
                base.Update();
            }
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
            }
        }
        public class RectButton : GraffitiSelector.ActionButton
        {
            public RectButton(Menu menu, MenuObject owner, GraffitiSelector.ActionButton.Action action) : base(menu, owner, action)
            {
                this.roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), this.size, false);
                this.subObjects.Add(this.roundedRect);
            }
            public override void Update()
            {
                base.Update();
                this.roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, -2f, this.bump);
            }
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.White), this.White(timeStacker));
                float alpha = 1f - Mathf.Lerp(this.lastSin, this.sin, timeStacker);
                for (int i = 0; i < this.roundedRect.sprites.Length; i++)
                {
                    this.roundedRect.sprites[i].color = color;
                    this.roundedRect.sprites[i].alpha = alpha;
                    this.roundedRect.sprites[i].isVisible = true;
                }
            }
            public RoundedRect roundedRect;
        }
        public class ConfigButton : GraffitiSelector.ActionButton
        {
            public ConfigButton(Menu menu, MenuObject owner, GraffitiSelector.ActionButton.Action action, int configNumber) : base(menu, owner, action)
            {
                this.configNumber = configNumber;
                this.roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), this.size, false);
                this.subObjects.Add(this.roundedRect);
            }
            public override void Update()
            {
                base.Update();
                this.lastRectVisible = this.rectVisible;
                this.rectVisible = Custom.LerpAndTick(this.rectVisible, (base.EditorSelector.editor.currentConfig == this.configNumber) ? 1f : 0f, 0.03f, 0.033333335f);
                this.roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, -2f, this.bump);
            }
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                this.symbolSprite.color = Color.Lerp(Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), Mathf.Lerp(this.lastRectVisible, this.rectVisible, timeStacker)), Menu.MenuRGB(Menu.MenuColors.White), this.White(timeStacker));
                Color color = Color.Lerp(Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(this.lastRectVisible, this.rectVisible, timeStacker)), Menu.MenuRGB(Menu.MenuColors.White), this.White(timeStacker));
                float alpha = Mathf.Lerp(this.lastRectVisible, this.rectVisible, timeStacker) * (1f - Mathf.Lerp(this.lastSin, this.sin, timeStacker));
                for (int i = 0; i < this.roundedRect.sprites.Length; i++)
                {
                    this.roundedRect.sprites[i].color = color;
                    this.roundedRect.sprites[i].alpha = alpha;
                    this.roundedRect.sprites[i].isVisible = true;
                }
            }
            public RoundedRect roundedRect;
            private float rectVisible;
            private float lastRectVisible;
            private int configNumber;
        }

        public static void Initiate(Player player, ProcessManager manager, RainWorldGame game)
        {
            if (Instance == null)
            {
                GraffitiSelectorOverlay m = new(manager);
                Instance = new(m, m.pages[0]);

                int num = MultiplayerUnlocks.ItemUnlockList.Count + MultiplayerUnlocks.CreatureUnlockList.Count + 6;
                float num2 = 980f;
                float num3 = 300f;
                SandboxEditorSelector.Width = 18;
                SandboxEditorSelector.Height = 4;
                SandboxEditorSelector.ButtonSize = 50f;
                while (SandboxEditorSelector.Width * SandboxEditorSelector.Height < num || (float)SandboxEditorSelector.Width * SandboxEditorSelector.ButtonSize > num2 || (float)SandboxEditorSelector.Height * SandboxEditorSelector.ButtonSize > num3)
                {
                    if ((float)(SandboxEditorSelector.Height + 1) * SandboxEditorSelector.ButtonSize < num3)
                    {
                        SandboxEditorSelector.Height++;
                    }
                    else
                    {
                        SandboxEditorSelector.Height = 4;
                        SandboxEditorSelector.ButtonSize = (float)((int)(SandboxEditorSelector.ButtonSize * 0.9f));
                    }
                    SandboxEditorSelector.Width = (int)(num2 / SandboxEditorSelector.ButtonSize);
                }
                Instance.size = new Vector2(1000f, 1000f);

                m.pages[0].subObjects.Add(Instance);
            }

            Instance.currentlyVisible = !Instance.currentlyVisible;
        }
    }
}
