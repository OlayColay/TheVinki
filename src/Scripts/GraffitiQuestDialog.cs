using System;
using System.Linq;
using RWCustom;
using UnityEngine;
using Vinki;

namespace Menu
{
    public class GraffitiQuestDialog : Dialog
    {
        public GraffitiQuestDialog(ProcessManager manager, Vector2 cancelButtonPos) : base(manager)
        {
            if (scene != null)
            {
                return;
            }

            float[] screenOffsets = Custom.GetScreenOffsets();
            leftAnchor = screenOffsets[0];
            rightAnchor = screenOffsets[1];
            pages[0].pos = new Vector2(0.01f, 0f);
            Page page = pages[0];
            page.pos.y += 2000f;
            scene = new InteractiveMenuScene(this, pages[0], Enums.MenuSceneID.GraffitiMap);
            pages[0].subObjects.Add(scene);

            float cancelButtonWidth = GetCancelButtonWidth(CurrLang);
            cancelButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CLOSE", cancelButtonPos, new Vector2(cancelButtonWidth, 30f));
            pages[0].subObjects.Add(cancelButton);
            opening = true;
            targetAlpha = 1f;
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
            for (int i = 0; i < scene.depthIllustrations.Count; i++)
            {
                if (i < bgCount || graffitiSlapping[i - bgCount] == 0)
                {
                    scene.depthIllustrations[i].sprite.alpha = Mathf.Lerp(0f, 1f, Mathf.Lerp(0f, 1f, darkSprite.alpha * 1.25f));
                }
            }
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
            cancelButton.buttonBehav.greyedOut = opening || graffitiSlapping.Max() > 0;

            if (opening)
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
                graffitiSpots[i].sprite.scale = EaseOutElastic(0.001f, 1f, t);
                graffitiSpots[i].alpha = Mathf.Min(t * 5f, 1f);
                graffitiSlapping[i]--;

                // Only show one graffiti animation at a time
                break;
            }

            if (removeCloud > 0 && graffitiSlapping.All(x => x == 0))
            {
                float t = (slapLength - removeCloud) / slapLength;
                cloud.sprite.scale = Mathf.Abs(EaseOutElastic(1f, 0.001f, t));
                cloud.alpha = Mathf.Max(1f - t * 5f, 0f);
                removeCloud--;
            }
        }

        public static float EaseOutElastic(float start, float end, float value)
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

        public static MenuDepthIllustration[] graffitiSpots = new MenuDepthIllustration[10];
        public static int[] graffitiSlapping;

        public static MenuDepthIllustration cloud;
        public static int removeCloud = 0;

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
        public static readonly int bgCount = 3;
    }
}
