﻿using System;
using System.Collections.Generic;
using System.Linq;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace Menu
{
    // Token: 0x0200041C RID: 1052
    public class GraffitiDialog : Dialog
    {
        // Token: 0x06002A9C RID: 10908 RVA: 0x0033BE9C File Offset: 0x0033A09C
        public GraffitiDialog(ProcessManager manager, Vector2 cancelButtonPos) : base(manager)
        {
            float[] screenOffsets = Custom.GetScreenOffsets();
            this.leftAnchor = screenOffsets[0];
            this.rightAnchor = screenOffsets[1];
            this.pages[0].pos = new Vector2(0.01f, 0f);
            Page page = this.pages[0];
            page.pos.y = page.pos.y + 2000f;
            this.title = new MenuIllustration(this, this.pages[0], "", "graffiti_map", new Vector2(Screen.width/2, Screen.height/2), true, true);
            this.pages[0].subObjects.Add(this.title);
            float cancelButtonWidth = GraffitiDialog.GetCancelButtonWidth(base.CurrLang);
            this.cancelButton = new SimpleButton(this, this.pages[0], base.Translate("CLOSE"), "CLOSE", cancelButtonPos, new Vector2(cancelButtonWidth, 30f));
            this.pages[0].subObjects.Add(this.cancelButton);
            this.opening = true;
            this.targetAlpha = 1f;
        }

        private static float GetCancelButtonWidth(InGameTranslator.LanguageID lang)
        {
            float result = 120f;
            if (lang == InGameTranslator.LanguageID.Japanese || lang == InGameTranslator.LanguageID.French || lang == InGameTranslator.LanguageID.German || lang == InGameTranslator.LanguageID.Russian)
            {
                result = 160f;
            }
            return result;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (this.opening || this.closing)
            {
                this.uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(this.lastAlpha, this.currentAlpha, timeStacker)), 1.5f);
                this.darkSprite.alpha = this.uAlpha * 0.95f;
            }
            this.pages[0].pos.y = Mathf.Lerp(this.manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, (this.uAlpha < 0.999f) ? this.uAlpha : 1f);
            this.title.sprite.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, this.darkSprite.alpha));
        }

        // Token: 0x06002AA4 RID: 10916 RVA: 0x0033CAC0 File Offset: 0x0033ACC0
        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if (message == "CLOSE")
            {
                this.closing = true;
                this.targetAlpha = 0f;
            }
        }

        // Token: 0x06002AA5 RID: 10917 RVA: 0x0033CB90 File Offset: 0x0033AD90
        public override void Update()
        {
            base.Update();
            this.lastAlpha = this.currentAlpha;
            this.currentAlpha = Mathf.Lerp(this.currentAlpha, this.targetAlpha, 0.2f);
            if (this.opening && this.pages[0].pos.y <= 0.01f)
            {
                this.opening = false;
            }
            if (this.closing && Math.Abs(this.currentAlpha - this.targetAlpha) < 0.09f)
            {
                this.manager.StopSideProcess(this);
                this.closing = false;
            }
            this.cancelButton.buttonBehav.greyedOut = this.opening;
        }

        // Token: 0x0400272A RID: 10026
        public MenuIllustration title;

        // Token: 0x0400272B RID: 10027
        public SimpleButton cancelButton;

        // Token: 0x0400272C RID: 10028
        public float leftAnchor;

        // Token: 0x0400272D RID: 10029
        public float rightAnchor;

        // Token: 0x0400272E RID: 10030
        public bool opening;

        // Token: 0x0400272F RID: 10031
        public bool closing;

        // Token: 0x0400273C RID: 10044
        public float lastAlpha;

        // Token: 0x0400273D RID: 10045
        public float currentAlpha;

        // Token: 0x0400273E RID: 10046
        public float uAlpha;

        // Token: 0x0400273F RID: 10047
        public float targetAlpha;
    }
}