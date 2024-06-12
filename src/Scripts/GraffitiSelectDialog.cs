using System;
using System.Linq;
using RWCustom;
using UnityEngine;
using Vinki;

namespace Menu
{
    public class GraffitiSelectDialog : Dialog
    {
        public GraffitiSelectDialog(ProcessManager manager, Vector2 cancelButtonPos, RainWorldGame game) : base(manager)
        {
            float[] screenOffsets = Custom.GetScreenOffsets();
            GraffitiStartPos = new Vector2(100f, manager.rainWorld.screenSize.y - 250f);
            pages[0].pos = new Vector2(100f, 25f);
            Page page = pages[0];
            page.pos.y += 2000f;
            players = game.Players.Where(abs => abs.realizedCreature != null && abs.realizedCreature is Player).Select(abs => abs.realizedCreature as Player).ToArray();

            // Background rects
            roundedRects[0] = new(this, page, new Vector2(75f, 60f), new Vector2(475f, 645f), true);
            page.subObjects.Add(roundedRects[0]);
            roundedRects[1] = new(this, page, new Vector2(620f, players.Length > 1 ? 230f : 150f), new Vector2(475f, 475f), true);
            page.subObjects.Add(roundedRects[1]);

            // Preview sprite
            previewSprite = new FSprite();
            roundedRects[1].Container.AddChild(previewSprite);

            // Preview label
            previewLabel = new(this, page, "", new Vector2(620f, players.Length > 1 ? 180f : 100f), new Vector2(475f, 50f), true);
            page.subObjects.Add(previewLabel);

            // Cancel button
            float cancelButtonWidth = GetCancelButtonWidth(CurrLang);
            cancelButton = new SimpleButton(this, page, Translate("CLOSE"), "CLOSE", cancelButtonPos - page.pos + new Vector2(0f, 2025f), new Vector2(cancelButtonWidth, 30f));
            page.subObjects.Add(cancelButton);

            opening = true;
            targetAlpha = 1f;

            // Player buttons
            if (players.Length > 1)
            {
                playerButtons = new BigSimpleButton[players.Length];
                playerSprites = new MenuIllustration[players.Length];
                float spacing = players.Length > 4 ? 61f : 114f;
                Vector2 buttonSize = players.Length > 4 ? new(50f, 50f) : new(89f, 89f);
                float playersY = players.Length > 4 ? 125f : 75f;
                float playersX = players.Length > 4 ? 620f : 640f;
                bool defaultColors = Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.DEFAULT;
                for (int i = 0; i < players.Length; i++)
                {
                    Color color = PlayerGraphics.SlugcatColor((players[i].State as PlayerState).slugcatCharacter);
                    bool isVanilla = players[i].SlugCatClass == SlugcatStats.Name.White || players[i].SlugCatClass == SlugcatStats.Name.Yellow || players[i].SlugCatClass == SlugcatStats.Name.Red;
                    playerButtons[i] = new(this, page, "", "PLAYER " + i, new Vector2(playersX + (spacing * (i % 8)), playersY - (i >= 8 ? spacing : 0f)), buttonSize, FLabelAlignment.Center, false);
                    string path = string.Concat(
                        [
                            "MultiplayerPortrait",
                        (defaultColors && !isVanilla) ? "41" : "01",
                        "-",
                        players[i].SlugCatClass.value
                        ]);
                    //Custom.Log("MultiplayerPortrait: " + path);
                    playerSprites[i] = new(this, page, "", path, playerButtons[i].pos + playerButtons[i].size / 2f, true, true
                    )
                    {
                        color = (defaultColors && !isVanilla) ? Color.white : color
                    };
                    playerSprites[i].sprite.scale = players.Length > 4 ? 0.5f : 1f;
                    page.subObjects.Add(playerButtons[i]);
                    page.subObjects.Add(playerSprites[i]);
                }
            }

            // Page switch buttons
            nextButton = new(this, page, "NEXT PAGE", new Vector2(405f, 635f), 1);
            page.subObjects.Add(nextButton);
            prevButton = new(this, page, "PREV PAGE", new Vector2(175f, 635f), 3);
            page.subObjects.Add(prevButton);
            pageLabel = new(this, page, "PAGE X/Y", new Vector2(150f, 635f), new Vector2(335f, 50f), true);
            page.subObjects.Add(pageLabel);

            // Shuffle button
            shuffleButton = new(this, page, "SHUFFLE", new Vector2(115f, 635f), 0);
            shuffleButton.symbolSprite.element = Futile.atlasManager.GetElementWithName("vinki_mediashuffle");
            shuffleButton.symbolSprite.scale = 0.85f;
            page.subObjects.Add(shuffleButton);

            // Repeat button
            repeatButton = new(this, page, "REPEAT", new Vector2(465f, 635f), 0);
            repeatButton.symbolSprite.element = Futile.atlasManager.GetElementWithName("vinki_mediarepeat");
            repeatButton.symbolSprite.scale = 0.85f;
            page.subObjects.Add(repeatButton);

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

            if (Plugin.queuedGNums[currentPlayer] == -1)
            {
                shuffleButton.roundedRect.borderColor = MenuColor(MenuColors.White);
                repeatButton.buttonBehav.greyedOut = true;
            }
            else
            {
                shuffleButton.roundedRect.borderColor = null;
                repeatButton.buttonBehav.greyedOut = false;
            }
        }

