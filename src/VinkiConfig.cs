using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using SlugBase;
using SlugBase.SaveData;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Vinki
{
    public class VinkiConfig : OptionInterface
    {
        public static VinkiConfig Instance { get; } = new();
        public static Configurable<bool> RequireSprayCans;
        public static Configurable<bool> UpGraffiti;
        public static Configurable<int> GraffitiFadeTime;
        public static Configurable<bool> DeleteGraffiti;
        public static Configurable<bool> RestoreGraffitiOnUpdate;
        public static Configurable<bool> ShowVinkiTitleCard;
        public static Configurable<bool> GlassesOverDMS;
        public static Configurable<bool> TagDamageJolly;

        public VinkiConfig()
        {
            RequireSprayCans = config.Bind("requireSprayCans", true, new ConfigurableInfo("Requires a spray can to spray graffiti (craft one with a rock and a colorful item).", tags: new object[]
            {
                "Require Spray Cans for Graffiti"
            }));
            UpGraffiti = config.Bind("upGraffiti", true, new ConfigurableInfo("Use the Up direction for Graffiti Mode (in addition to the normal binding).", tags: new object[]
            {
                "Use Up as Graffiti Mode"
            }));
            GraffitiFadeTime = config.Bind("graffitiFadeTime", 5, new ConfigurableInfo("How many cycles sprayed graffiti should last (excludes story-related graffiti). Use -1 for infinite cycles.", new ConfigAcceptableRange<int>(-1, 999), tags: new object[]
            {
                "Graffiti Display Cycles"
            }));
            DeleteGraffiti = config.Bind("deleteGraffiti", false, new ConfigurableInfo("Delete Graffiti permanently when running out of display cycles. Will help with loading times if you've sprayed a lot of graffiti.", tags: new object[]
            {
                "Delete Graffiti Permanently After Display Cycles"
            }));
            RestoreGraffitiOnUpdate = config.Bind("restoreGraffitiOnUpdate", true, new ConfigurableInfo("Restore default graffiti when the mod updates to a new version. Helpful to automatically add any new graffiti from updates.", tags: new object[]
            {
                "Restore Default Graffiti when Mod Updates"
            }));
            ShowVinkiTitleCard = config.Bind("showVinkiTitleCard", true, new ConfigurableInfo("Always show one of the Vinki title cards when starting the game. This replaces other title cards, and the mod must be high in the mod order on the left to work!", tags: new object[]
            {
                "Always Show Vinki Title Cards"
            }));
            GlassesOverDMS = config.Bind("glassesOverDMS", true, new ConfigurableInfo("Wear Vinki's glasses on top of the current DMS skin for Vinki. Only works if you have DMS enabled.", tags: new object[]
            {
                "Wear Glasses Over DMS Skin"
            }));
            TagDamageJolly = config.Bind("tagDamageJolly", false, new ConfigurableInfo("Tagging a player while in Jolly Coop does damage.", tags: new object[]
            {
                "Tagging Damages Jolly Players"
            }));
        }

        public static void RegisterOI()
        {
            if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
                MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
        }

        // Called when the config menu is opened by the player.
        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "Options"),
                new OpTab(this, "Credits"),
                new OpTab(this, "Vinki Graffiti"),
                new OpTab(this, "Other Graffiti 1"),
                new OpTab(this, "Other Graffiti 2"),
            };

            // Options tab
            AddDivider(593f);
            AddTitle();
            AddDivider(557f);
            AddCheckbox(RequireSprayCans, 520f);
            AddCheckbox(UpGraffiti, 480f);
            AddIntBox(GraffitiFadeTime, 440f);
            AddCheckbox(DeleteGraffiti, 400f);
            AddCheckbox(RestoreGraffitiOnUpdate, 360f);
            AddHoldButton(
                "Restore Default Graffiti",
                "Restore the default graffiti that came with The Vinki. Useful for after installing an update that includes new default graffiti.",
                RestoreDefaultGraffiti,
                320f,
                200f,
                40f
            );
            AddHoldButton(
                "Reset Graffiti Folder to Default",
                "Revert Graffiti Folder to default. This will remove any custom files you've added to it!",
                ResetGraffitiFolder,
                280f,
                200f,
                color: Color.red
            );
            AddCheckbox(ShowVinkiTitleCard, 240f);
            AddCheckbox(GlassesOverDMS, 200f);
            AddCheckbox(TagDamageJolly, 160f);

            // Credits tab
            AddDivider(593f, 1);
            AddTitle(1);
            AddDivider(557f, 1);
            AddSubtitle(530f, "Art", 1);
            AddText(510f, "Beep", 1);
            AddSubtitle(470f, "Coding", 1);
            AddText(450f, "OlayColay", 1);
            AddSubtitle(410f, "Cursed Art", 1);
            AddText(390f, "Beep    Bluzai    MagicaJaphet    MaxDubstep    OlayColay", 1);
            AddSubtitle(350f, "Level Editing", 1);
            AddText(330f, "TarnishedPotato", 1);
            AddSubtitle(290f, "Music", 1);
            AddText(270f, "MaxDubstep", 1);
            AddSubtitle(230f, "Sound Effects", 1);
            AddText(210f, "MaxDubstep", 1);
            AddSubtitle(170f, "Writing", 1);
            AddText(150f, "Beep    MaxDubstep    OlayColay    TarnishedPotato    Tsunochizu", 1);
            AddSubtitle(60f, "Special Thanks", 1);
            AddText(30f, "Developers of this mod's dependencies\n" +
                "Abigail    banba fan    Doop    goof    JayDee    Nico    Rae    Sadman    skrybl    Sunbloom    SunnyBeam\n",
            1);

            // Vinki Graffiti tab
            AddGraffiti(525f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "vinki", 2);

            // Other Graffiti 1 tab
            AddSubtitle(580f, "Monk", 3);
            float curY = AddGraffiti(525f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Yellow", 3);
            AddSubtitle(curY + 30f, "Survivor", 3);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "White", 3);
            AddSubtitle(curY + 30f, "Hunter", 3);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Red", 3);
            AddSubtitle(curY + 30f, "Gourmand", 3);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Gourmand", 3);
            AddSubtitle(curY + 30f, "Artificer", 3);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Artificer", 3);

            // Other Graffiti 2 tab
            AddSubtitle(580f, "Rivulet", 4);
            curY = AddGraffiti(525f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Rivulet", 4);
            AddSubtitle(curY + 30f, "Spearmaster", 4);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Spear", 4);
            AddSubtitle(curY + 30f, "Saint", 4);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Saint", 4);
            AddSubtitle(curY + 30f, "Sofanthiel", 4);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Sofanthiel", 4);
        }

        // Combines two flipped 'LinearGradient200's together to make a fancy looking divider.
        private void AddDivider(float y, int tab = 0)
        {
            OpImage dividerLeft = new OpImage(new Vector2(300f, y), "LinearGradient200");
            dividerLeft.sprite.SetAnchor(0.5f, 0f);
            dividerLeft.sprite.rotation = 270f;

            OpImage dividerRight = new OpImage(new Vector2(300f, y), "LinearGradient200");
            dividerRight.sprite.SetAnchor(0.5f, 0f);
            dividerRight.sprite.rotation = 90f;

            Tabs[tab].AddItems(new UIelement[]
            {
                dividerLeft,
                dividerRight
            });
        }

        // Adds the mod name to the interface.
        private void AddTitle(int tab = 0)
        {
            OpLabel title = new OpLabel(new Vector2(150f, 560f), new Vector2(300f, 30f), "The Vinki", bigText: true);

            Tabs[tab].AddItems(new UIelement[]
            {
                title
            });
        }

        // Adds a subtitle to the interface.
        private void AddSubtitle(float y, string text, int tab = 0)
        {
            OpLabel title = new OpLabel(new Vector2(200f, y), new Vector2(200f, 20f), text, bigText: true);

            Tabs[tab].AddItems(new UIelement[]
            {
                title
            });
        }

        // Adds small text to the interface.
        private void AddText(float y, string text, int tab = 0)
        {
            OpLabel title = new OpLabel(new Vector2(250f, y), new Vector2(100f, 10f), text);

            Tabs[tab].AddItems(new UIelement[]
            {
                title
            });
        }

        // Adds a checkbox tied to the config setting passed through `optionText`, as well as a label next to it with a description.
        private void AddCheckbox(Configurable<bool> optionText, float y)
        {
            OpCheckBox checkbox = new OpCheckBox(optionText, new Vector2(150f, y))
            {
                description = optionText.info.description
            };

            OpLabel checkboxLabel = new OpLabel(150f + 40f, y + 2f, optionText.info.Tags[0] as string)
            {
                description = optionText.info.description
            };

            Tabs[0].AddItems(new UIelement[]
            {
                checkbox,
                checkboxLabel
            });
        }

        private void AddIntBox(Configurable<int> optionText, float y)
        {
            OpUpdown opUpdown = new OpUpdown(optionText, new Vector2(100f, y - 4f), 75f);
            opUpdown.description = Translate(optionText.info.description);
            //if (uifocusable != null)
            //{
            //    UIfocusable.MutualVerticalFocusableBind(uifocusable, opUpdown);
            //}
            opUpdown.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
            opUpdown.SetNextFocusable(UIfocusable.NextDirection.Right, opUpdown);
            Tabs[0].AddItems(new UIelement[]
            {
                opUpdown
            });
            //uifocusable = opUpdown;
            Tabs[0].AddItems(new UIelement[]
            {
                new OpLabel(190f, y + 0f, Translate(optionText.info.Tags[0] as string), false)
                {
                    bumpBehav = opUpdown.bumpBehav,
                    description = opUpdown.description
                }
            });
        }

        private void AddHoldButton(string displayName, string description, OnSignalHandler action, float y, float width, float fillTime = 80f, Color? color = null)
        {
            OpHoldButton holdButton = new OpHoldButton(new Vector2(150f, y), new Vector2(width, 30f), Translate(displayName), fillTime)
            {
                description = Translate(description),
                colorEdge = color ?? MenuColorEffect.rgbMediumGrey
            };
            holdButton.OnPressDone += action;

            Tabs[0].AddItems(new UIelement[]
            {
                holdButton
            });
        }

        private void RestoreDefaultGraffiti(UIfocusable trigger)
        {
            if (!Hooks.CopyGraffitiBackup())
            {
                return;
            }
            trigger.PlaySound(SoundID.MENU_Start_New_Game);
            trigger.held = false;

            Hooks.LoadGraffiti();
        }

        private void ResetGraffitiFolder(UIfocusable trigger)
        {
            Directory.Delete(Plugin.graffitiFolder, true);
            RestoreDefaultGraffiti(trigger);
        }

        private static readonly int imgHeight = 50;
        private float AddGraffiti(float yStart, string folderPath, int tab)
        {
            var names = Directory.EnumerateFiles(AssetManager.ResolveDirectory(folderPath), "*.png", SearchOption.AllDirectories).ToArray()
                .Select(Path.GetFileNameWithoutExtension).ToArray();
            var atlasPaths = names.Select((name) => folderPath + Path.DirectorySeparatorChar + name);
            foreach (var atlasPath in atlasPaths)
            {
                Futile.atlasManager.LoadImage(atlasPath);
            }
            var thumbnails = atlasPaths.Select((atlas) => new OpImage(Vector2.zero, atlas)).ToArray();
            

            float y = yStart;
            for (int i = 0; i < thumbnails.Length; y-=imgHeight+45)
            {
                for (float x = 25f; x <= 525f && i < thumbnails.Length; x += 125f,i++)
                {
                    float newScale = imgHeight / thumbnails[i].size.y;
                    thumbnails[i].scale = new Vector2(newScale, newScale);
                    thumbnails[i].SetPos(new Vector2(x, y));

                    int separator = names[i].LastIndexOf(" - ");
                    if (separator <= 0)
                    {
                        continue;
                    }
                    string author = "by " + names[i].Substring(0, separator);
                    string title = '"' + names[i].Substring(separator + 3) + '"';

                    OpLabel titleLabel = new(new Vector2(x, y-20f), new Vector2(50f, 20f), title, FLabelAlignment.Center, false);
                    OpLabel authorLabel = new(new Vector2(x, y-35f), new Vector2(50f, 20f), author, FLabelAlignment.Center, false);
                    Tabs[tab].AddItems(new UIelement[]{ thumbnails[i], titleLabel, authorLabel });
                }
            }

            return y;
        }
    }
}