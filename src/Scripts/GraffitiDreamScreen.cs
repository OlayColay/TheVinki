using System;
using System.Collections.Generic;
using System.Linq;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace Menu
{
    public class GraffitiDialog : Dialog
    {
        public GraffitiDialog(ProcessManager manager, Vector2 cancelButtonPos) : base(manager)
        {
            float[] screenOffsets = Custom.GetScreenOffsets();
            this.leftAnchor = screenOffsets[0];
            this.rightAnchor = screenOffsets[1];
            this.pages[0].pos = new Vector2(0.01f, 0f);
            Page page = this.pages[0];
            page.pos.y = page.pos.y + 2000f;

            this.background = new MenuIllustration(this, this.pages[0], "", "graffiti_map", new Vector2(Screen.width/2, Screen.height/2), true, true);
            this.pages[0].subObjects.Add(this.background);

            this.graffitiSpots[0] = new MenuIllustration(this, this.pages[0], "", "graffiti_ss", new Vector2(Screen.width / 2, Screen.height / 2), true, true);

            for (int i = 0; i < graffitiSpots.Length; i++)
            {
                graffitiSlapping[i] = 60;
                graffitiSpots[i].sprite.scale = 3f;
                graffitiSpots[i].alpha = 0f;

                this.pages[0].subObjects.Add(graffitiSpots[i]);
            }

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
            this.background.sprite.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 0.85f, this.darkSprite.alpha));
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if (message == "CLOSE")
            {
                this.closing = true;
                this.targetAlpha = 0f;
            }
        }

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

            if (this.opening)
            {
                return;
            }

            for (int i = 0; i < this.graffitiSpots.Length; i++)
            {
                if (graffitiSlapping[i] == 0)
                {
                    continue;
                }

                float t = (60f - graffitiSlapping[i]) / 60f;
                Debug.Log("Graffiti width: " + graffitiSpots[i].sprite.scaleX);
                graffitiSpots[i].sprite.scale = Mathf.Lerp(3f, 1f, t);
                graffitiSpots[i].alpha = t * 1.2f;
                graffitiSlapping[i]--;

                // Only show one graffiti animation at a time
                break;
            }
        }

        public MenuIllustration background;
        public MenuIllustration[] graffitiSpots = new MenuIllustration[1];
        public int[] graffitiSlapping = new int[1];

        public SimpleButton cancelButton;

        public float leftAnchor;
        public float rightAnchor;

        public bool opening;
        public bool closing;

        public float lastAlpha;
        public float currentAlpha;

        public float uAlpha;
        public float targetAlpha;
    }
}
