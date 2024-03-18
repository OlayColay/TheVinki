using Menu;
using Menu.Remix.MixedUI;
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
        public static Configurable<bool> UseGraffitiButton;
        public static Configurable<bool> TokensInEveryCampaign;

        private static OpHoldButton unlockButton;
        private static OpHoldButton lockButton;

        public VinkiConfig()
        {
            RequireSprayCans = config.Bind("requireSprayCans", true, new ConfigurableInfo("Requires a spray can to spray graffiti (craft one with a rock and a colorful item).", tags:
            [
                "Require Spray Cans for Graffiti"
            ]));
            UpGraffiti = config.Bind("upGraffiti", true, new ConfigurableInfo("Use the Up direction for Graffiti Mode (in addition to the normal binding).", tags:
            [
                "Use Up as Graffiti Mode"
            ]));
            GraffitiFadeTime = config.Bind("graffitiFadeTime", 5, new ConfigurableInfo("How many cycles sprayed graffiti should last (excludes story-related graffiti). Use -1 for infinite cycles.", new ConfigAcceptableRange<int>(-1, 999), tags:
            [
                "Graffiti Display Cycles"
            ]));
            DeleteGraffiti = config.Bind("deleteGraffiti", false, new ConfigurableInfo("Delete Graffiti permanently when running out of display cycles. Will help with loading times if you've sprayed a lot of graffiti.", tags:
            [
                "Delete Graffiti Permanently After Display Cycles"
            ]));
            RestoreGraffitiOnUpdate = config.Bind("restoreGraffitiOnUpdate", true, new ConfigurableInfo("Restore default graffiti when the mod updates to a new version. Helpful to automatically add any new graffiti from updates.", tags:
            [
                "Restore Default Graffiti when Mod Updates"
            ]));
            ShowVinkiTitleCard = config.Bind("showVinkiTitleCard", true, new ConfigurableInfo("Always show one of the Vinki title cards when starting the game. This replaces other title cards, and the mod must be high in the mod order on the left to work!", tags:
            [
                "Always Show Vinki Title Cards"
            ]));
            GlassesOverDMS = config.Bind("glassesOverDMS", true, new ConfigurableInfo("Wear Vinki's glasses on top of the current DMS skin for Vinki. Only works if you have DMS enabled.", tags:
            [
                "Wear Glasses Over DMS Skin"
            ]));
            TagDamageJolly = config.Bind("tagDamageJolly", false, new ConfigurableInfo("Tagging a player while in Jolly Coop does damage.", tags:
            [
                "Tagging Damages Jolly Players"
            ]));
            UseGraffitiButton = config.Bind("useGraffitiButton", true, new ConfigurableInfo("Use the Graffiti Mode button when crafting, spraying, and tagging. Disable if you have custom bindings and don't like having to press two buttons to perform these abilities.", tags:
            [
                "Require Graffiti Mode Button for Controls"
            ]));
            TokensInEveryCampaign = config.Bind("tokensInEveryCampaign", false, new ConfigurableInfo("Be able to collect the unlockable graffiti tokens in campaigns besides Vinki's (excluding Saint)", tags:
            [
                "Graffiti Tokens in Any Campaign"
            ]));
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
            Tabs =
            [
                new OpTab(this, "Options"),
                new OpTab(this, "Credits"),
                new OpTab(this, "Vinki Graffiti"),
                new OpTab(this, "Unlockables"),
                new OpTab(this, "Other Graf. 1"),
                new OpTab(this, "Other Graf. 2"),
            ];

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
            AddCheckbox(UseGraffitiButton, 120f);
            AddCheckbox(TokensInEveryCampaign, 80f);

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
            AddText(330f, "TarnishedPotato    Xim", 1);
            AddSubtitle(290f, "Music", 1);
            AddText(270f, "MaxDubstep", 1);
            AddSubtitle(230f, "Sound Effects", 1);
            AddText(210f, "MaxDubstep", 1);
            AddSubtitle(170f, "Writing", 1);
            AddText(150f, "Beep    MaxDubstep    OlayColay    TarnishedPotato    Tsuno", 1);
            AddSubtitle(60f, "Special Thanks", 1);
            AddText(30f, "Developers of this mod's dependencies\n" +
                "Abigail    banba fan    Doop    goof    JayDee    Nico    Rae    Sadman    skrybl    Sunbloom    SunnyBeam\n",
            1);

            // Vinki Graffiti 1 tab
            AddSubtitle(580f, "Vinki", 2);
            AddGraffiti(525f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "vinki", 2);

            // Vinki Graffiti Unlockables tab
            AddGraffiti(550f, "decals" + Path.DirectorySeparatorChar + "Unlockables", 3, false, 0, true);
            unlockButton = AddHoldButton(
                "Unlock All Graffiti",
                "Unlock every graffiti. Useful if your game is bugged or you had to reset your Graffiti folder.",
                UnlockAllGraffiti,
                0f,
                200f,
                120f,
                color: Color.red,
                3,
                50f
            );
            unlockButton.greyedOut = Hooks.AllGraffitiUnlocked();
            lockButton = AddHoldButton(
                "Reset All Unlockables",
                "Reset all unlockable graffitis to be locked from use.",
                LockAllGraffiti,
                0f,
                200f,
                200f,
                color: Color.red,
                3,
                350f
            );
            lockButton.greyedOut = !Hooks.AnyGraffitiUnlocked();

            // Other Graffiti 1 tab
            AddSubtitle(580f, "Monk", 4);
            float curY = AddGraffiti(525f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Yellow", 4);
            AddSubtitle(curY + 30f, "Survivor", 4);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "White", 4);
            AddSubtitle(curY + 30f, "Hunter", 4);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Red", 4);
            AddSubtitle(curY + 30f, "Gourmand", 4);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Gourmand", 4);
            AddSubtitle(curY + 30f, "Artificer", 4);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Artificer", 4);

            // Other Graffiti 2 tab
            AddSubtitle(580f, "Rivulet", 5);
            curY = AddGraffiti(525f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Rivulet", 5);
            AddSubtitle(curY + 30f, "Spearmaster", 5);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Spear", 5);
            AddSubtitle(curY + 30f, "Saint", 5);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Saint", 5);
            AddSubtitle(curY + 30f, "???", 5);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Inv", 5, true);
            AddSubtitle(curY + 30f, "Misc.", 5);
            curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "Scenes", 5);
        }

        // Combines two flipped 'LinearGradient200's together to make a fancy looking divider.
        private void AddDivider(float y, int tab = 0)
        {
            OpImage dividerLeft = new(new Vector2(300f, y), "LinearGradient200");
            dividerLeft.sprite.SetAnchor(0.5f, 0f);
            dividerLeft.sprite.rotation = 270f;

            OpImage dividerRight = new(new Vector2(300f, y), "LinearGradient200");
            dividerRight.sprite.SetAnchor(0.5f, 0f);
            dividerRight.sprite.rotation = 90f;

            Tabs[tab].AddItems(
            [
                dividerLeft,
                dividerRight
            ]);
        }

        // Adds the mod name to the interface.
        private void AddTitle(int tab = 0)
        {
            OpLabel title = new(new Vector2(150f, 560f), new Vector2(300f, 30f), "The Vinki", bigText: true);

            Tabs[tab].AddItems(
            [
                title
            ]);
        }

        // Adds a subtitle to the interface.
        private void AddSubtitle(float y, string text, int tab = 0)
        {
            OpLabel title = new(new Vector2(200f, y), new Vector2(200f, 20f), text, bigText: true);

            Tabs[tab].AddItems(
            [
                title
            ]);
        }

        // Adds small text to the interface.
        private void AddText(float y, string text, int tab = 0)
        {
            OpLabel title = new(new Vector2(250f, y), new Vector2(100f, 10f), text);

            Tabs[tab].AddItems(
            [
                title
            ]);
        }

        // Adds a checkbox tied to the config setting passed through `optionText`, as well as a label next to it with a description.
        private void AddCheckbox(Configurable<bool> optionText, float y)
        {
            OpCheckBox checkbox = new(optionText, new Vector2(150f, y))
            {
                description = optionText.info.description
            };

            OpLabel checkboxLabel = new(150f + 40f, y + 2f, optionText.info.Tags[0] as string)
            {
                description = optionText.info.description
            };

            Tabs[0].AddItems(
            [
                checkbox,
                checkboxLabel
            ]);
        }

        private void AddIntBox(Configurable<int> optionText, float y)
        {
            OpUpdown opUpdown = new(optionText, new Vector2(100f, y - 4f), 75f)
            {
                description = Translate(optionText.info.description)
            };
            //if (uifocusable != null)
            //{
            //    UIfocusable.MutualVerticalFocusableBind(uifocusable, opUpdown);
            //}
            opUpdown.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
            opUpdown.SetNextFocusable(UIfocusable.NextDirection.Right, opUpdown);
            Tabs[0].AddItems(
            [
                opUpdown
            ]);
            //uifocusable = opUpdown;
            Tabs[0].AddItems(
            [
                new OpLabel(190f, y + 0f, Translate(optionText.info.Tags[0] as string), false)
                {
                    bumpBehav = opUpdown.bumpBehav,
                    description = opUpdown.description
                }
            ]);
        }

        private OpHoldButton AddHoldButton(string displayName, string description, OnSignalHandler action, float y, float width, float fillTime = 80f, Color? color = null, int tab = 0, float x = 150f)
        {
            OpHoldButton holdButton = new(new Vector2(x, y), new Vector2(width, 30f), Translate(displayName), fillTime)
            {
                description = Translate(description),
                colorEdge = color ?? MenuColorEffect.rgbMediumGrey,
            };
            holdButton.OnPressDone += action;

            Tabs[tab].AddItems(
            [
                holdButton
            ]);

            return holdButton;
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
            Hooks.AddGraffitiObjectives();
        }

        private void ResetGraffitiFolder(UIfocusable trigger)
        {
            Directory.Delete(Plugin.graffitiFolder, true);
            RestoreDefaultGraffiti(trigger);
        }

        private void UnlockAllGraffiti(UIfocusable trigger)
        {
            // Show the lock button
            unlockButton.greyedOut = true;
            lockButton.greyedOut = false;
            unlockButton._filled = 0f;

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(AssetManager.ResolveDirectory("decals/Unlockables"), "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(AssetManager.ResolveDirectory("decals/Unlockables"), AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki")), true);
            }
            Hooks.LoadGraffiti();
            Hooks.AddGraffitiObjectives();

            foreach (OpImage img in Tabs[3].items.Where((item) => item is OpImage).Cast<OpImage>())
            {
                img.color = Color.white;
            }
        }

        private void LockAllGraffiti(UIfocusable trigger)
        {
            // Show the unlock button
            unlockButton.greyedOut = false;
            lockButton.greyedOut = true;
            lockButton._filled = 0f;

            string folderPath = AssetManager.ResolveDirectory("decals/VinkiGraffiti/vinki");
            // Get all the filenames from Unlockables
            var unlockables = Directory.EnumerateFiles(AssetManager.ResolveDirectory("decals/Unlockables"), "*.*", SearchOption.AllDirectories).Select(Path.GetFileName);
            // Remove all the files that were unlocked
            foreach (string fileName in unlockables)
            {
                string filePath = Path.Combine(folderPath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            var graffitis = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories).Select(Path.GetFileName);
            // Remove added files from other mods
            foreach (string fileName in graffitis)
            {
                if (!fileName.Contains(" - "))
                {
                    File.Delete(Path.Combine(folderPath, fileName));
                }
            }
            Hooks.LoadGraffiti();
            Hooks.AddGraffitiObjectives();

            foreach (OpImage img in Tabs[3].items.Where((item) => item is OpImage).Cast<OpImage>())
            {
                if (img.sprite.element.name != "Tsuno - 一True Victory一")
                {
                    img.color = Color.black;
                }
            }
        }

        private static readonly int imgHeight = 50;
        private float AddGraffiti(float yStart, string folderPath, int tab, bool hidden = false, int startAt = 0, bool unlockables = false)
        {
            var names = Directory.EnumerateFiles(AssetManager.ResolveDirectory(folderPath), "*.png", SearchOption.AllDirectories).ToArray()
                .Select(Path.GetFileNameWithoutExtension).ToArray();
            var atlasPaths = names.Select((name) => folderPath + Path.DirectorySeparatorChar + name);
            foreach (var atlasPath in atlasPaths)
            {
                Futile.atlasManager.LoadImage(atlasPath);
            }
            OpImage[] thumbnails;

            if (hidden)
            {
                thumbnails = atlasPaths.Select((_) => new OpImage(Vector2.zero, "decals/QUESTIONMARK")).ToArray();
            }
            else
            {
                thumbnails = atlasPaths.Select((atlas) => new OpImage(Vector2.zero, atlas)).ToArray();
            }

            // Get array of graffitis that are already in the VinkiGraffiti folder
            var vinkiNames = Directory.EnumerateFiles(AssetManager.ResolveDirectory("decals" + Path.DirectorySeparatorChar + "VinkiGraffiti" + Path.DirectorySeparatorChar + "vinki"), "*.png", SearchOption.AllDirectories).ToArray()
                .Select(Path.GetFileNameWithoutExtension).ToArray();

            float y = yStart;
            for (int i = startAt; i < startAt + 30 && i < thumbnails.Length; y-=imgHeight+45)
            {
                for (float x = 25f; x <= 525f && i < thumbnails.Length; x += 125f,i++)
                {
                    float newScale = imgHeight / thumbnails[i].size.y;
                    thumbnails[i].scale = new Vector2(newScale, newScale);
                    thumbnails[i].SetPos(new Vector2(x + 25f, y + 25f));
                    thumbnails[i].anchor = new Vector2(0.5f, 0.5f);

                    // Hide graffiti if not unlocked yet
                    if (unlockables)
                    {
                        thumbnails[i].color = (vinkiNames.Contains(names[i]) || names[i] == "Tsuno - 一True Victory一") ? Color.white : Color.black;
                    }

                    int separator = names[i].LastIndexOf(" - ");
                    if (separator <= 0)
                    {
                        continue;
                    }
                    string author = "by " + names[i].Substring(0, separator);
                    string title = '"' + names[i].Substring(separator + 3) + '"';

                    OpLabel titleLabel = new(new Vector2(x, y-20f), new Vector2(50f, 20f), hidden ? "???" : title, FLabelAlignment.Center, false);
                    OpLabel authorLabel = new(new Vector2(x, y-35f), new Vector2(50f, 20f), author, FLabelAlignment.Center, false);
                    Tabs[tab].AddItems([thumbnails[i], titleLabel, authorLabel]);
                }
            }

            return y;
        }
    }
}