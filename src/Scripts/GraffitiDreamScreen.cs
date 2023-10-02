using System;
using System.Collections.Generic;
using System.Globalization;
using JollyCoop.JollyManual;
using Menu;
using Menu.Remix;
using RWCustom;
using UnityEngine;
using JollyCoop;
namespace Vinki
{
    // Token: 0x02000361 RID: 865
    public class GraffitiDialog : Dialog
    {
        // Token: 0x17000668 RID: 1640
        // (get) Token: 0x060025DE RID: 9694 RVA: 0x002EC943 File Offset: 0x002EAB43
        public Options Options
        {
            get
            {
                return this.manager.rainWorld.options;
            }
        }

        // Token: 0x060025DF RID: 9695 RVA: 0x002EC955 File Offset: 0x002EAB55
        public JollyCoop.JollyMenu.JollyPlayerOptions JollyOptions(int index)
        {
            return this.Options.jollyPlayerOptionsArray[index];
        }

        // Token: 0x060025E0 RID: 9696 RVA: 0x002EC964 File Offset: 0x002EAB64
        public GraffitiDialog(SlugcatStats.Name name, ProcessManager manager, Vector2 closeButtonPos) : base(manager)
        {
            this.manager = manager;
            this.currentSlugcatPageName = name;
            this.targetAlpha = 1f;
            this.closing = false;
            this.opening = true;
            this.requestingInputMenu = false;
            this.elementDescription = new Dictionary<string, string>();
            this.oi = MachineConnector.GetRegisteredOI(JollyCoop.JollyCoop.MOD_ID);
            this.AddCancelButton(closeButtonPos);
            Vector2 pos = new Vector2(0f, this.Options.ScreenSize.y + 100f);
            //this.slidingMenu = new JollyCoop.JollyMenu.JollySlidingMenu(this, this.pages[0], pos);
            //this.pages[0].subObjects.Add(this.slidingMenu);
            this.manualTopics = new Dictionary<JollyEnums.JollyManualPages, int>
            {
                {
                    JollyEnums.JollyManualPages.Introduction,
                    1
                },
                {
                    JollyEnums.JollyManualPages.Difficulties,
                    1
                },
                {
                    JollyEnums.JollyManualPages.Surviving_a_cycle,
                    1
                },
                {
                    JollyEnums.JollyManualPages.Camera,
                    2
                },
                {
                    JollyEnums.JollyManualPages.Piggybacking,
                    1
                },
                {
                    JollyEnums.JollyManualPages.Pointing,
                    1
                },
                {
                    JollyEnums.JollyManualPages.Selecting_a_slugcat,
                    1
                }
            };
            JollyCustom.Log("Opening jolly dialog!!!", false);
        }

        // Token: 0x060025E1 RID: 9697 RVA: 0x002ECA8C File Offset: 0x002EAC8C
        private void AddCancelButton(Vector2 pos)
        {
            this.cancelButton = new SimpleButton(this, this.pages[0], base.Translate("CLOSE"), "CANCEL", pos, new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.cancelButton);
        }

        // Token: 0x060025E2 RID: 9698 RVA: 0x002ECAED File Offset: 0x002EACED
        private void PlayDialogCloseSound()
        {
            base.PlaySound(SoundID.MENU_Remove_Level);
        }

        // Token: 0x060025E3 RID: 9699 RVA: 0x002ECAFC File Offset: 0x002EACFC
        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if (message.StartsWith("JOLLYCOLORDIALOG"))
            {
                int num = int.Parse(message.Split(new char[]
                {
                    'G'
                })[1], NumberFormatInfo.InvariantInfo);
                //if (num < this.slidingMenu.playerSelector.Length)
                //{
                //    JollyCustom.Log("Changing color for player " + num.ToString(), false);
                //    SlugcatStats.Name name = this.Options.jollyPlayerOptionsArray[num].playerClass ?? this.currentSlugcatPageName;
                //    List<string> names = PlayerGraphics.ColoredBodyPartList(name);
                //    this.colorDialog = new JollyCoop.JollyMenu.ColorChangeDialog(this, name, num, this.manager, names);
                //    this.manager.ShowDialog(this.colorDialog);
                //}
                return;
            }
            if (message != null)
            {
                if (message == "CANCEL")
                {
                    base.PlaySound(SoundID.MENU_Switch_Page_Out);
                    this.RequestClose();
                    return;
                }
                if (message == "INFO_COLOR")
                {
                    string text = base.Translate(message);
                    Vector2 vector = DialogBoxNotify.CalculateDialogBoxSize(text, true);
                    this.instructionsDialog = new DialogNotify(Custom.ReplaceLineDelimeters(text), base.Translate("COLOR INFORMATION"), new Vector2(vector.x, vector.y + 80f), this.manager, new Action(this.PlayDialogCloseSound), true);
                    this.manager.ShowDialog(this.instructionsDialog);
                    return;
                }
                if (message == "INFO_DIFF")
                {
                    string text2 = base.Translate(message);
                    Vector2 vector2 = DialogBoxNotify.CalculateDialogBoxSize(text2, true);
                    this.instructionsDialog = new DialogNotify(Custom.ReplaceLineDelimeters(text2), base.Translate("DIFFICULTY INFORMATION"), new Vector2(vector2.x, vector2.y + 80f), this.manager, new Action(this.PlayDialogCloseSound), true);
                    this.manager.ShowDialog(this.instructionsDialog);
                    return;
                }
                if (message == "INFO_CAMERA")
                {
                    string text3 = base.Translate(message);
                    Vector2 vector3 = DialogBoxNotify.CalculateDialogBoxSize(text3, true);
                    this.instructionsDialog = new DialogNotify(Custom.ReplaceLineDelimeters(text3), base.Translate("CAMERA INFORMATION"), new Vector2(vector3.x, vector3.y + 80f), this.manager, new Action(this.PlayDialogCloseSound), true);
                    this.manager.ShowDialog(this.instructionsDialog);
                    return;
                }
                if (message == "INPUT")
                {
                    this.RequestInputMenu();
                    return;
                }
                if (!(message == "JOLLY_MANUAL"))
                {
                    return;
                }
                JollyManualDialog dialog = new JollyManualDialog(this.manager, this.manualTopics);
                base.PlaySound(SoundID.MENU_Player_Join_Game);
                this.manager.ShowDialog(dialog);
            }
        }