        private void UpdatePreview(string spritePath)
        {
            previewSprite.RemoveFromContainer();

            if (spritePath == "")
            {
                return;
            }

            previewSprite = new(spritePath);
            float newScale = 465f / Mathf.Max(previewSprite.width, previewSprite.height);
            previewSprite.scale = newScale;
            previewSprite.x = roundedRects[1].DrawX(1f) + (roundedRects[1].size.x / 2);
            previewSprite.y = roundedRects[1].DrawY(1f) + (roundedRects[1].size.y / 2);
            roundedRects[1].Container.AddChild(previewSprite);

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
                previewSprite.x = roundedRects[1].DrawX(1f) + (roundedRects[1].size.x / 2);
                previewSprite.y = roundedRects[1].DrawY(1f) + (roundedRects[1].size.y / 2);
            }
            pages[0].pos.y = Mathf.Lerp(manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, (uAlpha < 0.999f) ? uAlpha : 1f);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if (message.StartsWith("CLOSE"))
            {
                closing = true;
                targetAlpha = 0f;
                if (message == "CLOSE")
                {
                    PlaySound(SoundID.MENU_Switch_Page_Out);
                }
            }
            else if (message.StartsWith("SELECT "))
            {
                int gNum = int.Parse(message.Substring(7));
                Plugin.VLogger.LogInfo("Selecting " + Plugin.graffitis[players[currentPlayer].SlugCatClass.ToString()][gNum].imageName);
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                Plugin.queuedGNums[currentPlayer] = gNum;

                // Color selected button
                foreach (GraffitiButton b in graffitiButtons)
                {
                    b.roundedRect.borderColor = null;
                }
                graffitiButtons[gNum % GraffitiPerPage].roundedRect.borderColor = MenuColor(MenuColors.White);
                shuffleButton.roundedRect.borderColor = null;
                repeatButton.buttonBehav.greyedOut = false;

                // Update graffiti preview
                UpdatePreview("decals/" + Plugin.graffitis[players[currentPlayer].SlugCatClass.ToString()][gNum].imageName);
            }
            else if (message.StartsWith("PLAYER "))
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                currentPlayer = int.Parse(message.Substring(7));
                curGPage = 0;
                PopulateGraffitiButtons();

                // Color selected player
                //foreach (SimpleButton b in playerButtons)
                //{
                //    b.InterpColor(Color.white);
                //}
                //playerButtons[currentPlayer].InterpColor(players[currentPlayer].JollyOption.bodyColor);

                int gNum = Plugin.queuedGNums[currentPlayer];
                UpdatePreview(gNum == -1 ? "" : "decals/" + Plugin.graffitis[players[currentPlayer].SlugCatClass.ToString()][gNum].imageName);
                previewLabel.text = players[currentPlayer].JollyOption.customPlayerName ?? Translate("Player " + (currentPlayer + 1));
            }
            else if (message == "SHUFFLE")
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                Plugin.queuedGNums[currentPlayer] = -1;
                Plugin.repeatGraffiti[currentPlayer] = false;

                // Color selected button
                foreach (GraffitiButton b in graffitiButtons)
                {
                    b.roundedRect.borderColor = null;
                }
                repeatButton.roundedRect.borderColor = null;
                shuffleButton.roundedRect.borderColor = MenuColor(MenuColors.White);

                repeatButton.buttonBehav.greyedOut = true;

                previewLabel.text = Translate("Shuffling between designs randomly");
                UpdatePreview("");
            }
            else if (message == "REPEAT")
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                Plugin.repeatGraffiti[currentPlayer] = !Plugin.repeatGraffiti[currentPlayer];

                // Color selected button
                repeatButton.roundedRect.borderColor = Plugin.repeatGraffiti[currentPlayer] ? MenuColor(MenuColors.White) : null;

                previewLabel.text = Translate(Plugin.repeatGraffiti[currentPlayer] ? "Spraying this design repeatedly" : "Spraying this design once then shuffling");
            }
            else if (message.EndsWith("PAGE"))
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                curGPage += message.StartsWith("NEXT") ? 1 : -1;
                PopulateGraffitiButtons();
            }
        }

        public override void Update()
        {
            Plugin.VLogger.LogInfo("Selectables: " + pages[0].selectables.Count() + "\tSubObjects: " + pages[0].subObjects.Count());
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
        //public SimpleButton[] playerButtons;
        public BigSimpleButton[] playerButtons;
        public MenuIllustration[] playerSprites;
        public BigArrowButton nextButton;
        public BigArrowButton prevButton;
        public BigArrowButton shuffleButton;
        public BigArrowButton repeatButton;
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
