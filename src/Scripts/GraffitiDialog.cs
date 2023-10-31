using System;
using System.Collections.Generic;
using System.Linq;
using Menu.Remix;
using RWCustom;
using SlugBase.SaveData;
using UnityEngine;
using Vinki;

namespace Menu
{
    public class GraffitiDialog : Dialog
    {
        public GraffitiDialog(ProcessManager manager, Vector2 cancelButtonPos) : base(manager)
        {
            if (this.scene != null)
            {
                return;
            }

            float[] screenOffsets = Custom.GetScreenOffsets();
            this.leftAnchor = screenOffsets[0];
            this.rightAnchor = screenOffsets[1];
            this.pages[0].pos = new Vector2(0.01f, 0f);
            Page page = this.pages[0];
            page.pos.y = page.pos.y + 2000f;
            this.scene = new InteractiveMenuScene(this, pages[0], Enums.GraffitiMap);
            this.pages[0].subObjects.Add(this.scene);

            float cancelButtonWidth = GetCancelButtonWidth(base.CurrLang);
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
            for (int i = 0; i < bgCount; i++)
            {
                this.scene.depthIllustrations[i].sprite.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 1f, this.darkSprite.alpha * 1.25f));
            }
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
            this.cancelButton.buttonBehav.greyedOut = this.opening || graffitiSlapping.Max() > 0;

            if (this.opening)
            {
                return;
            }

            for (int i = 0; i < graffitiSpots.Length; i++)
            {
                if (graffitiSlapping[i] == 0)
                {
                    continue;
                }

                float t = (slapLength - graffitiSlapping[i]) / slapLength;
                graffitiSpots[i].sprite.scale = EaseOutElastic(0.01f, 1f, t);
                graffitiSpots[i].alpha = Mathf.Min(t * 5f, 1f);
                graffitiSlapping[i]--;

                // Only show one graffiti animation at a time
                break;
            }
        }

        private float EaseOutElastic(float start, float end, float value)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if ((value /= d) == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        public static MenuDepthIllustration[] graffitiSpots;
        public static int[] graffitiSlapping;

        public SimpleButton cancelButton;

        public float leftAnchor;
        public float rightAnchor;

        public bool opening;
        public bool closing;

        public float lastAlpha;
        public float currentAlpha;

        public float uAlpha;
        public float targetAlpha;

        public static readonly float slapLength = 40f;
        public static readonly int bgCount = 1;
    }
}
