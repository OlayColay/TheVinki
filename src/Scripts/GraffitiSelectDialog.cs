using System;
using System.Linq;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;
using Vinki;

namespace Menu
{
    public class GraffitiSelectDialog : Dialog
    {
        public GraffitiSelectDialog(ProcessManager manager, Vector2 cancelButtonPos, RainWorldGame game) : base(manager)
        {
            float[] screenOffsets = Custom.GetScreenOffsets();
            GraffitiStartPos = new Vector2(100f, Screen.height - 250f);
            pages[0].pos = new Vector2(0.01f, 0f);
            Page page = pages[0];
            page.pos.y = page.pos.y + 2000f;

            // Background rects
            roundedRects[0] = new(this, page, new Vector2(75f, 60f), new Vector2(475f, 645f), true);
            page.subObjects.Add(roundedRects[0]);
            roundedRects[1] = new(this, page, new Vector2(620f, 230f), new Vector2(475f, 475f), true);
            page.subObjects.Add(roundedRects[1]);

            // Preview sprite
            previewSprite = new FSprite();
            roundedRects[1].Container.AddChild(previewSprite);

            // Preview label
            previewLabel = new(this, page, "", new Vector2(620f, 180f), new Vector2(475f, 50f), true);
            page.subObjects.Add(previewLabel);

            // Cancel button
            float cancelButtonWidth = GetCancelButtonWidth(base.CurrLang);
            cancelButton = new SimpleButton(this, page, base.Translate("CLOSE"), "CLOSE", cancelButtonPos, new Vector2(cancelButtonWidth, 30f));
            page.subObjects.Add(cancelButton);
            opening = true;
            targetAlpha = 1f;

            // Player buttons
            players = game.Players.Select(abs => abs.realizedCreature as Player).ToArray();
            playerButtons = new SimpleButton[players.Length];

            for (int i = 0; i < playerButtons.Length; i++)
            {
                playerButtons[i] = new SimpleButton(this, page, base.Translate("PLAYER " + (i+1)), "PLAYER " + i, new Vector2(cancelButtonPos.x, Screen.height - 50 - 40*i), new Vector2(cancelButtonWidth, 30f));
                page.subObjects.Add(playerButtons[i]);
            }

            // Page switch buttons
            nextButton = new(this, page, "NEXT PAGE", new Vector2(430f, 635f), 1);
            page.subObjects.Add(nextButton);
            prevButton = new(this, page, "PREV PAGE", new Vector2(150f, 635f), 3);
            page.subObjects.Add(prevButton);
            pageLabel = new(this, page, "PAGE X/Y", new Vector2(150f, 635f), new Vector2(335f, 50f), true);
            page.subObjects.Add(pageLabel);

            PopulateGraffitiButtons();
        }

        private void PopulateGraffitiButtons()
        {
            // First delete any graffiti buttons on screen
            pages[0].subObjects.ForEach(delegate (MenuObject obj) 
            { 
                if (obj is GraffitiButton) 
                { 
                    obj.RemoveSprites(); 
                } 
            });
            for (int j = 0; j < graffitiButtons?.Length; j++)
            {
                pages[0].RemoveSubObject(graffitiButtons[j]);
            }

            string[] graffitiFiles;
            if (Plugin.graffitis.ContainsKey(players[currentPlayer].SlugCatClass.ToString()))
            {
                graffitiFiles = Plugin.graffitis[players[currentPlayer].SlugCatClass.ToString()].Select(g => "decals/" + g.imageName).ToArray();
            }
            else
            {
                graffitiFiles = Plugin.graffitis["White"].Select(g => "decals/" + g.imageName).ToArray();
            }
            graffitiButtons = new GraffitiButton[Math.Min(GraffitiPerPage, graffitiFiles.Length - (curGPage * GraffitiPerPage))];

            int gNum = curGPage * GraffitiPerPage;
            float yPos = GraffitiStartPos.y;
            for (int i = 0; i < NumRows && gNum < graffitiFiles.Length; i++)
            {
                float xPos = GraffitiStartPos.x;
                for (int j = 0; j < NumCols && gNum < graffitiFiles.Length; j++) 
                {
                    graffitiButtons[i * NumCols + j] = new GraffitiButton(this, pages[0], graffitiFiles[gNum], "SELECT " + gNum, new Vector2(xPos, yPos));
                    pages[0].subObjects.Add(graffitiButtons[i * NumCols + j]);
                    xPos += GraffitiSpacing;
                    gNum++;
                }
                yPos -= GraffitiSpacing;
            }

            pageCount = (int)Math.Ceiling((float)graffitiFiles.Length / GraffitiPerPage);
            nextButton.buttonBehav.greyedOut = ((curGPage + 1) >= pageCount);
            prevButton.buttonBehav.greyedOut = (curGPage == 0);
            pageLabel.text = Translate("PAGE") + ' ' + (curGPage+1) + '/' + pageCount;

            //int i = 0;
            //for (int y = Screen.height - 100; y > 100; y -= 100)
            //{
            //    for (int x = 50; x < Screen.width - 300; x += 100)
            //    {
            //        if (i == 0)
            //        {
            //            graffitiButtons[i] = new GraffitiButton(this, pages[0], "Sandbox_Randomize", "SHUFFLE", new Vector2(x, y));
            //        }
            //        else
            //        {
            //            graffitiButtons[i] = new GraffitiButton(this, pages[0], graffitiFiles[i-1], "SELECT " + i, new Vector2(x, y));
            //        }
            //        pages[0].subObjects.Add(graffitiButtons[i]);

            //        i++;
            //        if (i >= graffitiButtons.Length)
            //        {
            //            return;
            //        }
            //    }
            //}

            // Initiate selected button's color
            //foreach (GraffitiButton b in graffitiButtons)
            //{
            //    b.roundedRect.borderColor = null;
            //}
            int queuedGNum = Plugin.queuedGNums[currentPlayer];
            if (queuedGNum >= curGPage * GraffitiPerPage && queuedGNum < (curGPage + 1) * GraffitiPerPage)
            {
                graffitiButtons[queuedGNum % GraffitiPerPage].roundedRect.borderColor = MenuColor(MenuColors.White);
            }
        }

