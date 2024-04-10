using UnityEngine;

namespace Menu
{
    // Token: 0x02000460 RID: 1120
    public class GraffitiButton : ButtonTemplate
    {
        // Token: 0x06002C98 RID: 11416 RVA: 0x00369700 File Offset: 0x00367900
        public override Color MyColor(float timeStacker)
        {
            if (!buttonBehav.greyedOut)
            {
                float num = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
                num = Mathf.Max(num, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
                HSLColor from = HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.MediumGrey), num);
                return HSLColor.Lerp(from, Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
            }
            if (maintainOutlineColorWhenGreyedOut)
            {
                return Menu.MenuRGB(Menu.MenuColors.DarkGrey);
            }
            return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
        }

        // Token: 0x06002C99 RID: 11417 RVA: 0x003697D0 File Offset: 0x003679D0
        public GraffitiButton(Menu menu, MenuObject owner, string symbolName, string singalText, Vector2 pos) : base(menu, owner, pos, new Vector2(99f, 99f))
        {
            signalText = singalText;
            roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, true);
            subObjects.Add(roundedRect);
            symbolSprite = new FSprite(symbolName, true);
            Container.AddChild(symbolSprite);

            float newScale = 90f / Mathf.Max(symbolSprite.width, symbolSprite.height);
            symbolSprite.scale = newScale;
        }

        // Token: 0x06002C9A RID: 11418 RVA: 0x00369850 File Offset: 0x00367A50
        public override void Update()
        {
            base.Update();
            buttonBehav.Update();
            roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
            roundedRect.addSize = new Vector2(4f, 4f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * 3.1415927f)) * (buttonBehav.clicked ? 0f : 1f);
        }

        // Token: 0x06002C9B RID: 11419 RVA: 0x003698F8 File Offset: 0x00367AF8
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            symbolSprite.color = (buttonBehav.greyedOut ? Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey) : base.MyColor(timeStacker));
            symbolSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
            symbolSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
            Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
            for (int i = 0; i < 9; i++)
            {
                roundedRect.sprites[i].color = color;
            }
        }

        // Token: 0x06002C9C RID: 11420 RVA: 0x00369A35 File Offset: 0x00367C35
        public void UpdateSymbol(string newSymbolName)
        {
            symbolSprite.element = Futile.atlasManager.GetElementWithName(newSymbolName);
        }

        // Token: 0x06002C9D RID: 11421 RVA: 0x00369A4D File Offset: 0x00367C4D
        public override void RemoveSprites()
        {
            symbolSprite.RemoveFromContainer();
            base.RemoveSprites();
        }

        // Token: 0x06002C9E RID: 11422 RVA: 0x00369A60 File Offset: 0x00367C60
        public override void Clicked()
        {
            Singal(this, signalText);
        }

        // Token: 0x04002A4F RID: 10831
        public RoundedRect roundedRect;

        // Token: 0x04002A50 RID: 10832
        public string signalText;

        // Token: 0x04002A51 RID: 10833
        public FSprite symbolSprite;

        // Token: 0x04002A52 RID: 10834
        public bool maintainOutlineColorWhenGreyedOut;
    }
}
