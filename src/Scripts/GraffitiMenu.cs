using System;
using System.Collections.Generic;
using ArenaBehaviors;
using RWCustom;
using UnityEngine;

namespace Menu
{
    // Token: 0x02000407 RID: 1031
    public class GraffitiMenu : RectangularMenuObject
    {
        // Token: 0x170006E9 RID: 1769
        // (get) Token: 0x06002A38 RID: 10808 RVA: 0x00331C49 File Offset: 0x0032FE49
        public MultiplayerUnlocks unlocks
        {
            get
            {
                return null;
            }
        }

        // Token: 0x06002A39 RID: 10809 RVA: 0x00331C60 File Offset: 0x0032FE60
        public GraffitiMenu(Menu menu, MenuObject owner) : base(menu, owner, new Vector2(-1000f, -1000f), new Vector2((float)GraffitiMenu.Width, (float)GraffitiMenu.Height) * GraffitiMenu.ButtonSize)
        {
            this.lastPos = new Vector2(-1000f, -1000f);
            this.bkgRect = new RoundedRect(menu, this, new Vector2(-10f, -30f), this.size + new Vector2(20f, 60f), true);
            this.subObjects.Add(this.bkgRect);
            this.infoLabel = new MenuLabel(menu, this, "", new Vector2(this.size.x / 2f - 100f, 0f), new Vector2(200f, 20f), false, null);
            this.subObjects.Add(this.infoLabel);
            this.buttons = new Button[GraffitiMenu.Width, GraffitiMenu.Height];
            int num = 0;
            this.AddButton(new RectButton(menu, this, GraffitiMenu.ActionButton.Action.ClearAll), ref num);
            if (!ModManager.MSC)
            {
                for (int i = 0; i < 2; i++)
                {
                    this.AddButton(null, ref num);
                }
            }
            foreach (MultiplayerUnlocks.SandboxUnlockID unlockID in MultiplayerUnlocks.ItemUnlockList)
            {
                //    if (this.unlocks.SandboxItemUnlocked(unlockID))
                //    {
                this.AddButton(new CreatureOrItemButton(menu, this, MultiplayerUnlocks.SymbolDataForSandboxUnlock(unlockID)), ref num);
                //    }
                //    else
                //    {
                //this.AddButton(new LockedButton(menu, this), ref num);
                //    }
            }
            foreach (MultiplayerUnlocks.SandboxUnlockID unlockID2 in MultiplayerUnlocks.CreatureUnlockList)
            {
                //    if (this.unlocks.SandboxItemUnlocked(unlockID2))
                //    {
                this.AddButton(new CreatureOrItemButton(menu, this, MultiplayerUnlocks.SymbolDataForSandboxUnlock(unlockID2)), ref num);
                //    }
                //    else
                //    {
                //        this.AddButton(new LockedButton(menu, this), ref num);
                //    }
            }
            this.AddButton(new RectButton(menu, this, GraffitiMenu.ActionButton.Action.Play), GraffitiMenu.Width - 1, 0);
            this.AddButton(new RandomizeButton(menu, this), ModManager.MSC ? (GraffitiMenu.Width - 5) : (GraffitiMenu.Width - 6), 0);
            this.AddButton(new ConfigButton(menu, this, GraffitiMenu.ActionButton.Action.ConfigA, 0), ModManager.MSC ? (GraffitiMenu.Width - 4) : (GraffitiMenu.Width - 5), 0);
            this.AddButton(new ConfigButton(menu, this, GraffitiMenu.ActionButton.Action.ConfigB, 1), ModManager.MSC ? (GraffitiMenu.Width - 3) : (GraffitiMenu.Width - 4), 0);
            this.AddButton(new ConfigButton(menu, this, GraffitiMenu.ActionButton.Action.ConfigC, 2), ModManager.MSC ? (GraffitiMenu.Width - 2) : (GraffitiMenu.Width - 3), 0);
            for (int j = 0; j < GraffitiMenu.Width; j++)
            {
                for (int k = 0; k < GraffitiMenu.Height; k++)
                {
                    if (this.buttons[j, k] != null)
                    {
                        this.buttons[j, k].Initiate(new IntVector2(j, k));
                    }
                }
            }
            this.cursors = new List<ButtonCursor>();
        }

