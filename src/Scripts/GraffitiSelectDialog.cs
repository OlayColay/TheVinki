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
            pages[0].pos = new Vector2(0.01f, 0f);
            Page page = pages[0];
            page.pos.y = page.pos.y + 2000f;

            float cancelButtonWidth = GetCancelButtonWidth(base.CurrLang);
            cancelButton = new SimpleButton(this, pages[0], base.Translate("CLOSE"), "CLOSE", cancelButtonPos, new Vector2(cancelButtonWidth, 30f));
            pages[0].subObjects.Add(cancelButton);
            opening = true;
            targetAlpha = 1f;

            players = game.Players.Select(abs => abs.realizedCreature as Player).ToArray();
            playerButtons = new SimpleButton[players.Length];

            for (int i = 0; i < playerButtons.Length; i++)
            {
                playerButtons[i] = new SimpleButton(this, pages[0], base.Translate("PLAYER " + (i+1)), "PLAYER " + i, new Vector2(cancelButtonPos.x, Screen.height - 50 - 40*i), new Vector2(cancelButtonWidth, 30f));
                pages[0].subObjects.Add(playerButtons[i]);
            }

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
            graffitiButtons = new GraffitiButton[graffitiFiles.Length+1];

            int i = 0;
            for (int y = Screen.height - 100; y > 100; y -= 100)
            {
                for (int x = 50; x < Screen.width - 300; x += 100)
                {
                    if (i == 0)
                    {
                        graffitiButtons[i] = new GraffitiButton(this, pages[0], "Sandbox_Randomize", "SHUFFLE", new Vector2(x, y));
                    }
                    else
                    {
                        graffitiButtons[i] = new GraffitiButton(this, pages[0], graffitiFiles[i-1], "SELECT " + i, new Vector2(x, y));
                    }
                    pages[0].subObjects.Add(graffitiButtons[i]);

                    i++;
                    if (i >= graffitiButtons.Length)
                    {
                        return;
                    }
                }
            }

            // Initiate selected button's color
            foreach (GraffitiButton b in graffitiButtons)
            {
                b.roundedRect.borderColor = null;
            }
            graffitiButtons[Plugin.queuedGNums[currentPlayer]+1].roundedRect.borderColor = MenuColor(MenuColors.White);
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
                int gNum = int.Parse(message.Substring(7)) - 1;
                Debug.Log("Selecting " + Plugin.graffitis[players[currentPlayer].SlugCatClass.ToString()][gNum].imageName);
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                Plugin.queuedGNums[currentPlayer] = gNum;

                // Color selected button
                foreach (GraffitiButton b in graffitiButtons)
                {
                    b.roundedRect.borderColor = null;
                }
                graffitiButtons[gNum+1].roundedRect.borderColor = MenuColor(MenuColors.White);
            }
            else if (message.StartsWith("PLAYER "))
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                currentPlayer = (int)char.GetNumericValue(message[7]);
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
        public GraffitiButton[] graffitiButtons;
        public SimpleButton[] playerButtons;

        public bool opening;
        public bool closing;

        public float lastAlpha;
        public float currentAlpha;

        public float uAlpha;
        public float targetAlpha;

        public Player[] players;
        public int currentPlayer = 0;
    }
}