        private void UpdatePreview(string spritePath)
        {
            previewSprite.RemoveFromContainer();
            previewSprite = new(spritePath);
            float newScale = 465f / Mathf.Max(previewSprite.width, previewSprite.height);
            previewSprite.scale = newScale;
            previewSprite.x = roundedRects[1].DrawX(1f) + (roundedRects[1].size.x / 2);
            previewSprite.y = roundedRects[1].DrawY(1f) + (roundedRects[1].size.y / 2);
            roundedRects[1].Container.AddChild(previewSprite);

            Debug.Log("Preview: " + spritePath);
            previewLabel.text = spritePath.Substring(spritePath.LastIndexOf("/") + 1);
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
            else if (message.StartsWith("SELECT "))
            {
                int gNum = int.Parse(message.Substring(7));
                Debug.Log("Selecting " + Plugin.graffitis[players[currentPlayer].SlugCatClass.ToString()][gNum].imageName);
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                Plugin.queuedGNums[currentPlayer] = gNum;

                // Color selected button
                foreach (GraffitiButton b in graffitiButtons)
                {
                    b.roundedRect.borderColor = null;
                }
                graffitiButtons[gNum % GraffitiPerPage].roundedRect.borderColor = MenuColor(MenuColors.White);

                // Update graffiti preview
                UpdatePreview("decals/" + Plugin.graffitis[players[currentPlayer].SlugCatClass.ToString()][gNum].imageName);
            }
            else if (message.StartsWith("PLAYER "))
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                currentPlayer = (int)char.GetNumericValue(message[7]);
                curGPage = 0;
                PopulateGraffitiButtons();

                // Color selected player
                //foreach (SimpleButton b in playerButtons)
                //{
                //    b.InterpColor(Color.white);
                //}
                //playerButtons[currentPlayer].InterpColor(players[currentPlayer].JollyOption.bodyColor);
            }
            else if (message == "SHUFFLE")
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                Plugin.queuedGNums[currentPlayer] = -1;

                // Color selected button
                foreach (GraffitiButton b in graffitiButtons)
                {
                    b.roundedRect.borderColor = null;
                }
                graffitiButtons[0].roundedRect.borderColor = MenuColor(MenuColors.White);
            }
            else if (message.EndsWith("PAGE"))
            {
                curGPage += message.StartsWith("NEXT") ? 1 : -1;
                PopulateGraffitiButtons();
            }
        }

        public override void Update()
        {
            Debug.Log("Selectables: " + pages[0].selectables.Count() + "\tSubObjects: " + pages[0].subObjects.Count());
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

        public RoundedRect[] roundedRects = new RoundedRect[2];
        public SimpleButton cancelButton;
        public GraffitiButton[] graffitiButtons;
        public SimpleButton[] playerButtons;
        public BigArrowButton nextButton;
        public BigArrowButton prevButton;
        public MenuLabel pageLabel;

        public bool opening;
        public bool closing;

        public float lastAlpha;
        public float currentAlpha;

        public float uAlpha;
        public float targetAlpha;

        public Player[] players;
        public int currentPlayer = 0;

        public int curGPage = 0;
        public int pageCount = 1;

        public FSprite previewSprite;
        public MenuLabel previewLabel;

        public readonly static int NumRows = 5;
        public readonly static int NumCols = 4;
        public readonly static int GraffitiPerPage = NumRows * NumCols;

        public readonly Vector2 GraffitiStartPos;
        public readonly static float GraffitiSpacing = 110f;
    }
}