        // Token: 0x06002A3B RID: 10811 RVA: 0x00332014 File Offset: 0x00330214
        private void AddButton(Button button, ref int counter)
        {
            int num = counter / this.buttons.GetLength(0);
            int x = counter - num * this.buttons.GetLength(0);
            this.AddButton(button, x, this.buttons.GetLength(1) - 1 - num);
            counter++;
        }

        // Token: 0x06002A3C RID: 10812 RVA: 0x00332064 File Offset: 0x00330264
        private void AddButton(Button button, int x, int y)
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

        // Token: 0x06002A3D RID: 10813 RVA: 0x003320C4 File Offset: 0x003302C4
        public void ActionButtonClicked(ActionButton actionButton)
        {
            if (actionButton.action == GraffitiMenu.ActionButton.Action.ClearAll)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Clear_All);
                return;
            }
            if (actionButton.action == GraffitiMenu.ActionButton.Action.Play)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Play);
                return;
            }
            if (actionButton.action == GraffitiMenu.ActionButton.Action.ConfigA)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Switch_Config);
                return;
            }
            if (actionButton.action == GraffitiMenu.ActionButton.Action.ConfigB)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Switch_Config);
                return;
            }
            if (actionButton.action == GraffitiMenu.ActionButton.Action.ConfigC)
            {
                this.menu.PlaySound(SoundID.SANDBOX_Switch_Config);
                return;
            }
            if (actionButton.action == GraffitiMenu.ActionButton.Action.Randomize)
            {
                (actionButton as RandomizeButton).Active = !(actionButton as RandomizeButton).Active;
                this.menu.PlaySound((actionButton as RandomizeButton).Active ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
                this.UpdateInfoLabel(actionButton.intPos.x, actionButton.intPos.y);
            }
        }

        // Token: 0x06002A3E RID: 10814 RVA: 0x00332230 File Offset: 0x00330430
        public void UpdateInfoLabel(int x, int y)
        {
            this.infoLabel.text = ((this.buttons[x, y] != null) ? this.buttons[x, y].DescriptorText : "");
        }

        // Token: 0x06002A3F RID: 10815 RVA: 0x00332297 File Offset: 0x00330497
        public void MouseCursorEnterMenuMode()
        {
            this.mouseModeClickedDownLow = true;
        }

        // Token: 0x06002A40 RID: 10816 RVA: 0x003322C0 File Offset: 0x003304C0
        public override void Update()
        {
            this.lastVisFac = this.visFac;
            this.counter++;
            base.Update();
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            if (this.mouseModeClickedDownLow)
            {
                flag2 = true;
                if (!flag4)
                {
                    this.mouseModeClickedDownLow = false;
                }
            }
            //if (this.editor.gameSession.game.pauseMenu != null)
            //{
            //    flag = false;
            //}
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

        // Token: 0x06002A41 RID: 10817 RVA: 0x00332664 File Offset: 0x00330864
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
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

        // Token: 0x0400268F RID: 9871
        private RoundedRect bkgRect;

        // Token: 0x04002692 RID: 9874
        public bool currentlyVisible;

        // Token: 0x04002693 RID: 9875
        public int lingerCounter;

        // Token: 0x04002694 RID: 9876
        public float visFac;

        // Token: 0x04002695 RID: 9877
        public float lastVisFac;

        // Token: 0x04002696 RID: 9878
        public Button[,] buttons;

        // Token: 0x04002697 RID: 9879
        public static float ButtonSize = 50f;

        // Token: 0x04002698 RID: 9880
        public static int Width = 19;

        // Token: 0x04002699 RID: 9881
        public static int Height = 4;

        // Token: 0x0400269A RID: 9882
        public bool upTop;

        // Token: 0x0400269B RID: 9883
        public bool mouseModeClickedDownLow;

        // Token: 0x0400269C RID: 9884
        public MenuLabel infoLabel;

        // Token: 0x0400269D RID: 9885
        public List<ButtonCursor> cursors;

        // Token: 0x0400269E RID: 9886
        private int counter;

        // Token: 0x020008A8 RID: 2216
        public class ButtonCursor : RectangularMenuObject
        {
            // Token: 0x17000A31 RID: 2609
            // (get) Token: 0x060041C5 RID: 16837 RVA: 0x00492FE2 File Offset: 0x004911E2
            public GraffitiMenu EditorSelector
            {
                get
                {
                    return this.owner as GraffitiMenu;
                }
            }

            // Token: 0x060041C6 RID: 16838 RVA: 0x00492FF0 File Offset: 0x004911F0
            public ButtonCursor(Menu menu, MenuObject owner, IntVector2 intPos, SandboxEditor.EditCursor roomCursor) : base(menu, owner, intPos.ToVector2() * GraffitiMenu.ButtonSize, new Vector2(GraffitiMenu.ButtonSize, GraffitiMenu.ButtonSize))
            {
                this.intPos = intPos;
                this.roundedRect = new RoundedRect(menu, this, new Vector2(-2f, -2f), this.size + new Vector2(4f, 4f), false);
                this.subObjects.Add(this.roundedRect);
                this.extraRect = new RoundedRect(menu, this, new Vector2(-2f, -2f), this.size + new Vector2(4f, 4f), false);
                this.subObjects.Add(this.extraRect);
                this.selectedButton = this.EditorSelector.buttons[intPos.x, intPos.y];
            }

            // Token: 0x060041C7 RID: 16839 RVA: 0x004930F0 File Offset: 0x004912F0
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
                    this.intPos.x = GraffitiMenu.Width - 1;
                }
                else if (this.intPos.x >= GraffitiMenu.Width)
                {
                    this.intPos.x = 0;
                }
                this.intPos.y = this.intPos.y + yAdd;
                if (this.intPos.y < 0)
                {
                    this.intPos.y = GraffitiMenu.Height - 1;
                }
                else if (this.intPos.y >= GraffitiMenu.Height)
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

            // Token: 0x060041C8 RID: 16840 RVA: 0x00493298 File Offset: 0x00491498
            public void Click()
            {
                this.clickOnRelease = false;
                if (this.intPos.x < 0 || this.intPos.x >= GraffitiMenu.Width || this.intPos.y < 0 || this.intPos.y >= GraffitiMenu.Height || this.EditorSelector.buttons[this.intPos.x, this.intPos.y] == null)
                {
                    this.menu.PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
                    return;
                }
                this.EditorSelector.buttons[this.intPos.x, this.intPos.y].Clicked(this);
            }

            // Token: 0x060041C9 RID: 16841 RVA: 0x00493354 File Offset: 0x00491554
            public override void Update()
            {
                base.Update();
                this.lastActive = this.active;
                this.lastSin = this.sin;
                this.lastInnerVisible = this.innerVisible;
                this.active = 1f;
                Vector2 vector = this.intPos.ToVector2() * GraffitiMenu.ButtonSize;
                this.pos = Vector2.Lerp(Custom.MoveTowards(this.pos, vector, 5f), vector, 0.4f);
                float t = ((Custom.DistLess(this.pos, vector, 10f) /*&& !this.roomCursor.input[0].thrw*/) ? 1f : 0f) * this.active;
                this.roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, 0f, t);
                this.extraRect.addSize = new Vector2(1f, 1f) * -8f;
                this.stillCounter++;
                this.sin += this.innerVisible;
                this.innerVisible = Custom.LerpAndTick(this.innerVisible, Mathf.InverseLerp(10f, 30f, (float)this.stillCounter), 0.06f, 0.04f);
            }

            // Token: 0x060041CA RID: 16842 RVA: 0x004936F0 File Offset: 0x004918F0
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                float num = Custom.SCurve(Mathf.Lerp(this.lastActive, this.active, timeStacker), 0.65f);
                Color color = Color.white;
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

            // Token: 0x060041CB RID: 16843 RVA: 0x0049388A File Offset: 0x00491A8A
            public override void RemoveSprites()
            {
                base.RemoveSprites();
            }

            // Token: 0x040044D1 RID: 17617
            public IntVector2 intPos;

            // Token: 0x040044D2 RID: 17618
            private RoundedRect roundedRect;

            // Token: 0x040044D3 RID: 17619
            private RoundedRect extraRect;

            // Token: 0x040044D5 RID: 17621
            public Button selectedButton;

            // Token: 0x040044D6 RID: 17622
            private float active;

            // Token: 0x040044D7 RID: 17623
            private float lastActive;

            // Token: 0x040044D8 RID: 17624
            private float sin;

            // Token: 0x040044D9 RID: 17625
            private float lastSin;

            // Token: 0x040044DA RID: 17626
            private float innerVisible;

            // Token: 0x040044DB RID: 17627
            private float lastInnerVisible;

            // Token: 0x040044DC RID: 17628
            private int stillCounter;

            // Token: 0x040044DD RID: 17629
            public bool clickOnRelease;
        }

        // Token: 0x020008A9 RID: 2217
        public class Button : RectangularMenuObject
        {
            // Token: 0x17000A32 RID: 2610
            // (get) Token: 0x060041CC RID: 16844 RVA: 0x00493892 File Offset: 0x00491A92
            public GraffitiMenu EditorSelector
            {
                get
                {
                    return this.owner as GraffitiMenu;
                }
            }

            // Token: 0x17000A33 RID: 2611
            // (get) Token: 0x060041CD RID: 16845 RVA: 0x0049389F File Offset: 0x00491A9F
            public virtual string DescriptorText
            {
                get
                {
                    return "";
                }
            }

            // Token: 0x17000A34 RID: 2612
            // (get) Token: 0x060041CE RID: 16846 RVA: 0x004938A8 File Offset: 0x00491AA8
            public override bool Selected
            {
                get
                {
                    for (int i = 0; i < this.EditorSelector.cursors.Count; i++)
                    {
                        if (this.EditorSelector.cursors[i].selectedButton == this)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            // Token: 0x060041CF RID: 16847 RVA: 0x0049390C File Offset: 0x00491B0C
            public virtual float White(float timeStacker)
            {
                return Mathf.Lerp(this.lastSin, this.sin, timeStacker) * (0.5f + 0.5f * Mathf.Sin(((float)this.counter + timeStacker) / 30f * 3.1415927f * 2f));
            }

            // Token: 0x060041D0 RID: 16848 RVA: 0x00493958 File Offset: 0x00491B58
            public Button(Menu menu, MenuObject owner) : base(menu, owner, default(Vector2), new Vector2(GraffitiMenu.ButtonSize, GraffitiMenu.ButtonSize))
            {
            }

            // Token: 0x060041D1 RID: 16849 RVA: 0x00493988 File Offset: 0x00491B88
            public virtual void Initiate(IntVector2 intPos)
            {
                this.intPos = intPos;
                this.pos = intPos.ToVector2() * GraffitiMenu.ButtonSize;
                this.lastPos = this.pos;
                if (intPos.x < GraffitiMenu.Width - 1 && this.EditorSelector.buttons[intPos.x + 1, intPos.y] != null)
                {
                    this.rightDivider = new FSprite("pixel", true);
                    this.rightDivider.scaleX = 2f;
                    this.rightDivider.scaleY = GraffitiMenu.ButtonSize - 15f;
                    this.rightDivider.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
                    this.Container.AddChild(this.rightDivider);
                }
                if (intPos.y < GraffitiMenu.Height - 1 && this.EditorSelector.buttons[intPos.x, intPos.y + 1] != null)
                {
                    this.upDivider = new FSprite("pixel", true);
                    this.upDivider.scaleX = GraffitiMenu.ButtonSize - 15f;
                    this.upDivider.scaleY = 2f;
                    this.upDivider.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
                    this.Container.AddChild(this.upDivider);
                }
            }

            // Token: 0x060041D2 RID: 16850 RVA: 0x00493AD7 File Offset: 0x00491CD7
            public virtual void Flash()
            {
            }

            // Token: 0x060041D3 RID: 16851 RVA: 0x00493AD9 File Offset: 0x00491CD9
            public virtual void Clicked(ButtonCursor cursor)
            {
            }

            // Token: 0x060041D4 RID: 16852 RVA: 0x00493ADC File Offset: 0x00491CDC
            public override void Update()
            {
                base.Update();
                this.lastSin = this.sin;
                this.counter++;
                this.sin = Custom.LerpAndTick(this.sin, this.Selected ? 1f : 0f, 0.03f, 0.033333335f);
            }

            // Token: 0x060041D5 RID: 16853 RVA: 0x00493B38 File Offset: 0x00491D38
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

            // Token: 0x060041D6 RID: 16854 RVA: 0x00493BE4 File Offset: 0x00491DE4
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

            // Token: 0x040044DF RID: 17631
            public IntVector2 intPos;

            // Token: 0x040044E0 RID: 17632
            public FSprite rightDivider;

            // Token: 0x040044E1 RID: 17633
            public FSprite upDivider;

            // Token: 0x040044E2 RID: 17634
            protected int counter;

            // Token: 0x040044E3 RID: 17635
            public float lastSin;

            // Token: 0x040044E4 RID: 17636
            public float sin;
        }

        // Token: 0x020008AA RID: 2218
        public class CreatureOrItemButton : Button
        {
            // Token: 0x17000A35 RID: 2613
            // (get) Token: 0x060041D7 RID: 16855 RVA: 0x00493C12 File Offset: 0x00491E12
            public IconSymbol.IconSymbolData data
            {
                get
                {
                    return this.symbol.iconData;
                }
            }

            // Token: 0x17000A36 RID: 2614
            // (get) Token: 0x060041D8 RID: 16856 RVA: 0x00493C1F File Offset: 0x00491E1F
            public override string DescriptorText
            {
                get
                {
                    return this.menu.Translate((this.data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature) ? "Add creature" : "Add item");
                }
            }

            // Token: 0x060041D9 RID: 16857 RVA: 0x00493C4F File Offset: 0x00491E4F
            public CreatureOrItemButton(Menu menu, MenuObject owner, IconSymbol.IconSymbolData data) : base(menu, owner)
            {
                this.symbol = IconSymbol.CreateIconSymbol(data, this.Container);
                this.symbol.Show(true);
            }

            // Token: 0x060041DA RID: 16858 RVA: 0x00493C77 File Offset: 0x00491E77
            public override void Flash()
            {
                base.Flash();
                this.symbol.showFlash = 1f;
            }

            // Token: 0x060041DB RID: 16859 RVA: 0x00493C8F File Offset: 0x00491E8F
            public override void Clicked(ButtonCursor cursor)
            {
                base.Clicked(cursor);
                // TODO: Select graffiti
            }

            // Token: 0x060041DC RID: 16860 RVA: 0x00493CBE File Offset: 0x00491EBE
            public override void Update()
            {
                base.Update();
                this.symbol.Update();
                this.symbol.showFlash = Mathf.Max(this.symbol.showFlash, this.White(1f));
            }

            // Token: 0x060041DD RID: 16861 RVA: 0x00493CF7 File Offset: 0x00491EF7
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                this.symbol.Draw(timeStacker, this.DrawPos(timeStacker) + base.DrawSize(timeStacker) / 2f);
            }

            // Token: 0x060041DE RID: 16862 RVA: 0x00493D29 File Offset: 0x00491F29
            public override void RemoveSprites()
            {
                base.RemoveSprites();
                this.symbol.RemoveSprites();
            }

            // Token: 0x040044E5 RID: 17637
            public IconSymbol symbol;
        }

        // Token: 0x020008AB RID: 2219
        public abstract class ActionButton : Button
        {
            // Token: 0x17000A37 RID: 2615
            // (get) Token: 0x060041DF RID: 16863 RVA: 0x00493D3C File Offset: 0x00491F3C
            public override string DescriptorText
            {
                get
                {
                    return this.descriptorText;
                }
            }

            // Token: 0x060041E0 RID: 16864 RVA: 0x00493D44 File Offset: 0x00491F44
            public override float White(float timeStacker)
            {
                return Mathf.Max(Mathf.Lerp(this.lastBump, this.bump, timeStacker), base.White(timeStacker));
            }

            // Token: 0x060041E1 RID: 16865 RVA: 0x00493D64 File Offset: 0x00491F64
            public virtual Color MyColor(float timeStacker)
            {
                if (this.action == GraffitiMenu.ActionButton.Action.Play)
                {
                    return Color.Lerp(this.symbolSpriteColor, Color.red, 0.5f + 0.5f * Mathf.Sin(((float)this.counter + timeStacker) / 30f * 3.1415927f * 2f));
                }
                return Color.Lerp(this.symbolSpriteColor, Color.white, this.White(timeStacker));
            }

            // Token: 0x060041E2 RID: 16866 RVA: 0x00493DEA File Offset: 0x00491FEA
            public override void Flash()
            {
                base.Flash();
                this.bump = 1f;
            }

            // Token: 0x060041E3 RID: 16867 RVA: 0x00493E00 File Offset: 0x00492000
            public ActionButton(Menu menu, MenuObject owner, Action action) : base(menu, owner)
            {
                this.action = action;
                this.descriptorText = "";
                this.symbolSpriteName = "Futile_White";
                this.symbolSpriteColor = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
                if (action == GraffitiMenu.ActionButton.Action.ClearAll)
                {
                    this.symbolSpriteName = "Sandbox_ClearAll";
                    this.descriptorText = menu.Translate("Clear all");
                }
                else if (action == GraffitiMenu.ActionButton.Action.Play)
                {
                    this.symbolSpriteName = "Sandbox_Play";
                    this.descriptorText = menu.Translate("Play!");
                }
                else if (action == GraffitiMenu.ActionButton.Action.ConfigA)
                {
                    this.symbolSpriteName = "Sandbox_A";
                    this.descriptorText = menu.Translate("Configuration") + " A";
                }
                else if (action == GraffitiMenu.ActionButton.Action.ConfigB)
                {
                    this.symbolSpriteName = "Sandbox_B";
                    this.descriptorText = menu.Translate("Configuration") + " B";
                }
                else if (action == GraffitiMenu.ActionButton.Action.ConfigC)
                {
                    this.symbolSpriteName = "Sandbox_C";
                    this.descriptorText = menu.Translate("Configuration") + " C";
                }
                else if (action == GraffitiMenu.ActionButton.Action.Locked)
                {
                    this.symbolSpriteName = "Sandbox_QuestionMark";
                }
                else if (action == GraffitiMenu.ActionButton.Action.Randomize)
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

            // Token: 0x060041E4 RID: 16868 RVA: 0x0049400C File Offset: 0x0049220C
            public override void Clicked(ButtonCursor cursor)
            {
                base.Clicked(cursor);
                base.EditorSelector.ActionButtonClicked(this);
                this.bump = 1f;
            }

            // Token: 0x060041E5 RID: 16869 RVA: 0x0049404A File Offset: 0x0049224A
            public override void Update()
            {
                this.lastBump = this.bump;
                this.bump = Mathf.Max(0f, this.bump - 0.05f);
                base.Update();
            }

            // Token: 0x060041E6 RID: 16870 RVA: 0x0049407C File Offset: 0x0049227C
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

            // Token: 0x060041E7 RID: 16871 RVA: 0x0049413E File Offset: 0x0049233E
            public override void RemoveSprites()
            {
                this.shadow1.RemoveFromContainer();
                this.shadow2.RemoveFromContainer();
                this.symbolSprite.RemoveFromContainer();
                base.RemoveSprites();
            }

            // Token: 0x040044E6 RID: 17638
            public Action action;

            // Token: 0x040044E7 RID: 17639
            public string symbolSpriteName;

            // Token: 0x040044E8 RID: 17640
            public string descriptorText;

            // Token: 0x040044E9 RID: 17641
            public Color symbolSpriteColor;

            // Token: 0x040044EA RID: 17642
            public FSprite symbolSprite;

            // Token: 0x040044EB RID: 17643
            public FSprite shadow1;

            // Token: 0x040044EC RID: 17644
            public FSprite shadow2;

            // Token: 0x040044ED RID: 17645
            public float bump;

            // Token: 0x040044EE RID: 17646
            public float lastBump;

            // Token: 0x020009C7 RID: 2503
            public class Action : ExtEnum<Action>
            {
                // Token: 0x06004545 RID: 17733 RVA: 0x004B504F File Offset: 0x004B324F
                public Action(string value, bool register = false) : base(value, register)
                {
                }

                // Token: 0x04004AE3 RID: 19171
                public static readonly Action ClearAll = new Action("ClearAll", true);

                // Token: 0x04004AE4 RID: 19172
                public static readonly Action Play = new Action("Play", true);

                // Token: 0x04004AE5 RID: 19173
                public static readonly Action Randomize = new Action("Randomize", true);

                // Token: 0x04004AE6 RID: 19174
                public static readonly Action ConfigA = new Action("ConfigA", true);

                // Token: 0x04004AE7 RID: 19175
                public static readonly Action ConfigB = new Action("ConfigB", true);

                // Token: 0x04004AE8 RID: 19176
                public static readonly Action ConfigC = new Action("ConfigC", true);

                // Token: 0x04004AE9 RID: 19177
                public static readonly Action Locked = new Action("Locked", true);
            }
        }

        // Token: 0x020008AC RID: 2220
        public class LockedButton : ActionButton
        {
            // Token: 0x060041E8 RID: 16872 RVA: 0x00494167 File Offset: 0x00492367
            public override Color MyColor(float timeStacker)
            {
                return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(this.lastBump, this.bump, timeStacker));
            }

            // Token: 0x060041E9 RID: 16873 RVA: 0x00494194 File Offset: 0x00492394
            public LockedButton(Menu menu, MenuObject owner) : base(menu, owner, GraffitiMenu.ActionButton.Action.Locked)
            {
            }

            // Token: 0x060041EA RID: 16874 RVA: 0x004941A3 File Offset: 0x004923A3
            public override void Update()
            {
                base.Update();
            }

            // Token: 0x060041EB RID: 16875 RVA: 0x004941AB File Offset: 0x004923AB
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
            }
        }

        // Token: 0x020008AD RID: 2221
        public class RandomizeButton : ActionButton
        {
            // Token: 0x17000A38 RID: 2616
            // (get) Token: 0x060041EC RID: 16876 RVA: 0x004941B4 File Offset: 0x004923B4
            // (set) Token: 0x060041ED RID: 16877 RVA: 0x004941D3 File Offset: 0x004923D3
            public bool Active
            {
                get
                {
                    return true;
                }
                set
                {
                    //base.EditorSelector.editor.gameSession.GameTypeSetup.saveCreatures = !value;
                }
            }

            // Token: 0x17000A39 RID: 2617
            // (get) Token: 0x060041EE RID: 16878 RVA: 0x004941F3 File Offset: 0x004923F3
            public override string DescriptorText
            {
                get
                {
                    return this.menu.Translate(this.Active ? "Random creatures" : "Persistent creatures");
                }
            }

            // Token: 0x060041EF RID: 16879 RVA: 0x00494214 File Offset: 0x00492414
            public override Color MyColor(float timeStacker)
            {
                if (this.Active)
                {
                    return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(this.lastSin, this.sin, timeStacker));
                }
                return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(this.lastBump, this.bump, timeStacker));
            }

            // Token: 0x060041F0 RID: 16880 RVA: 0x00494280 File Offset: 0x00492480
            public RandomizeButton(Menu menu, MenuObject owner) : base(menu, owner, GraffitiMenu.ActionButton.Action.Randomize)
            {
            }

            // Token: 0x060041F1 RID: 16881 RVA: 0x0049428F File Offset: 0x0049248F
            public override void Update()
            {
                base.Update();
            }

            // Token: 0x060041F2 RID: 16882 RVA: 0x00494297 File Offset: 0x00492497
            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
            }
        }

        // Token: 0x020008AE RID: 2222
        public class RectButton : ActionButton
        {
            // Token: 0x060041F3 RID: 16883 RVA: 0x004942A0 File Offset: 0x004924A0
            public RectButton(Menu menu, MenuObject owner, Action action) : base(menu, owner, action)
            {
                this.roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), this.size, false);
                this.subObjects.Add(this.roundedRect);
            }

            // Token: 0x060041F4 RID: 16884 RVA: 0x004942DF File Offset: 0x004924DF
            public override void Update()
            {
                base.Update();
                this.roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, -2f, this.bump);
            }

            // Token: 0x060041F5 RID: 16885 RVA: 0x0049431C File Offset: 0x0049251C
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

            // Token: 0x040044EF RID: 17647
            public RoundedRect roundedRect;
        }

        // Token: 0x020008AF RID: 2223
        public class ConfigButton : ActionButton
        {
            // Token: 0x060041F6 RID: 16886 RVA: 0x004943BC File Offset: 0x004925BC
            public ConfigButton(Menu menu, MenuObject owner, Action action, int configNumber) : base(menu, owner, action)
            {
                this.configNumber = configNumber;
                this.roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), this.size, false);
                this.subObjects.Add(this.roundedRect);
            }

            // Token: 0x060041F7 RID: 16887 RVA: 0x00494410 File Offset: 0x00492610
            public override void Update()
            {
                base.Update();
                this.lastRectVisible = this.rectVisible;
                this.rectVisible = 1f;
                this.roundedRect.addSize = new Vector2(1f, 1f) * Mathf.Lerp(-8f, -2f, this.bump);
            }

            // Token: 0x060041F8 RID: 16888 RVA: 0x004944A4 File Offset: 0x004926A4
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

            // Token: 0x040044F0 RID: 17648
            public RoundedRect roundedRect;

            // Token: 0x040044F1 RID: 17649
            private float rectVisible;

            // Token: 0x040044F2 RID: 17650
            private float lastRectVisible;

            // Token: 0x040044F3 RID: 17651
            private int configNumber;
        }
    }
}