        // Token: 0x060025E4 RID: 9700 RVA: 0x002ECD9D File Offset: 0x002EAF9D
        private void RequestInputMenu()
        {
            this.requestingInputMenu = true;
            this.RequestClose();
        }

        // Token: 0x060025E5 RID: 9701 RVA: 0x002ECDAC File Offset: 0x002EAFAC
        public override void Update()
        {
            base.Update();
            this.lastAlpha = this.currentAlpha;
            this.currentAlpha = Mathf.Lerp(this.currentAlpha, this.targetAlpha, 0.2f);
            bool flag = RWInput.CheckPauseButton(0, this.manager.rainWorld);
            if (flag && !this.lastPauseButton)
            {
                base.PlaySound(SoundID.MENU_Switch_Page_Out);
                this.RequestClose();
            }
            this.lastPauseButton = flag;
            if (this.closing && Math.Abs(this.currentAlpha - this.targetAlpha) < 0.09f)
            {
                this.Options.Save();
                this.manager.StopSideProcess(this);
                this.closing = false;
                if (this.requestingInputMenu)
                {
                    this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.InputOptions);
                    base.PlaySound(SoundID.MENU_Switch_Page_In);
                    this.manager.rainWorld.options.Save();
                }
            }
        }

        // Token: 0x060025E6 RID: 9702 RVA: 0x002ECE93 File Offset: 0x002EB093
        public void RequestClose()
        {
            if (this.closing)
            {
                return;
            }
            this.closing = true;
            this.targetAlpha = 0f;
        }

        // Token: 0x060025E7 RID: 9703 RVA: 0x002ECEB0 File Offset: 0x002EB0B0
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (this.opening || this.closing)
            {
                this.uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(this.lastAlpha, this.currentAlpha, timeStacker)), 1.5f);
                this.darkSprite.alpha = this.uAlpha * 0.92f;
            }
            //this.slidingMenu.pos.y = Mathf.Lerp(this.Options.ScreenSize.y + 100f, 0f, (this.uAlpha < 0.999f) ? this.uAlpha : 1f);
        }

        // Token: 0x060025E8 RID: 9704 RVA: 0x002ECF61 File Offset: 0x002EB161
        public override void ShutDownProcess()
        {
            base.ShutDownProcess();
            this.darkSprite.RemoveFromContainer();
            //this.slidingMenu.RemoveSprites();
        }

        // Token: 0x060025E9 RID: 9705 RVA: 0x002ECF80 File Offset: 0x002EB180
        public int GetFileIndex(SlugcatStats.Name name)
        {
            int result = 4;
            if (name == SlugcatStats.Name.White)
            {
                result = 0;
            }
            if (name == SlugcatStats.Name.Yellow)
            {
                result = 1;
            }
            if (name == SlugcatStats.Name.Red)
            {
                result = 2;
            }
            return result;
        }

        // Token: 0x060025EA RID: 9706 RVA: 0x002ECFC0 File Offset: 0x002EB1C0
        public override string UpdateInfoText()
        {
            string text = null;
            SimpleButton simpleButton = this.selectedObject as SimpleButton;
            if (simpleButton != null)
            {
                text = simpleButton.signalText;
            }
            SymbolButton symbolButton = this.selectedObject as SymbolButton;
            if (symbolButton != null)
            {
                text = symbolButton.signalText;
            }
            string result;
            if (text != null && this.elementDescription.TryGetValue(text, out result))
            {
                return result;
            }
            return base.UpdateInfoText();
        }

        // Token: 0x040022D1 RID: 8913
        public SimpleButton cancelButton;

        // Token: 0x040022D2 RID: 8914
        public OptionInterface oi;

        // Token: 0x040022D3 RID: 8915
        public DialogNotify instructionsDialog;

        // Token: 0x040022D4 RID: 8916
        public JollyCoop.JollyMenu.ColorChangeDialog colorDialog;

        // Token: 0x040022D5 RID: 8917
        private bool lastPauseButton;

        // Token: 0x040022D6 RID: 8918
        private float targetAlpha;

        // Token: 0x040022D7 RID: 8919
        private float currentAlpha;

        // Token: 0x040022D8 RID: 8920
        private float lastAlpha;

        // Token: 0x040022D9 RID: 8921
        public float uAlpha;

        // Token: 0x040022DA RID: 8922
        private bool closing;

        // Token: 0x040022DB RID: 8923
        private bool opening;

        // Token: 0x040022DC RID: 8924
        //public JollyCoop.JollyMenu.JollySlidingMenu slidingMenu;

        // Token: 0x040022DD RID: 8925
        public SlugcatStats.Name currentSlugcatPageName;

        // Token: 0x040022DE RID: 8926
        public MenuTabWrapper tabWrapper;

        // Token: 0x040022DF RID: 8927
        public Dictionary<string, string> elementDescription;

        // Token: 0x040022E0 RID: 8928
        public Dictionary<JollyEnums.JollyManualPages, int> manualTopics;

        // Token: 0x040022E1 RID: 8929
        private bool requestingInputMenu;
    }
}
