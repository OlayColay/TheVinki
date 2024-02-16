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
    public class GraffitiSelectDialog : Dialog
    {
        public GraffitiSelectDialog(ProcessManager manager, Vector2 cancelButtonPos) : base(manager)
        {
            float[] screenOffsets = Custom.GetScreenOffsets();
            leftAnchor = screenOffsets[0];
            rightAnchor = screenOffsets[1];
            pages[0].pos = new Vector2(0.01f, 0f);
            Page page = pages[0];
            page.pos.y = page.pos.y + 2000f;

            float cancelButtonWidth = GetCancelButtonWidth(base.CurrLang);
            cancelButton = new SimpleButton(this, pages[0], base.Translate("CLOSE"), "CLOSE", cancelButtonPos, new Vector2(cancelButtonWidth, 30f));
            pages[0].subObjects.Add(cancelButton);
            opening = true;
            targetAlpha = 1f;

            PopulateGraffitiButtons();
        }

        private void PopulateGraffitiButtons()
        {
            for (int y = 100; y < Screen.height - 100; y += 100)
            {
                for (int x = 100; x < Screen.width - 100; x += 100)
                {
                    GraffitiButton grafButton = new(this, pages[0], "Sandbox_Randomize", "SELECT RANDOM", new Vector2(x, y));
                    pages[0].subObjects.Add((grafButton));
                }
            }
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
            if (opening || closing)
            {
                uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, currentAlpha, timeStacker)), 1.5f);
                darkSprite.alpha = uAlpha * 0.95f;
            }
            pages[0].pos.y = Mathf.Lerp(manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, (uAlpha < 0.999f) ? uAlpha : 1f);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if (message == "CLOSE")
            {
                closing = true;
                targetAlpha = 0f;
            }
        }

        public override void Update()
        {
            base.Update();
            lastAlpha = currentAlpha;
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);
            if (opening && pages[0].pos.y <= 0.01f)
            {
                opening = false;
            }
            if (closing && Math.Abs(currentAlpha - targetAlpha) < 0.09f)
            {
                manager.StopSideProcess(this);
                closing = false;
            }
            cancelButton.buttonBehav.greyedOut = opening;

            if (opening)
            {
                return;
            }
        }

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
