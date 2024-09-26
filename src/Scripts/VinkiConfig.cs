using Menu;
using Menu.Remix.MixedUI;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Vinki
{
    public class VinkiConfig : OptionInterface
    {
        public static VinkiConfig Instance { get; } = new();
        public static Configurable<bool> RequireCansGraffiti;
        public static Configurable<bool> RequireCansTagging;
        public static Configurable<bool> UpGraffiti;
        public static Configurable<int> GraffitiFadeTime;
        public static Configurable<bool> DeleteGraffiti;
        public static Configurable<bool> RestoreGraffitiOnUpdate;
        public static Configurable<bool> GlassesOverDMS;
        public static Configurable<bool> TagDamageJolly;
        public static Configurable<bool> UseGraffitiButton;
        public static Configurable<bool> TokensInEveryCampaign;
        public static Configurable<bool> AutoOpenMap;
        public static Configurable<bool> SkipIntro;

        private static OpHoldButton unlockButton;
        private static OpHoldButton lockButton;

        private int currentVinkiPage = 0;
        private int currentOtherPage = 0;

        public VinkiConfig()
        {
            RequireCansGraffiti = config.Bind("requireCansGraffiti", true, new ConfigurableInfo("Requires a spray can to spray graffiti on the background (craft a can with a rock and a colorful item).", tags:
            [
                "Require Spray Cans for Graffiti"
            ]));
            RequireCansTagging = config.Bind("requireCansTagging", true, new ConfigurableInfo("Requires a spray can to tag creatures (craft a can with a rock and a colorful item).", tags:
            [
                "Require Spray Cans for Tagging"
            ]));
            UseGraffitiButton = config.Bind("useGraffitiButton", true, new ConfigurableInfo("Use the Graffiti Mode button when crafting, spraying, and tagging. Disable if you have custom bindings and don't like having to press two buttons to perform these abilities.", tags:
            [
                "Require Graffiti Mode Button for Controls"
            ]));
            UpGraffiti = config.Bind("upGraffiti", true, new ConfigurableInfo("Use the Up direction for Graffiti Mode (in addition to the normal binding).", tags:
            [
                "Use Up as Graffiti Mode"
            ]));
            TagDamageJolly = config.Bind("tagDamageJolly", false, new ConfigurableInfo("Tagging a player while in Jolly Coop does damage.", tags:
            [
                "Tagging Damages Jolly Players"
            ]));
            TokensInEveryCampaign = config.Bind("tokensInEveryCampaign", false, new ConfigurableInfo("Be able to collect the unlockable graffiti tokens in campaigns besides Vinki's (excluding Saint)", tags:
            [
                "Graffiti Tokens in Any Campaign"
            ]));
            AutoOpenMap = config.Bind("autoOpenMap", true, new ConfigurableInfo("Automatically open the quest map after a cycle where you spray a new story graffiti. Disable if you want to speedrun", tags:
            [
                "Automatically Open Quest Map"
            ]));
            GlassesOverDMS = config.Bind("glassesOverDMS", true, new ConfigurableInfo("Wear Vinki's glasses on top of the current DMS skin for Vinki. Only works if you have DMS enabled.", tags:
            [
                "Wear Glasses Over DMS Skin"
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
            SkipIntro = config.Bind("skipIntro", false, new ConfigurableInfo("When starting a new game skip the campaign intro and tutorials and start at the top of The Wall.", tags:
            [
                "Skip Intro and Tutorials"
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
                new OpTab(this, "Other Graffiti"),
            ];

            // Options tab
            AddTitle(0, "Gameplay", 570f);
            AddCheckbox(RequireCansGraffiti, 540f);
            AddCheckbox(RequireCansTagging, 510f);
            AddCheckbox(UseGraffitiButton, 480f);
            AddCheckbox(UpGraffiti, 450f);
            AddCheckbox(TagDamageJolly, 420f);
            AddCheckbox(TokensInEveryCampaign, 390f);
            AddCheckbox(AutoOpenMap, 360f);
            AddCheckbox(SkipIntro, 330f);
            AddTitle(0, "Visuals", 275f);
            AddCheckbox(GlassesOverDMS, 245f);
            AddIntBox(GraffitiFadeTime, 215f);
            AddCheckbox(DeleteGraffiti, 185f);
            AddTitle(0, "Graffiti Files", 130f);
            AddCheckbox(RestoreGraffitiOnUpdate, 100f);
            AddButton(
                "Open Graffiti Folder",
                "Click to open the Graffiti Folder in your file explorer for easily adding custom graffiti",
                OpenGraffitiFolder,
                60f,
                200f,
                x: 200f
            );
            AddHoldButton(
                "Restore Default Graffiti",
                "Restore the default graffiti that came with The Vinki. Useful for after installing an update that includes new default graffiti.",
                RestoreDefaultGraffiti,
                25f,
                200f,
                40f,
                x: 50f
            );
            AddHoldButton(
                "Reset Graffiti Folder to Default",
                "Revert Graffiti Folder to default. This will remove any custom graffiti you've added to it!",
                ResetGraffitiFolder,
                25f,
                200f,
                color: Color.red,
                x: 350f
            );

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
            AddText(330f, "JayDee   TarnishedPotato    Xim", 1);
            AddSubtitle(290f, "Music", 1);
            AddText(270f, "MaxDubstep", 1);
            AddSubtitle(230f, "Sound Effects", 1);
            AddText(210f, "MaxDubstep", 1);
            AddSubtitle(170f, "Writing", 1);
            AddText(150f, "Beep    MaxDubstep    OlayColay    TarnishedPotato    Tsuno", 1);
            AddSubtitle(60f, "Special Thanks", 1);
            AddText(25f, "Developers of this mod's dependencies\n" +
                "Abigail    a doku    AxoTheAxolotl    Azura Hardware    banba fan   BreadwardBolero    BUGS    Doop    Eversplode    goof\n" +
                "Johnn    Nico    Rae    Repeat    Sadman    Salami_Hunter    skrybl    Sunbloom    SunnyBeam    TacticalBombs    Vinyla\n",
            1);

            // Vinki Graffiti tab
            RegenerateVinkiGraffitiPage();

            // Vinki Graffiti Unlockables tab
            AddGraffiti(550f, "decals" + Path.DirectorySeparatorChar + "Unlockables", 3, false, true);
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

            // Other Graffiti tab
            RegenerateOtherGraffitiPage();
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
        private void AddTitle(int tab, string text = "The Vinki", float yPos = 560f)
        {
            OpLabel title = new(new Vector2(150f, yPos), new Vector2(300f, 30f), text, bigText: true);

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
                new OpLabel(190f, y + 2f, Translate(optionText.info.Tags[0] as string), false)
                {
                    bumpBehav = opUpdown.bumpBehav,
                    description = opUpdown.description
                }
            ]);
        }

        private void AddPageButtons(int tab)
        {
            OpSimpleButton nextButton = new(new Vector2(350f, 0f), new Vector2(200f, 30f), Translate("Next Page"));
            OpSimpleButton prevButton = new(new Vector2(50f, 0f), new Vector2(200f, 30f), Translate("Last Page"));

            if (tab == 2)
            {
                int namesLength = Directory.EnumerateFiles(AssetManager.ResolveDirectory("decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "vinki"), "*.png", SearchOption.AllDirectories).Count();
                nextButton.OnClick += (_) =>
                {
                    currentVinkiPage++;
                    RegenerateVinkiGraffitiPage();
                };
                prevButton.OnClick += (_) =>
                {
                    currentVinkiPage--;
                    RegenerateVinkiGraffitiPage();
                };
                nextButton.greyedOut = currentVinkiPage * 30 + 30 >= namesLength;
                prevButton.greyedOut = currentVinkiPage == 0;
            }
            else if (tab == 4)
            {
                nextButton.OnClick += (_) =>
                {
                    currentOtherPage++;
                    RegenerateOtherGraffitiPage();
                };
                prevButton.OnClick += (_) =>
                {
                    currentOtherPage--;
                    RegenerateOtherGraffitiPage();
                };
                nextButton.greyedOut = currentOtherPage >= 3;
                prevButton.greyedOut = currentOtherPage == 0;
            }

            Tabs[tab].AddItems([
                nextButton,
                prevButton
            ]);
        }

        private OpSimpleButton AddButton(string displayName, string description, OnSignalHandler action, float y, float width, Color? color = null, int tab = 0, float x = 150f)
        {
            OpSimpleButton button = new(new Vector2(x, y), new Vector2(width, 30f), Translate(displayName))
            {
                description = Translate(description),
                colorEdge = color ?? MenuColorEffect.rgbMediumGrey,
            };
            button.OnClick += action;

            Tabs[tab].AddItems(
            [
                button
            ]);

            return button;
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
            Directory.Delete(Plugin.mainGraffitiFolder, true);
            RestoreDefaultGraffiti(trigger);
        }

        private void OpenGraffitiFolder(UIfocusable trigger)
        {
            ModManager.Mod mod = ModManager.ActiveMods.Find((mod) => mod.id == Plugin.MOD_ID);

            if (mod == null)
            {
                Plugin.VLogger.LogError("Couldn't find Vinki mod in list of active mods!");
            }

            Plugin.VLogger.LogMessage("Opening \"" + Plugin.mainGraffitiFolder + "\" in file explorer");
            if (Directory.Exists(Plugin.mainGraffitiFolder))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = Plugin.mainGraffitiFolder,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
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
        private float AddGraffiti(float yStart, string folderPath, int tab, bool hidden = false, bool unlockables = false)
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
            int currentPage = tab == 2 ? currentVinkiPage : 0;
            for (int i = currentPage * 30; i < currentPage * 30 + 30 && i < thumbnails.Length; y-=imgHeight+45)
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

        private void RegenerateVinkiGraffitiPage()
        {
            Tabs[2].RemoveItems([.. Tabs[2].items]);
            AddGraffiti(555f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "vinki", 2);
            AddPageButtons(2);
        }

        private void RegenerateOtherGraffitiPage()
        {
            Tabs[4].RemoveItems([.. Tabs[4].items]);
            float curY = 500f;
            switch (currentOtherPage)
            {
                case 0:
                    AddSubtitle(curY + 30f, "Monk", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Yellow", 4);
                    AddSubtitle(curY + 30f, "Survivor", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "White", 4);
                    AddSubtitle(curY + 30f, "Hunter", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Red", 4);
                    AddSubtitle(curY + 30f, "Watcher", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Night", 4);
                    break;
                case 1:
                    AddSubtitle(curY + 30f, "Artificer", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Artificer", 4);
                    AddSubtitle(curY + 30f, "Gourmand", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Gourmand", 4);
                    AddSubtitle(curY + 30f, "Rivulet", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Rivulet", 4);
                    AddSubtitle(curY + 30f, "Spearmaster", 4);
                    AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Spear", 4);
                    break;
                case 2:
                    AddSubtitle(curY + 30f, "Saint", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Saint", 4);
                    AddSubtitle(curY + 30f, "???", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Inv", 4, true);
                    AddSubtitle(curY + 30f, "Misc.", 4);
                    AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "Scenes", 4);
                    break;
                case 3:
                    AddSubtitle(curY + 30f, "Escort", 4);
                    curY = AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "EscortMe", 4);
                    AddSubtitle(curY + 30f, "Gravel-Eater", 4);
                    AddGraffiti(curY - 25f, "decals" + Path.DirectorySeparatorChar + "GraffitiBackup" + Path.DirectorySeparatorChar + "Gravelslug", 4);
                    break;
            }
            AddPageButtons(4);
        }
    }
}